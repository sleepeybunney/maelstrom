using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Maelstrom
{
    class FileList
    {
        public List<string> Files;

        public FileList(string path)
        {
            Files = new List<string>();
            Files.AddRange(File.ReadAllLines(path));
            Files.RemoveAll(f => String.IsNullOrWhiteSpace(f));
        }
    }
}
