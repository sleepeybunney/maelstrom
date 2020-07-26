using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using FF8Mod.Archive;

namespace FF8Mod.MiniMog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<Monster> _monsters;

        public MainWindow()
        {
            InitializeComponent();

            using (var reader = new XmlTextReader("BattleScript.xshd"))
            {
                initEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                execEditor.SyntaxHighlighting = initEditor.SyntaxHighlighting;
                counterEditor.SyntaxHighlighting = initEditor.SyntaxHighlighting;
                deathEditor.SyntaxHighlighting = initEditor.SyntaxHighlighting;
                preCounterEditor.SyntaxHighlighting = initEditor.SyntaxHighlighting;
            }
        }

        private List<Monster> Monsters
        {
            get
            {
                return _monsters;
            }
            set
            {
                _monsters = value;
                UpdateMonsterList();
            }
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "File Source (*.fs)|*.fs",
                Title = "Open battle.fs"
            };

            if (dialog.ShowDialog() == true)
            {
                var path = Path.Combine(Path.GetDirectoryName(dialog.FileName), Path.GetFileNameWithoutExtension(dialog.FileName));
                try
                {
                    // open & validate file
                    var fs = new FileSource(path);
                    if (!fs.PathExists(Globals.DataPath + @"\battle"))
                    {
                        throw new Exception("Incorrect file - please open battle.fs");
                    }

                    var monsterPaths = fs.FileList.Files.Where(f =>
                        f.StartsWith(Globals.DataPath + @"\battle\c0m")
                        && f.EndsWith(@".dat")
                    );

                    var monsters = new List<Monster>();
                    foreach (var p in monsterPaths)
                    {
                        // load whatever works & ignore the rest
                        try
                        {
                            var monster = Monster.FromSource(fs, p);
                            monsters.Add(monster);
                        }
                        catch (Exception) { }
                    }

                    Monsters = monsters;

                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message, "Error opening file");
                }
            }
        }

        private void SelectMonster(object sender, SelectionChangedEventArgs e)
        {
            if (monsterList.SelectedIndex < 0)
            {
                ClearEditors();
                return;
            }

            var scripts = Monsters[monsterList.SelectedIndex].AI.Scripts;
            PopulateEditor(initEditor, scripts.Init);
            PopulateEditor(execEditor, scripts.Execute);
            PopulateEditor(counterEditor, scripts.Counter);
            PopulateEditor(deathEditor, scripts.Death);
            PopulateEditor(preCounterEditor, scripts.PreCounter);
        }

        private void PopulateEditor(Editor editor, List<Battle.Instruction> script)
        {
            editor.Text = string.Join(Environment.NewLine, script);
        }

        private void ClearEditors()
        {
            foreach (var editor in new Editor[] { initEditor, execEditor, counterEditor, deathEditor, preCounterEditor })
            {
                PopulateEditor(editor, new List<Battle.Instruction>());
            }
        }

        private void UpdateMonsterList()
        {
            monsterList.Items.Clear();
            foreach (var m in Monsters)
            {
                monsterList.Items.Add(m);
            }
        }
    }
}
