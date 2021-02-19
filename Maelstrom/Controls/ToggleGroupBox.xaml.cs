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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sleepey.Maelstrom
{
    /// <summary>
    /// Interaction logic for ToggleGroupBox.xaml
    /// </summary>
    [ContentProperty("InnerContent")]
    public partial class ToggleGroupBox : UserControl
    {
        public string Title { get; set; }

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool), typeof(ToggleGroupBox));

        public object InnerContent { get; set; }

        public ToggleGroupBox()
        {
            InitializeComponent();
        }
    }
}
