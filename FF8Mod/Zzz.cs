using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FF8Mod
{
    public class Zzz
    {
        public string ArchivePath;
        public List<ZzzIndexEntry> Index;

        public Zzz(string archivePath)
        {
            if (!File.Exists(archivePath)) throw new FileNotFoundException("ZZZ file not found: " + archivePath);

            ArchivePath = archivePath;
            Index = new List<ZzzIndexEntry>();

            using (var stream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                var fileCount = reader.ReadUInt32();
                for (uint i = 0; i < fileCount; i++)
                {
                    var pathLength = reader.ReadUInt32();
                    var pathBytes = reader.ReadBytes((int)pathLength);
                    var path = Encoding.UTF8.GetString(pathBytes);
                    var offset = reader.ReadUInt64();
                    var length = reader.ReadUInt32();
                    Index.Add(new ZzzIndexEntry(path, offset, length));
                }
            }
        }

        public byte[] GetFile(string path)
        {
            var entry = Index.Find(f => f.Path == path);
            if (entry == null) throw new FileNotFoundException(string.Format("File ({0}) not found in ZZZ archive - {1}", path, ArchivePath));

            using (var stream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek((long)entry.Offset, SeekOrigin.Begin);
                return reader.ReadBytes((int)entry.Length);
            }
        }

        public byte[] RebuildIndex()
        {
            var header = new List<byte>();
            header.AddRange(BitConverter.GetBytes((uint)Index.Count));
            for (int i = 0; i < Index.Count; i++)
            {
                var path = Encoding.UTF8.GetBytes(Index[i].Path);
                header.AddRange(BitConverter.GetBytes((uint)path.Length));
                header.AddRange(path);
                header.AddRange(BitConverter.GetBytes(Index[i].Offset));
                header.AddRange(BitConverter.GetBytes(Index[i].Length));
            }

            return header.ToArray();
        }

        public void ReplaceFile(string path, byte[] data)
        {
            var entry = Index.FindIndex(f => f.Path == path);
            var oldLength = Index[entry].Length;
            var sizeChange = (uint)data.Length - Index[entry].Length;
            Index[entry].Length = (uint)data.Length;
            for (int i = entry + 1; i < Index.Count; i++)
            {
                Index[i].Offset = Index[i].Offset + sizeChange;
            }

            var tempArchivePath = Path.GetTempFileName();
            using (var inStream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var outStream = new FileStream(tempArchivePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var reader = new BinaryReader(inStream))
            using (var writer = new BinaryWriter(outStream))
            {
                var header = RebuildIndex();
                writer.Write(header);
                inStream.Seek(header.Length, SeekOrigin.Begin);
                for (int i = 0; i < Index.Count; i++)
                {
                    if (i == entry)
                    {
                        writer.Write(data);
                        inStream.Seek(oldLength, SeekOrigin.Current);
                    }
                    else
                    {
                        writer.Write(reader.ReadBytes((int)Index[i].Length));
                    }
                }
            }

            File.Delete(ArchivePath);
            File.Move(tempArchivePath, ArchivePath);
        }
    }
}
