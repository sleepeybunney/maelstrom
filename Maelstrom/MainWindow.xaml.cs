using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using FF8Mod.Archive;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Windows.Controls;

namespace FF8Mod.Maelstrom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            GoButton.Click += OnGo;
            goContent.Content = GoButton;
        }

        static Button GoButton = new Button()
        {
            Margin = new Thickness(0, 0, 10, 0),
            Content = "GO"
        };

        static ProgressRing Spinner = new ProgressRing()
        {
            IsActive = true,
            Width = 22,
            Height = 22,
            MinWidth = 22,
            MinHeight = 22,
            Foreground = Brushes.White
        };

        private void OnGo(object sender, RoutedEventArgs e)
        {
            goContent.Content = Spinner;

            if (!Properties.Settings.Default.SeedSet) Properties.Settings.Default.SeedValue = new Random().Next(-1, int.MaxValue) + 1;
            var seed = Properties.Settings.Default.SeedValue;

            var gameLocation = Path.GetDirectoryName(Properties.Settings.Default.GameLocation);
            var dataPath = Path.Combine(gameLocation, "data", "lang-en");
            var battlePath = Path.Combine(dataPath, "battle");
            var fieldPath = Path.Combine(dataPath, "field");
            var af3dn = Path.Combine(gameLocation, "AF3DN.P");

            var spoilerFile = new SpoilerFile();

            Task.Run(() =>
            {
                Parallel.Invoke(() =>
                {
                    // shuffle/rebalance bosses
                    while (true)
                    {
                        try
                        {
                            CreateOrRestoreArchiveBackup(battlePath);

                            if (Properties.Settings.Default.BossShuffle)
                            {
                                var battleSource = new FileSource(battlePath);
                                var shuffle = Boss.Shuffle(battleSource, Properties.Settings.Default.BossRebalance, seed);
                                if (Properties.Settings.Default.SpoilerFile) spoilerFile.AddBosses(shuffle);
                                battleSource.Encode();
                            }

                            break;
                        }
                        catch (Exception x)
                        {
                            if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                            {
                                if (HandleFileException(battlePath) == false) break;
                            }
                            else throw;
                        }
                    }
                },

                () =>
                {
                    // skip story scenes
                    while (true)
                    {
                        try
                        {
                            CreateOrRestoreArchiveBackup(fieldPath);

                            if (Properties.Settings.Default.StorySkip)
                            {
                                var fieldSource = new FileSource(fieldPath);
                                StorySkip.Apply(fieldSource, af3dn, seed);
                                fieldSource.Encode();
                            }
                            else
                            {
                                StorySkip.Remove(af3dn);
                            }

                            break;
                        }
                        catch (Exception x)
                        {
                            if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                            {
                                if (HandleFileException(fieldPath) == false) break;
                            }
                            else throw;
                        }
                    }
                },

                () =>
                {
                    // preset names
                    var menuPath = Path.Combine(dataPath, "menu");

                    while (true)
                    {
                        try
                        {
                            CreateOrRestoreArchiveBackup(menuPath);
                            var menuSource = new FileSource(menuPath);

                            if (Properties.Settings.Default.NameSet)
                            {
                                PresetNames.Apply(menuSource);
                                menuSource.Encode();
                            }

                            if (Properties.Settings.Default.ShopShuffle)
                            {
                                ShopShuffle.Apply(menuSource, seed);
                                menuSource.Encode();
                            }

                            break;
                        }
                        catch (Exception x)
                        {
                            if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                            {
                                if (HandleFileException(menuPath) == false) break;
                            }
                            else throw;
                        }
                    }
                },

                () =>
                {
                    // shuffle draw points
                    while (true)
                    {
                        try
                        {
                            if (Properties.Settings.Default.DrawPointShuffle)
                            {
                                DrawPointShuffle.GeneratePatch(seed).Apply(Properties.Settings.Default.GameLocation);
                            }
                            else
                            {
                                DrawPointShuffle.GeneratePatch(seed).Remove(Properties.Settings.Default.GameLocation);
                            }

                            break;
                        }
                        catch (Exception x)
                        {
                            if (x is IOException || x is UnauthorizedAccessException || x is FileNotFoundException)
                            {
                                if (HandleFileException(Properties.Settings.Default.GameLocation) == false) break;
                            }
                            else throw;
                        }
                    }
                });

                while (true)
                {
                    try
                    {
                        if (Properties.Settings.Default.StorySkip)
                        {
                            var battleSource = new FileSource(battlePath);
                            var fieldSource = new FileSource(fieldPath);
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
                            if (HandleFileException(battlePath) == false) break;
                        }
                        else throw;
                    }
                }

                this.Invoke(() =>
                {
                    if (Properties.Settings.Default.SpoilerFile)
                    {
                        File.WriteAllText("spoilers." + seed.ToString() + ".txt", spoilerFile.ToString());
                    }

                    GC.Collect();
                    goContent.Content = GoButton;
                    MessageBox.Show(this, "Done!", "Maelstrom");
                });
            });
        }

        private void CreateOrRestoreArchiveBackup(string singlePath)
        {
            foreach (var f in new string[] { singlePath + ".fs", singlePath + ".fi", singlePath + ".fl" })
            {
                var backup = f + ".bak";
                if (!File.Exists(backup)) File.Copy(f, backup);
                else File.Copy(backup, f, true);
            }
        }

        private bool HandleFileException(string filePath)
        {
            var message = string.Format("Could not write to file: {0}{1}Make sure it exists and is not open in another program.{1}Click OK to retry. Cancelling this operation may leave your game in an unplayable state.", filePath, Environment.NewLine);
            var result = MessageBox.Show(message, "Maelstrom - Error", MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
        }

        private void OnBrowse(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "Final Fantasy VIII Executable|ff8_en.exe",
                Title = "Select game location"
            };

            if (dialog.ShowDialog() == true)
            {
                Properties.Settings.Default.GameLocation = dialog.FileName;
            }
        }

        private void OnSetNames(object sender, RoutedEventArgs e)
        {
            new NameWindow().ShowDialog();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
