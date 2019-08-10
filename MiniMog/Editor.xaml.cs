using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Controls;

namespace FF8Mod.MiniMog
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : UserControl
    {
        public Editor()
        {
            InitializeComponent();
        }

        public IHighlightingDefinition SyntaxHighlighting
        {
            get { return editor.SyntaxHighlighting; }
            set { editor.SyntaxHighlighting = value; }
        }

        public string Text
        {
            get { return editor.Text; }
            set { editor.Text = value; }
        }
    }
}
