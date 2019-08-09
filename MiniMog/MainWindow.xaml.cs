using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

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

        private void OpenClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            dialog.Filter = "File Source (*.fs)|*.fs";
            dialog.Title = "Open battle.fs";

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

        private void UpdateMonsterList()
        {
            monsterList.Items.Clear();
            foreach (var m in Monsters)
            {
                monsterList.Items.Add(m);
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
    }
}
