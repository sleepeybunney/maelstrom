using System;
using System.Collections.Generic;
using System.IO;

namespace FF8Mod.Archive
{
    public class FileIndex
    {
        public List<IndexEntry> Entries;

        public FileIndex()
        {
            Entries = new List<IndexEntry>();
        }

        public FileIndex(string path) : this()
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ReadStream(stream);
            }
        }

        public FileIndex(byte[] data) : this()
        {
            using (var stream = new MemoryStream(data))
            {
                ReadStream(stream);
            }
        }

        private void ReadStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length - 11)
                {
                    Entries.Add(new IndexEntry(reader.ReadBytes(12)));
                }
            }
        }

        public byte[] Encode()
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
