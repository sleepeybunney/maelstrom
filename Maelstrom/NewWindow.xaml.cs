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
using System.Globalization;

namespace FF8Mod.Maelstrom
{
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : MetroWindow
    {
        public NewWindow()
        {
            // load user settings
            State.Current = State.LoadFile(App.Path + @"\settings.json", false);
            DataContext = State.Current;

            // load presets
            State.Presets = FetchPresets();

            InitializeComponent();
        }

        private List<State> FetchPresets()
        {
            var result = new List<State>();
            var presetsPath = Path.Combine(App.Path, "Presets");
            if (!Directory.Exists(presetsPath)) Directory.CreateDirectory(presetsPath);

            foreach (var file in Directory.GetFiles(presetsPath, "*.json"))
            {
                result.Add(State.LoadFile(file));
            }

            return result;
        }

        private void SaveNewPreset()
        {
            var presetsPath = Path.Combine(App.Path, "Presets");
            if (!Directory.Exists(presetsPath)) Directory.CreateDirectory(presetsPath);
            State.Current.PresetName = SaveName.Text;
            State.SaveFile(Path.Combine(presetsPath, Randomizer.SanitizeFileName(SaveName.Text) + ".json"), true);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State.SaveFile(App.Path + @"\settings.json", false);
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

        private void OnSelectPreset(object sender, SelectionChangedEventArgs e)
        {
            if (PresetList.SelectedItem == null) LoadDescription.Text = "";
            else LoadDescription.Text = PresetList.SelectedItem.ToString();
        }

        private void OnLoadPreset(object sender, RoutedEventArgs e)
        {
            if (PresetList.SelectedItem == null) return;
            State.Current = State.LoadState((State)PresetList.SelectedItem, true);

            // refresh binding
            DataContext = State.Current;
        }

        private void OnSavePreset(object sender, RoutedEventArgs e)
        {
            if (SaveName.Text == "") return;
            try
            {
                SaveNewPreset();
                State.Presets = FetchPresets();
                PresetList.SelectedItem = null;
                PresetList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget();
            }
            catch (Exception x) { }
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ListBox && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }

    public class TitleConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value[0] == null ? "null" : value[0].ToString();
            var lang = value[1] == null ? "null" : value[1].ToString();

            if (!File.Exists(path) || !Randomizer.DetectVersion(path)) return "No Game Loaded!";
            if (Globals.Remastered) return "Final Fantasy VIII Remastered (" + lang.ToUpper() + " 2019)";
            return "Final Fantasy VIII (" + lang.ToUpper() + " 2013)";
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
