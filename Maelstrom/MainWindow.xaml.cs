using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using FF8Mod.Archive;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using System.Windows.Media;

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
            goContent.Content = "GO";
        }

        private void OnGo(object sender, RoutedEventArgs e)
        {
            goButton.IsEnabled = false;
            goContent.Content = new ProgressRing()
            {
                IsActive = true,
                Width = 16,
                Height = 16,
                MinWidth = 16,
                MinHeight = 16,
                Foreground = Brushes.White
            };

            var seed = Properties.Settings.Default.SeedSet ? Properties.Settings.Default.SeedValue : new Random().Next(-1, int.MaxValue) + 1;

            var gameLocation = Path.GetDirectoryName(Properties.Settings.Default.GameLocation);
            var dataPath = Path.Combine(gameLocation, "data", "lang-en");
            var af3dn = Path.Combine(gameLocation, "AF3DN.P");

            Task.Run(() =>
            {
                Parallel.Invoke(() =>
                {
                    // shuffle/rebalance bosses
                    if (Properties.Settings.Default.BossShuffle)
                    {
                        var battlePath = Path.Combine(dataPath, "battle");
                        var battleSource = new FileSource(battlePath);
                        Boss.Shuffle(battleSource, Properties.Settings.Default.BossRebalance, seed);
                    }
                },

                () =>
                {
                    // skip story scenes
                    if (Properties.Settings.Default.StorySkip)
                    {
                        var fieldPath = Path.Combine(dataPath, "field");
                        var fieldSource = new FileSource(fieldPath);
                        StorySkip.Apply(fieldSource, af3dn);
                    }
                    else
                    {
                        StorySkip.Remove(af3dn);
                    }
                },

                () =>
                {
                    // shuffle draw points
                    if (Properties.Settings.Default.DrawPointShuffle) DrawPointShuffle.GeneratePatch(seed).Apply(Properties.Settings.Default.GameLocation);
                    else DrawPointShuffle.GeneratePatch(seed).Remove(Properties.Settings.Default.GameLocation);
                });

                MessageBox.Show("Done!", "Maelstrom");

                goButton.Invoke(() =>
                {
                    goButton.IsEnabled = true;
                    goContent.Content = "GO";
                });
            });
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

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
