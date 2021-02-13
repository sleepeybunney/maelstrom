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
using MahApps.Metro.Controls.Dialogs;

namespace FF8Mod.Maelstrom
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            // load user settings & presets
            State.Current = State.LoadFile(App.Path + @"\settings.json", false);
            DataContext = State.Current;
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
            catch (Exception) { }
        }

        private void OnLoadHistory(object sender, RoutedEventArgs e)
        {
            if (HistoryList.SelectedItem == null) return;
            State.Current.SeedValue = HistoryList.SelectedItem.ToString();
            State.Current.SeedFixed = true;
            RefreshSeed();
        }

        private async void OnGo(object sender, RoutedEventArgs e)
        {
            var dialog = await this.ShowProgressAsync(Environment.NewLine + "Saving data", "Do not remove the Controller," + Environment.NewLine + "MEMORY CARDs, or open disc cover.");
            dialog.SetIndeterminate();
            await Task.Run(Randomizer.Go);

            HistoryList.Items.Refresh();
            RefreshSeed();
            await dialog.CloseAsync();
        }

        private void RefreshSeed()
        {
            SeedValueBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            SeedValueBox.GetBindingExpression(TextBox.IsEnabledProperty).UpdateTarget();
            SeedFixedBox.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateTarget();
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

    public class IconConverter : IValueConverter
    {
        public static BitmapImage SteamIcon = App.GetEmbeddedImage("FF8Mod.Maelstrom.Images.icon_steam.jpg");
        public static BitmapImage RemasteredIcon = App.GetEmbeddedImage("FF8Mod.Maelstrom.Images.icon_remastered.jpg");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Randomizer.DetectVersion((string)value)) return null;
            else if (Globals.Remastered) return RemasteredIcon;
            return SteamIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IconVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Randomizer.DetectVersion((string)value)) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ComboBoxNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse((string)value, out int result);
            return result;
        }
    }
}
