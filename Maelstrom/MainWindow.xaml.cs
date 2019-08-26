using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using FF8Mod.Archive;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;

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

            var seed = Properties.Settings.Default.SeedSet ? Properties.Settings.Default.SeedValue : new Random().Next(-1, int.MaxValue) + 1;

            var gameLocation = Path.GetDirectoryName(Properties.Settings.Default.GameLocation);
            var dataPath = Path.Combine(gameLocation, "data", "lang-en");
            var af3dn = Path.Combine(gameLocation, "AF3DN.P");

            Task.Run(() =>
            {
                Parallel.Invoke(() =>
                {
                    // shuffle/rebalance bosses
                    var battlePath = Path.Combine(dataPath, "battle");
                    CreateOrRestoreArchiveBackup(battlePath);

                    if (Properties.Settings.Default.BossShuffle)
                    {
                        var battleSource = new FileSource(battlePath);
                        Boss.Shuffle(battleSource, Properties.Settings.Default.BossRebalance, seed);
                        battleSource.Encode();
                    }
                },

                () =>
                {
                    // skip story scenes
                    var fieldPath = Path.Combine(dataPath, "field");
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
                },

                () =>
                {
                    // preset names
                    var menuPath = Path.Combine(dataPath, "menu");
                    CreateOrRestoreArchiveBackup(menuPath);

                    if (Properties.Settings.Default.NameSet)
                    {
                        var menuSource = new FileSource(menuPath);
                        PresetNames.Apply(menuSource);
                        menuSource.Encode();
                    }
                },

                () =>
                {
                    // shuffle draw points
                    if (Properties.Settings.Default.DrawPointShuffle)
                    {
                        DrawPointShuffle.GeneratePatch(seed).Apply(Properties.Settings.Default.GameLocation);
                    }
                    else
                    {
                        DrawPointShuffle.GeneratePatch(seed).Remove(Properties.Settings.Default.GameLocation);
                    }
                });

                this.Invoke(() =>
                {
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
