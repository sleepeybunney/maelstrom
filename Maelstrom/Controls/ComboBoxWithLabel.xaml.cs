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

namespace Sleepey.Maelstrom
{
    /// <summary>
    /// Interaction logic for ComboBoxWithLabel.xaml
    /// </summary>
    public partial class ComboBoxWithLabel : UserControl
    {
        public string Title { get; set; }
        public int ComboBoxWidth { get; set; } = 80;
        public List<ComboBoxItem> ComboBoxItems { get; set; } = new List<ComboBoxItem>();
        public string SelectedValuePath { get; set; } = "Content";

        public object SelectedValue
        {
            get { return GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("SelectedValue", typeof(object), typeof(ComboBoxWithLabel));

        public ComboBoxWithLabel()
        {
            InitializeComponent();
        }
    }
}
