using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Maelstrom
{
    class FileIndex
    {
        public List<Entry> Entries;

        public FileIndex(string path)
        {
            Entries = new List<Entry>();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length - 11)
                {
                    Entries.Add(new Entry(reader.ReadBytes(12)));
                }
            }
        }

        public struct Entry
        {
            public uint Length;
            public uint Location;
            public bool Compressed;

            public Entry(byte[] data)
            {
                if (data.Length != 12) throw new InvalidDataException("Expected 12 bytes for a file index entry");
                Length = BitConverter.ToUInt32(data, 0);
                Location = BitConverter.ToUInt32(data, 4);
                Compressed = BitConverter.ToUInt32(data, 8) == 1;
            }

            public Entry(uint location, uint length, bool compressed)
            {
                Location = location;
                Length = length;
                Compressed = compressed;
            }
        }

        public byte[] Encoded
        {
            get
            {
                var length = Entries.Count * 12;
                var result = new byte[length];

                using (var stream = new MemoryStream(result))
                using (var writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < Entries.Count; i++)
                    {
                        writer.Write(Entries[i].Length);
                        writer.Write(Entries[i].Location);
                        uint compressed = (uint)(Entries[i].Compressed ? 1 : 0);
                        writer.Write(compressed);
                    }
                }

                return result;
            }
        }
    }
}
