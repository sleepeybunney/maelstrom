using System.IO;
using System.Reflection;
using System.Windows;

namespace FF8Mod.Maelstrom
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        public static string ReadEmbeddedFile(string path)
        {
            var stream = assembly.GetManifestResourceStream(path);
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
