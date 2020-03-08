using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
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
            Randomizer.Go(OnComplete);
        }

        private void OnComplete()
        {
            this.Invoke(() =>
            {
                goContent.Content = GoButton;
                MessageBox.Show(this, "Done!", "Maelstrom");
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
