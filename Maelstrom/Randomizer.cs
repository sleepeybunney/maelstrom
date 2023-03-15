using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.Maelstrom
{
    public static class Randomizer
    {
        public static void Go()
        {
            Debug.WriteLine("randomizer start");

            var settings = State.Current.Clone();
            Env.ExePath = settings.GameLocation;

            if (!File.Exists(Env.ExePath))
            {
                HandleExeException(Env.ExePath);
                return;
            }

            // update game version
            if (!DetectVersion(Env.ExePath))
            {
                HandleExeException(Env.ExePath);
                return;
            }

            // set region
            if (!Env.Remastered) settings.Language = Env.RegionCodeFromPath(Env.ExePath);
            Env.RegionCode = settings.Language;

            // generate new seed if not fixed
            if (!settings.SeedFixed) settings.SeedValue = (new Random().Next(-1, int.MaxValue) + 1).ToString();
            var seedString = settings.SeedValue;

            // update seed history
            if (State.Current.History == null) State.Current.History = new List<string>();
            State.Current.History.RemoveAll(s => s == seedString);
            State.Current.History.Insert(0, seedString);
            if (State.Current.History.Count > 50) State.Current.History = State.Current.History.Take(50).ToList();

            // use seed string's hash code as the seed (or parse if possible)
            if (!int.TryParse(seedString, out int seed))
            {
                seed = seedString.GetHashCode();
            }

            var spoilerFile = new SpoilerFile(settings);

            if (Env.Remastered) UnpackOnFirstRun();

            Parallel.Invoke
            (
                () => BattleOps(seed, spoilerFile, settings),
                () => FieldOps(seed, seedString, spoilerFile, settings),
                () => MenuOps(seed, spoilerFile, settings),
                () => MainOps(seed, spoilerFile, settings)
            );

            FinalOps(seed, seedString, spoilerFile, settings);
            if (Env.Remastered) RepackArchive();

            Debug.WriteLine("randomizer end");
        }

        // find and store the game version, returns true if successful
        public static bool DetectVersion(string path)
        {
            if (!File.Exists(path)) return false;
            var steamNames = new string[] { "ff8_en.exe", "ff8_fr.exe", "ff8_it.exe", "ff8_de.exe", "ff8_es.exe" };
            var exeFileName = Path.GetFileName(path).ToLower();
            if (exeFileName == "ffviii.exe") Env.Remastered = true;
            else if (steamNames.Contains(exeFileName)) Env.Remastered = false;
            else return false;
            return true;
        }

        private static void CreateOrRestoreArchiveBackup(string singlePath)
        {
            Debug.WriteLine("create or restore backup - " + singlePath);
            foreach (var f in new string[] { singlePath + ".fs", singlePath + ".fi", singlePath + ".fl" })
            {
                var backup = f + ".bak";
                if (!File.Exists(backup)) File.Copy(f, backup);
                else File.Copy(backup, f, true);
            }
        }

        private static List<string> WorkspaceFiles
        {
            get
            {
                var result = new List<string>()
                {
                    "field.fs",
                    "field.fi",
                    "field.fl",
                };

                foreach (var region in Env.RegionExts.Values)
                {
                    foreach (var ext in new string[] { "fs", "fi", "fl" })
                    {
                        foreach (var name in new string[] { "battle", "menu", "main" })
                        {
                            result.Add(string.Format("lang-{0}\\{1}.{2}", region, name, ext));
                        }
                    }
                }

                return result;
            }
        }

        private static string WorkspacePath
        {
            get { return Path.Combine(Path.GetDirectoryName(Env.MainZzzPath), "Data"); }
        }

        private static void UnpackOnFirstRun()
        {
            Debug.WriteLine("unpack archive - " + Env.MainZzzPath);
            Directory.CreateDirectory(WorkspacePath);

            foreach (var f in WorkspaceFiles)
            {
                var destPath = Path.Combine(WorkspacePath, f);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                if (!File.Exists(destPath))
                {
                    using (var source = new ArchiveStream(Env.MainZzzPath + @";data\" + f))
                    using (var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        source.CopyTo(dest);
                    }
                }
            }
        }

        private static void RepackArchive()
        {
            Debug.WriteLine("repack archive - " + Env.MainZzzPath);

            var filesToPack = new Dictionary<string, IEnumerable<byte>>();
            foreach (var f in WorkspaceFiles)
            {
                var sourcePath = Path.Combine(WorkspacePath, f);
                if (File.Exists(sourcePath))
                {
                    // todo: defer read so everything isn't in memory at once
                    var source = File.ReadAllBytes(sourcePath);
                    filesToPack.Add(@"data\" + f, source);
                }
            }

            new Zzz(Env.MainZzzPath).ReplaceFiles(filesToPack);

        }

        private static bool HandleFileException(string filePath)
        {
            Debug.WriteLine("file exception - " + filePath);
            var message = string.Format("Could not write to file: {0}{1}Make sure it exists and is not open in another program.{1}Click OK to retry. Cancelling this operation may leave your game in an unplayable state.", filePath, Environment.NewLine);
            var result = MessageBox.Show(message, "Maelstrom - Error", MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
        }

        private static void HandleExeException(string filePath)
        {
            Debug.WriteLine("file exception - " + filePath);
            var message = string.Format("Could not open file: {0}{1}Make sure the path is correct and that your version of the game is supported.", filePath, Environment.NewLine);
            MessageBox.Show(message, "Maelstrom - Error", MessageBoxButton.OK);
        }

        // update battle archive
        private static void BattleOps(int seed, SpoilerFile spoilerFile, State settings)
        {
            Debug.WriteLine("battle ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Env.BattlePath);
                    var battleSource = new NativeFileSource(Env.BattlePath);

                    // boss shuffle
                    if (settings.BossEnable)
                    {
                        var bossMap = Boss.Randomise(seed, settings);
                        if (settings.SpoilerFile) spoilerFile.AddBosses(bossMap);
                        Boss.Apply(battleSource, bossMap, settings, settings.RestrictUltimecia != "normal");
                    }

                    // loot shuffle
                    var drops = settings.LootDrops;
                    var steals = settings.LootSteals;
                    var draws = settings.LootDraws;

                    if (drops || steals || draws)
                    {
                        var shuffle = LootShuffle.Randomise(battleSource, seed, settings);
                        if (settings.SpoilerFile) spoilerFile.AddLoot(shuffle, drops, steals, draws);
                    }

                    if (settings.BossEnable || drops || steals || draws)
                    {
                        battleSource.Encode();
                    }

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Env.BattlePath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("battle ops end");
        }

        // update field archive (and af3dn.p)
        private static void FieldOps(int seed, string seedString, SpoilerFile spoilerFile, State settings)
        {
            Debug.WriteLine("field ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Env.FieldPath);
                    var fieldSource = new NativeFileSource(Env.FieldPath);
                    var unsavedChanges = false;

                    // apply free roam
                    if (settings.FreeRoam)
                    {
                        FreeRoam.Apply(fieldSource, seedString);
                        unsavedChanges = true;
                    }
                    else
                    {
                        FreeRoam.Remove();
                    }

                    // update final boss for boss shuffle
                    if (settings.BossEnable && settings.RestrictUltimecia != "normal")
                    {
                        Boss.UpdateFinalBossChamber(fieldSource);
                        unsavedChanges = true;
                    }

                    // apply card shuffle
                    if (settings.CardEnable)
                    {
                        var shuffle = CardShuffle.Shuffle(seed);
                        if (settings.SpoilerFile) spoilerFile.AddCards(shuffle);
                        CardShuffle.Apply(fieldSource, shuffle);
                        unsavedChanges = true;
                    }

                    // apply music shuffle
                    if (settings.MusicEnable && Env.RegionCode != "jp")
                    {
                        var shuffle = MusicShuffle.Randomise(seed, settings);
                        if (settings.SpoilerFile) spoilerFile.AddMusic(shuffle);
                        MusicShuffle.Apply(fieldSource, shuffle);
                        unsavedChanges = true;
                    }

                    // write to file
                    if (unsavedChanges)
                    {
                        fieldSource.Encode();
                    }

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Env.FieldPath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("field ops end");
        }

        // update menu archive
        private static void MenuOps(int seed, SpoilerFile spoilerFile, State settings)
        {
            Debug.WriteLine("menu ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Env.MenuPath);
                    var menuSource = new NativeFileSource(Env.MenuPath);

                    // preset names
                    if (settings.NameEnable && Env.RegionCode != "jp")
                    {
                        PresetNames.Apply(menuSource, settings);
                    }

                    // shop shuffle
                    if (settings.ShopEnable)
                    {
                        var shuffle = ShopShuffle.Randomise(seed, settings);
                        if (settings.SpoilerFile) spoilerFile.AddShops(shuffle);
                        ShopShuffle.Apply(menuSource, shuffle);
                    }

                    // draw point shuffle
                    if (!Env.Remastered)
                    {
                        if (settings.DrawPointEnable)
                        {
                            var shuffle = DrawPointShuffle.Randomise(seed, settings);
                            if (settings.SpoilerFile) spoilerFile.AddDrawPoints(shuffle);
                            DrawPointShuffle.Apply(shuffle);
                        }
                        else
                        {
                            DrawPointShuffle.RemovePatch(Env.ExePath);
                        }
                    }

                    // doomtrain
                    if (settings.DoomtrainEnable)
                    {
                        var shuffle = Doomtrain.Randomise(seed);
                        if (settings.SpoilerFile) spoilerFile.AddDoomtrain(shuffle);
                        Doomtrain.Apply(menuSource, shuffle);
                    }

                    // magic sort fix
                    if ((!Env.Remastered && settings.DrawPointEnable) || settings.LootDraws || settings.EmergencySpell)
                    {
                        MagicSortFix.Apply(menuSource);
                    }

                    if ((settings.NameEnable && Env.RegionCode != "jp") || settings.ShopEnable || (settings.DrawPointEnable && !Env.Remastered) || settings.DoomtrainEnable)
                    {
                        // shop prices fix
                        PriceFix.Apply(menuSource);

                        menuSource.Encode();
                    }

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Env.MenuPath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("menu ops end");
        }

        private static void MainOps(int seed, SpoilerFile spoilerFile, State settings)
        {
            Debug.WriteLine("main ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Env.MainPath);
                    var mainSource = new NativeFileSource(Env.MainPath);

                    // ability shuffle
                    if (settings.GfAbilitiesEnable)
                    {
                        var shuffle = AbilityShuffle.Randomise(mainSource, seed, settings);
                        if (settings.SpoilerFile) spoilerFile.AddAbilities(shuffle);
                    }

                    // emergency spell
                    if (settings.EmergencySpell)
                    {
                        EmergencySpell.Apply(mainSource);
                    }

                    if (CutNameFix.ApplicableRegions.Contains(Env.RegionCode))
                    {
                        CutNameFix.Apply(mainSource);
                    }

                    MagicDataFix.Apply(mainSource);
                    mainSource.Encode();

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Env.MainPath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("main ops end");
        }

        // update multiple files on 2nd pass
        private static void FinalOps(int seed, string seedString, SpoilerFile spoilerFile, State settings)
        {
            Debug.WriteLine("final ops start");
            while (true)
            {
                try
                {
                    // weapon shuffle
                    if (settings.UpgradeEnable)
                    {
                        var mainSource = new NativeFileSource(Env.MainPath);
                        var menuSource = new NativeFileSource(Env.MenuPath);

                        var shuffle = WeaponShuffle.Randomise(seed);
                        if (settings.SpoilerFile) spoilerFile.AddWeapons(mainSource, shuffle);
                        WeaponShuffle.Apply(menuSource, shuffle);

                        menuSource.Encode();
                    }

                    // free roam rewards
                    if (settings.FreeRoam || settings.BossEnable)
                    {
                        var battleSource = new NativeFileSource(Env.BattlePath);
                        var fieldSource = new NativeFileSource(Env.FieldPath);

                        if (settings.FreeRoam) Reward.SetRewards(battleSource, fieldSource, seed);
                        if (settings.BossEnable) Boss.ApplyEdeaFix(battleSource, fieldSource);

                        battleSource.Encode();
                        fieldSource.Encode();
                    }
                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Env.BattlePath) == false) break;
                    }
                    else throw;
                }
            }

            // save spoiler file
            if (settings.SpoilerFile)
            {
                // strip illegal chars from filename

                File.WriteAllText("spoilers." + SanitizeFileName(seedString) + ".txt", spoilerFile.ToString());
            }
            Debug.WriteLine("final ops end");
        }

        public static string SanitizeFileName(string fileName)
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var regex = new Regex(string.Format("[{0}]", Regex.Escape(invalidChars)));
            return regex.Replace(fileName, "_");
        }
    }
}
