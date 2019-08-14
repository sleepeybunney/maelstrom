using System;
using System.Linq;
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
                var gameDir = Path.GetDirectoryName(gameDirText.Text);
                var dataPath = Path.Combine(gameDir, "data", "lang-en");
                if (optionBossShuffle.IsChecked == true)
                {
                    var battlePath = Path.Combine(dataPath, "battle");
                    var battleSource = new FileSource(battlePath);
                    Boss.Shuffle(battleSource);
                }
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
                Properties.Settings.Default.GameDir = dialog.FileName;
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
