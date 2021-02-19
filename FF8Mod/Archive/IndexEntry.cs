using System;
using System.IO;

namespace Sleepey.FF8Mod.Archive
{
    public class IndexEntry
    {
        public uint Length;
        public uint Location;
        public uint Compression;

        public IndexEntry(byte[] data)
        {
            if (data.Length != 12) throw new InvalidDataException("Expected 12 bytes for a file index entry");
            Length = BitConverter.ToUInt32(data, 0);
            Location = BitConverter.ToUInt32(data, 4);
            Compression = BitConverter.ToUInt32(data, 8);
        }

        public IndexEntry(uint location, uint length, uint compression)
        {
            Location = location;
            Length = length;
            Compression = compression;
        }
    }
}
