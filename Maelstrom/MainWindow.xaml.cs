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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Maelstrom
{
    /// what's the plan
    /// ✓ shorten intro sequence
    /// ✓ clear a path out to the fire cave
    /// ✓ replace ifrit with something
    /// ✓ change a draw point
    /// ✓ work out how to patch the game a la deling
    /// - apply above changes by patch
    /// - randomise ifrit & granaldo
    /// - try randomising normal encounters too
    /// - can i make like a really big grat or something?
    /// - hide the MD level key somewhere -> randomise oilboyles & launch the garden

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var fs = new FileSource(@"D:\Steam\steamapps\common\FINAL FANTASY VIII\Data\lang-en\main");
            var img = fs.GetFile(@"c:\ff8\data\eng\loop01.lzs");
            File.WriteAllBytes(@"d:\ff8-bak\loop1.lzs", img);
        }
    }
}
