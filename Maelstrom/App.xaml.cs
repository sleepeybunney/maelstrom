using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

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
            using (var stream = assembly.GetManifestResourceStream(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static BitmapImage GetEmbeddedImage(string path)
        {
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                return result;
            }
        }

        public static string Path
        {
            get
            {
                return System.IO.Path.GetDirectoryName(assembly.Location);
            }
        }
    }
}
