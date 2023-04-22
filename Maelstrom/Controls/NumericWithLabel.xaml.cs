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
    /// Interaction logic for NumericWithLabel.xaml
    /// </summary>
    public partial class NumericWithLabel : UserControl
    {
        public string Title { get; set; }
        public int InputWidth { get; set; } = 120;
        public int Minimum { get; set; } = 0;
        public int Maximum { get; set; } = int.MaxValue;

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(NumericWithLabel));

        public NumericWithLabel()
        {
            InitializeComponent();
        }
    }
}
