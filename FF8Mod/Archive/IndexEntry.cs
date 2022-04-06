using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sleepey.FF8Mod.Archive
{
    public class IndexEntry
    {
        public uint Length { get; set; }
        public uint Location { get; set; }
        public uint Compression { get; set; }

        public IndexEntry(IEnumerable<byte> data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Count() != 12) throw new InvalidDataException("Expected 12 bytes for a file index entry");
            var dataArray = data.ToArray();
            Length = BitConverter.ToUInt32(dataArray, 0);
            Location = BitConverter.ToUInt32(dataArray, 4);
            Compression = BitConverter.ToUInt32(dataArray, 8);
            if (Compression > 2) throw new InvalidDataException("Unrecognised compression format: " + Compression.ToString());
        }

        public IndexEntry(uint location, uint length, uint compression)
        {
            Location = location;
            Length = length;
            Compression = compression;
            if (Compression > 2) throw new InvalidDataException("Unrecognised compression format: " + Compression.ToString());
        }
    }
}
