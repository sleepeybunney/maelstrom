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

namespace FF8Mod.Maelstrom
{
    /// <summary>
    /// Interaction logic for DoubleOption.xaml
    /// </summary>
    public partial class DoubleOption : UserControl
    {
        public string Title { get; set; }
        public string Title2 { get; set; }

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool), typeof(DoubleOption));

        public bool Checked2
        {
            get { return (bool)GetValue(Checked2Property); }
            set { SetValue(Checked2Property, value); }
        }

        public static readonly DependencyProperty Checked2Property = DependencyProperty.Register("Checked2", typeof(bool), typeof(DoubleOption));

        public DoubleOption()
        {
            InitializeComponent();
        }
    }
}
