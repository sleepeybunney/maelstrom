using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FF8Mod.Archive;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FF8Mod.Maelstrom
{
    public static class Randomizer
    {
        public static void Go(Action callback)
        {
            Debug.WriteLine("randomizer start");
            Globals.ExePath = Properties.Settings.Default.GameLocation;

            // detect game version
            if (Path.GetFileName(Globals.ExePath).ToLower() == "ffviii.exe") Globals.Remastered = true;
            else Globals.Remastered = false;

            // generate new seed if not fixed
            if (!Properties.Settings.Default.SeedSet) Properties.Settings.Default.SeedValue = (new Random().Next(-1, int.MaxValue) + 1).ToString();
            var seedString = Properties.Settings.Default.SeedValue;

            // use seed string's hash code as the seed (or parse if possible, for compatibility with 0.2.1 and earlier)
            if (!int.TryParse(seedString, out int seed))
            {
                seed = seedString.GetHashCode();
            }

            var spoilerFile = new SpoilerFile();

            Task.Run(() =>
            {
                Parallel.Invoke
                (
                    () => BattleOps(seed, spoilerFile),
                    () => FieldOps(seed, seedString, spoilerFile),
                    () => MenuOps(seed, spoilerFile),
                    () => ExeOps(seed, spoilerFile),
                    () => MainOps(seed, spoilerFile)
                );

                FinalOps(seed, seedString, spoilerFile);
                callback.Invoke();

                Debug.WriteLine("randomizer end");
            });
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

        private static bool HandleFileException(string filePath)
        {
            Debug.WriteLine("file exception - " + filePath);
            var message = string.Format("Could not write to file: {0}{1}Make sure it exists and is not open in another program.{1}Click OK to retry. Cancelling this operation may leave your game in an unplayable state.", filePath, Environment.NewLine);
            var result = MessageBox.Show(message, "Maelstrom - Error", MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
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

                    // boss shuffle/rebalance
                    if (Properties.Settings.Default.BossShuffle)
                    {
                        var shuffle = Boss.Shuffle(battleSource, Properties.Settings.Default.BossRebalance, seed);
                        if (Properties.Settings.Default.SpoilerFile) spoilerFile.AddBosses(shuffle);
                    }

                    // loot shuffle
                    if (Properties.Settings.Default.LootShuffle)
                    {
                        var shuffle = LootShuffle.Randomise(battleSource, seed);
                        if (Properties.Settings.Default.SpoilerFile) spoilerFile.AddLoot(shuffle);
                    }

                    if (Properties.Settings.Default.BossShuffle || Properties.Settings.Default.LootShuffle)
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
                    if (Properties.Settings.Default.StorySkip)
                    {
                        StorySkip.Apply(fieldSource, seedString, seed);
                    }
                    else
                    {
                        StorySkip.Remove();
                    }

                    // apply card shuffle
                    if (Properties.Settings.Default.CardShuffle)
                    {
                        var shuffle = CardShuffle.Shuffle(seed);
                        if (Properties.Settings.Default.SpoilerFile) spoilerFile.AddCards(shuffle);
                        CardShuffle.Apply(fieldSource, shuffle);
                    }

                    // write to file
                    if (Properties.Settings.Default.StorySkip || Properties.Settings.Default.CardShuffle)
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
                    if (Properties.Settings.Default.NameSet)
                    {
                        PresetNames.Apply(menuSource);
                        menuSource.Encode();
                    }

                    // shop shuffle
                    if (Properties.Settings.Default.ShopShuffle)
                    {
                        var shuffle = ShopShuffle.Randomise(seed);
                        if (Properties.Settings.Default.SpoilerFile) spoilerFile.AddShops(shuffle);
                        ShopShuffle.Apply(menuSource, shuffle);
                        menuSource.Encode();
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

        // update game executable
        private static void ExeOps(int seed, SpoilerFile spoilerFile)
        {
            Debug.WriteLine("exe ops start");
            while (true)
            {
                try
                {
                    // draw point shuffle
                    if (Properties.Settings.Default.DrawPointShuffle)
                    {
                        var shuffle = DrawPointShuffle.Randomise(seed);
                        if (Properties.Settings.Default.SpoilerFile) spoilerFile.AddDrawPoints(shuffle);
                        DrawPointShuffle.GeneratePatch(shuffle).Apply(Globals.ExePath);
                    }
                    else
                    {
                        DrawPointShuffle.RemovePatch(Globals.ExePath);
                    }

                    break;
                }
                catch (Exception x)
                {
                    if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                    {
                        if (HandleFileException(Globals.ExePath) == false) break;
                    }
                    else throw;
                }
            }
            Debug.WriteLine("exe ops end");
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
                    if (Properties.Settings.Default.AbilityShuffle)
                    {
                        AbilityShuffle.Randomise(mainSource, seed);
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
                    if (Properties.Settings.Default.StorySkip)
                    {
                        var battleSource = new FileSource(Globals.BattlePath);
                        var fieldSource = new FileSource(Globals.FieldPath);
                        Reward.SetRewards(battleSource, fieldSource, seed);
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
            if (Properties.Settings.Default.SpoilerFile)
            {
                // strip illegal chars from filename
                var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                var regex = new Regex(string.Format("[{0}]", Regex.Escape(invalidChars)));
                File.WriteAllText("spoilers." + regex.Replace(seedString, "_") + ".txt", spoilerFile.ToString());
            }
            Debug.WriteLine("final ops end");
        }
    }
}
