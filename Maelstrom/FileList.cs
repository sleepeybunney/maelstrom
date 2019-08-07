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
            Files = new List<string>();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!String.IsNullOrWhiteSpace(line)) Files.Add(line);
                }
            }
        }
    }
}
