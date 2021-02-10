using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FF8Mod.Archive;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace FF8Mod.Maelstrom
{
    public static class Randomizer
    {
        public static void Go(Action callback)
        {
            Debug.WriteLine("randomizer start");
            Globals.ExePath = State.Current.GameLocation;

            if (!File.Exists(Globals.ExePath))
            {
                HandleExeException(Globals.ExePath);
                callback.Invoke();
                return;
            }

            // update game version
            if (!DetectVersion(Globals.ExePath))
            {
                HandleExeException(Globals.ExePath);
                callback.Invoke();
                return;
            }

            // set region
            Globals.RegionCode = State.Current.Language;

            // generate new seed if not fixed
            if (!State.Current.SeedFixed) State.Current.SeedValue = (new Random().Next(-1, int.MaxValue) + 1).ToString();
            var seedString = State.Current.SeedValue;

            // update seed history
            if (State.Current.History == null) State.Current.History = new List<string>();
            State.Current.History.RemoveAll(s => s == seedString);
            State.Current.History.Insert(0, seedString);
            if (State.Current.History.Count > 50) State.Current.History = State.Current.History.Take(50).ToList();

            // use seed string's hash code as the seed (or parse if possible, for compatibility with 0.2.1 and earlier)
            if (!int.TryParse(seedString, out int seed))
            {
                seed = seedString.GetHashCode();
            }

            var spoilerFile = new SpoilerFile();

            Task.Run(() =>
            {
                if (Globals.Remastered) UnpackOnFirstRun();

                Parallel.Invoke
                (
                    () => BattleOps(seed, spoilerFile),
                    () => FieldOps(seed, seedString, spoilerFile),
                    () => MenuOps(seed, spoilerFile),
                    () => MainOps(seed, spoilerFile)
                );

                FinalOps(seed, seedString, spoilerFile);
                if (Globals.Remastered) RepackArchive();

                callback.Invoke();

                Debug.WriteLine("randomizer end");
            });
        }

        // find and store the game version, returns true if successful
        public static bool DetectVersion(string path)
        {
            var exeFileName = Path.GetFileName(path).ToLower();
            if (exeFileName == "ffviii.exe") Globals.Remastered = true;
            else if (exeFileName == "ff8_en.exe") Globals.Remastered = false;
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

                foreach (var region in Globals.RegionExts.Values)
                {
                    foreach (var ext in new string[] { "fs", "fi", "fl" })
                    {
                        foreach (var name in new string[] { "battle", "menu", "main"})
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
            get { return Path.Combine(Path.GetDirectoryName(Globals.MainZzzPath), "Data"); }
        }

        private static void UnpackOnFirstRun()
        {
            Debug.WriteLine("unpack archive - " + Globals.MainZzzPath);
            Directory.CreateDirectory(WorkspacePath);

            foreach (var f in WorkspaceFiles)
            {
                var destPath = Path.Combine(WorkspacePath, f);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                if (!File.Exists(destPath))
                {
                    using (var source = new ArchiveStream(Globals.MainZzzPath + @";data\" + f))
                    using (var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        source.CopyTo(dest);
                    }
                }
            }
        }

        private static void RepackArchive()
        {
            Debug.WriteLine("repack archive - " + Globals.MainZzzPath);

            var filesToPack = new Dictionary<string, byte[]>();
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

            new Zzz(Globals.MainZzzPath).ReplaceFiles(filesToPack);

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
        private static void BattleOps(int seed, SpoilerFile spoilerFile)
        {
            Debug.WriteLine("battle ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Globals.BattlePath);
                    var battleSource = new FileSource(Globals.BattlePath);

                    // boss shuffle
                    if (State.Current.BossEnable)
                    {
                        Dictionary<int, int> bossMap;
                        if (!State.Current.BossRandom) bossMap = Boss.Shuffle(seed);
                        else bossMap = Boss.Randomise(seed);

                        if (State.Current.SpoilerFile) spoilerFile.AddBosses(bossMap);
                        Boss.Apply(battleSource, bossMap);
                    }

                    // loot shuffle
                    var drops = State.Current.LootDrops;
                    var steals = State.Current.LootSteals;
                    var draws = State.Current.LootDraws;

                    if (drops || steals || draws)
                    {
                        var shuffle = LootShuffle.Randomise(battleSource, drops, steals, draws, seed);
                        if (State.Current.SpoilerFile) spoilerFile.AddLoot(shuffle, drops, steals, draws);
                    }

                    if (State.Current.BossEnable || drops || steals || draws)
                    {
                        battleSource.Encode();
                    }

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Globals.BattlePath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("battle ops end");
        }

        // update field archive (and af3dn.p)
        private static void FieldOps(int seed, string seedString, SpoilerFile spoilerFile)
        {
            Debug.WriteLine("field ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Globals.FieldPath);
                    var fieldSource = new FileSource(Globals.FieldPath);

                    // apply free roam
                    if (State.Current.FreeRoam)
                    {
                        StorySkip.Apply(fieldSource, seedString, seed);
                    }
                    else
                    {
                        StorySkip.Remove();
                    }

                    // apply card shuffle
                    if (State.Current.CardEnable)
                    {
                        var shuffle = CardShuffle.Shuffle(seed);
                        if (State.Current.SpoilerFile) spoilerFile.AddCards(shuffle);
                        CardShuffle.Apply(fieldSource, shuffle);
                    }

                    // apply music shuffle
                    if (State.Current.MusicEnable)
                    {
                        var shuffle = MusicShuffle.Randomise(seed, State.Current.MusicIncludeNonMusic);
                        if (State.Current.SpoilerFile) spoilerFile.AddMusic(shuffle);
                        MusicShuffle.Apply(fieldSource, shuffle);
                    }

                    // write to file
                    if (State.Current.FreeRoam || State.Current.CardEnable || State.Current.MusicEnable)
                    {
                        fieldSource.Encode();
                    }

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Globals.FieldPath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("field ops end");
        }

        // update menu archive
        private static void MenuOps(int seed, SpoilerFile spoilerFile)
        {
            Debug.WriteLine("menu ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Globals.MenuPath);
                    var menuSource = new FileSource(Globals.MenuPath);

                    // preset names
                    //if (State.Current.NameSet)
                    //{
                    //    PresetNames.Apply(menuSource);
                    //    menuSource.Encode();
                    //}

                    // shop shuffle
                    if (State.Current.ShopEnable)
                    {
                        var keyItems = State.Current.ShopKeyItems;
                        var summonItems = State.Current.ShopSummonItems;
                        var magazines = State.Current.ShopMagazines;
                        var chocoboWorld = State.Current.ShopChocoboWorld;

                        var shuffle = ShopShuffle.Randomise(seed, keyItems, summonItems, magazines, chocoboWorld);
                        if (State.Current.SpoilerFile) spoilerFile.AddShops(shuffle);
                        ShopShuffle.Apply(menuSource, shuffle);
                        menuSource.Encode();
                    }

                    // draw point shuffle
                    if (!Globals.Remastered)
                    {
                        if (State.Current.DrawPointEnable)
                        {
                            var apoc = State.Current.DrawPointIncludeApoc;
                            var slot = State.Current.DrawPointIncludeSlot;
                            var cut = State.Current.DrawPointIncludeCut;

                            var shuffle = DrawPointShuffle.Randomise(apoc, slot, cut, seed);
                            if (State.Current.SpoilerFile) spoilerFile.AddDrawPoints(shuffle);
                            DrawPointShuffle.Apply(menuSource, shuffle);
                            menuSource.Encode();
                        }
                        else
                        {
                            DrawPointShuffle.RemovePatch(Globals.ExePath);
                        }
                    }

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Globals.MenuPath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("menu ops end");
        }

        private static void MainOps(int seed, SpoilerFile spoilerFile)
        {
            Debug.WriteLine("main ops start");
            while (true)
            {
                try
                {
                    CreateOrRestoreArchiveBackup(Globals.MainPath);
                    var mainSource = new FileSource(Globals.MainPath);

                    // ability shuffle
                    if (State.Current.GfAbilitiesEnable)
                    {
                        var abilityShuffle = AbilityShuffle.Randomise(mainSource, seed, State.Current.GfAbilitiesBasics, State.Current.GfAbilitiesIncludeItemOnly);
                        if (State.Current.SpoilerFile) spoilerFile.AddAbilities(abilityShuffle);
                    }

                    mainSource.Encode();
                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Globals.MainPath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("main ops end");
        }

        // update multiple files on 2nd pass
        private static void FinalOps(int seed, string seedString, SpoilerFile spoilerFile)
        {
            Debug.WriteLine("final ops start");
            while (true)
            {
                try
                {
                    // free roam rewards
                    if (State.Current.FreeRoam || State.Current.BossEnable)
                    {
                        var battleSource = new FileSource(Globals.BattlePath);
                        var fieldSource = new FileSource(Globals.FieldPath);

                        if (State.Current.FreeRoam) Reward.SetRewards(battleSource, fieldSource, seed);
                        if (State.Current.BossEnable) Boss.ApplyEdeaFix(battleSource, fieldSource);

                        battleSource.Encode();
                        fieldSource.Encode();
                    }
                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Globals.BattlePath) == false) break;
                    }
                    else throw;
                }
            }

            // save spoiler file
            if (State.Current.SpoilerFile)
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
