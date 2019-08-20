using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

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
        }

        private void OnGo(object sender, RoutedEventArgs e)
        {
            goButton.IsEnabled = false;

            try
            {
                var gameLocation = Path.GetDirectoryName(Properties.Settings.Default.GameLocation);
                var dataPath = Path.Combine(gameLocation, "data", "lang-en");
                var af3dn = Path.Combine(gameLocation, "AF3DN.P");

                // shuffle/rebalance bosses
                if (Properties.Settings.Default.BossShuffle)
                {
                    var battlePath = Path.Combine(dataPath, "battle");
                    var battleSource = new FileSource(battlePath);
                    Boss.Shuffle(battleSource, Properties.Settings.Default.BossRebalance);
                }

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

                // shuffle draw points
                if (Properties.Settings.Default.DrawPointShuffle) DrawPointShuffle.Patch.Apply();
                else DrawPointShuffle.Patch.Remove();

                MessageBox.Show("Done!", "Maelstrom");

            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message, "Maelstrom - Error");
            }

            goButton.IsEnabled = true;
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
