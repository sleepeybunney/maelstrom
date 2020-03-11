using System;
using System.Collections.Generic;
using System.Text;

namespace FF8Mod
{
    public class ArchiveIndexEntry
    {
        public string Path;
        public ulong Offset;
        public uint Length;
        public bool Compressed;

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
