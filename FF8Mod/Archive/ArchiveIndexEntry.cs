using System;
using System.Collections.Generic;
using System.Text;

namespace Sleepey.FF8Mod.Archive
{
    public class ArchiveIndexEntry
    {
        public string Path { get; set; }
        public ulong Offset { get; set; }
        public uint Length { get; set; }
        public bool Compressed { get; set; }

        public ArchiveIndexEntry(string path, ulong offset, uint length, bool compressed)
        {
            Path = path;
            Offset = offset;
            Length = length;
            Compressed = compressed;
        }

        public ArchiveIndexEntry(string path, ulong offset, uint length) : this(path, offset, length, false) { }
    }
}
