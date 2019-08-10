using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                    if (!fs.PathExists(@"c:\ff8\data\eng\battle"))
                    {
                        throw new Exception("Incorrect file - please open battle.fs");
                    }

                    var monsterPaths = fs.FileList.Files.Where(f =>
                        f.StartsWith(@"c:\ff8\data\eng\battle\c0m")
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
            var monster = Monsters[monsterList.SelectedIndex];
            var script = monster.AI.Scripts;
            initEditor.Text = string.Join(Environment.NewLine, script.Init);
            execEditor.Text = string.Join(Environment.NewLine, script.Execute);
            counterEditor.Text = string.Join(Environment.NewLine, script.Counter);
            deathEditor.Text = string.Join(Environment.NewLine, script.Death);
            preCounterEditor.Text = string.Join(Environment.NewLine, script.PreCounter);
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
