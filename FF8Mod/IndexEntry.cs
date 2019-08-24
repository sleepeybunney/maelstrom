using System;
using System.IO;

namespace FF8Mod.Archive
{
    public class IndexEntry
    {
        public uint Length;
        public uint Location;
        public bool Compressed;

        public IndexEntry(byte[] data)
        {
            if (data.Length != 12) throw new InvalidDataException("Expected 12 bytes for a file index entry");
            Length = BitConverter.ToUInt32(data, 0);
            Location = BitConverter.ToUInt32(data, 4);
            Compressed = BitConverter.ToUInt32(data, 8) == 1;
        }

        public IndexEntry(uint location, uint length, bool compressed)
        {
            Location = location;
            Length = length;
            Compressed = compressed;
        }
    }
}
