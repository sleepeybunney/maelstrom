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

        // overwrite a field script with one imported from a text file
        private void ImportScript(FileSource fieldSource, string fieldName, int entity, int script, string importPath)
        {
            var field = FieldScript.FromSource(fieldSource, fieldName);
            field.ReplaceScript(entity, script, importPath);
            var innerSource = new FileSource(FieldScript.GetFieldPath(fieldName), fieldSource);
            innerSource.ReplaceFile(FieldScript.GetFieldPath(fieldName) + "\\" + fieldName + ".jsm", field.Encoded);
        }

        // slightly easier import with the filename convention "fieldName.entityID.scriptID.txt"
        private void ImportScript(FileSource fieldSource, string fieldName, int entity, int script)
        {
            ImportScript(fieldSource, fieldName, entity, script, string.Format(@"FieldScripts\{0}.{1}.{2}.txt", fieldName, entity, script));
        }

        // return a field script to its original state
        private void ResetScript(FileSource fieldSource, string fieldName, int entity, int script)
        {
            ImportScript(fieldSource, fieldName, entity, script, string.Format(@"OrigFieldScripts\{0}.{1}.{2}.txt", fieldName, entity, script));
        }

        private void OnGo(object sender, RoutedEventArgs e)
        {
            goButton.IsEnabled = false;

            try
            {
                var gameDir = Path.GetDirectoryName(Properties.Settings.Default.GameDir);
                var dataPath = Path.Combine(gameDir, "data", "lang-en");
                var af3dn = Path.Combine(gameDir, "AF3DN.P");

                var introPatch = new BinaryPatch(af3dn, 0x273fb, new byte[] { 0x33, 0x30 }, new byte[] { 0x30, 0x31 });

                if (Properties.Settings.Default.BossShuffle)
                {
                    var battlePath = Path.Combine(dataPath, "battle");
                    var battleSource = new FileSource(battlePath);
                    Boss.Shuffle(battleSource, Properties.Settings.Default.BossRebalance);
                }

                if (Properties.Settings.Default.StorySkip)
                {
                    var fieldPath = Path.Combine(dataPath, "field");
                    var fieldSource = new FileSource(fieldPath);

                    // replace liberi fatali intro with quistis walking through a door
                    ImportScript(fieldSource, "start0", 0, 1);
                    introPatch.Apply();

                    // brief conversation in the infirmary, receive 2 GFs and a party member
                    ImportScript(fieldSource, "bghoke_2", 12, 1);
                    ImportScript(fieldSource, "bghoke_2", 6, 4);

                    // remove tutorial at the front gate
                    ImportScript(fieldSource, "bggate_1", 0, 0);
                }
                else
                {
                    introPatch.Remove();
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
