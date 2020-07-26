using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FF8Mod.Archive
{
    public class FileList
    {
        public List<string> Files;

        public FileList()
        {
            Files = new List<string>();
        }

        public FileList(string path) : this()
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ReadStream(stream);
            }
        }

        public FileList(byte[] data) : this()
        {
            using (var stream = new MemoryStream(data))
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
                    if (!String.IsNullOrWhiteSpace(line)) Files.Add(line);
                }
            }
        }

        public int GetIndex(string path)
        {
            var pathLower = path.ToLower().Replace(@"\[x]\", "");
            return Files.FindIndex(f => f.ToLower().Replace(@"\x\", "").Replace(@"\eng\", "") == pathLower);
        }

        public List<string> GetDirectory(string path)
        {
            var result = Files.Where(f => f.StartsWith(path.ToLower()));
            if (result.Count() == 0) return null;
            return result.ToList();
        }

        public byte[] Encode()
        {
            return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, Files));
        }
    }
}
