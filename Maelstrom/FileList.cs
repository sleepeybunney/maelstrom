using System;
using System.Collections.Generic;
using System.IO;

namespace FF8Mod.Maelstrom
{
    class FileList
    {
        public List<string> Files;

        public FileList(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ReadStream(stream);
            }
        }

        public FileList(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                ReadStream(stream);
            }
        }

        private void ReadStream(Stream stream)
        {
            Files = new List<string>();
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
            var pathLower = path.ToLower();
            return Files.FindIndex(f => f.ToLower() == pathLower);
        }
    }
}
