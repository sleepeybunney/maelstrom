using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace FF8Mod.Maelstrom
{
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : MetroWindow
    {
        public NewWindow()
        {
            State.Load(App.Path + @"\settings.json");
            InitializeComponent();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State.Save(App.Path + @"\settings.json");
        }

        private void OnBrowse(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "Final Fantasy VIII Executable|ff8_en.exe;ffviii.exe",
                Title = "Select game location"
            };

            if (dialog.ShowDialog() == true)
            {
                GameLocation.Text = dialog.FileName;
                GameLocation.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }
    }
}
