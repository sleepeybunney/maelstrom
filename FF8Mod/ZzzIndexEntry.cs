using System;
using System.Collections.Generic;
using System.Text;

namespace FF8Mod
{
    public class ZzzIndexEntry
    {
        public string Path;
        public ulong Offset;
        public uint Length;

        public ZzzIndexEntry(string path, ulong offset, uint length)
        {
            Path = path;
            Offset = offset;
            Length = length;
        }
    }
}
