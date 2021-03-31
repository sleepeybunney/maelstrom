using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sleepey.FF8Mod.Archive
{
    public class FileList
    {
        public List<string> Files { get; set; } = new List<string>();

        public FileList() { }

        public FileList(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ReadStream(stream);
            }
        }

        public FileList(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            {
                ReadStream(stream);
            }
        }

        private void ReadStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line)) Files.Add(line);
                }
            }
        }

        public IEnumerable<string> GetDirectory(string path)
        {
            var result = Files.Where(f => f.StartsWith(path.ToLower()));
            if (result.Count() == 0) return null;
            return result;
        }

        public int GetIndex(string path) => Files.FindIndex(f => WildcardPath.Match(f, path));

        public IEnumerable<byte> Encode() => Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, Files));
    }
}
