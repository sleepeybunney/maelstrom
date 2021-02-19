using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Sleepey.FF8Mod
{
    public class Zzz
    {
        public string ArchivePath;
        public List<ArchiveIndexEntry> Index;

        public Zzz(string archivePath)
        {
            if (!File.Exists(archivePath)) throw new FileNotFoundException("ZZZ file not found: " + archivePath);

            ArchivePath = archivePath;
            Index = new List<ArchiveIndexEntry>();

            using (var stream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Index = ReadIndex(stream);
            }
        }

        public static List<ArchiveIndexEntry> ReadIndex(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var fileCount = reader.ReadUInt32();
                var result = new List<ArchiveIndexEntry>();

                for (uint i = 0; i < fileCount; i++)
                {
                    var pathLength = reader.ReadUInt32();
                    var pathBytes = reader.ReadBytes((int)pathLength);
                    var path = Encoding.UTF8.GetString(pathBytes);
                    var offset = reader.ReadUInt64();
                    var length = reader.ReadUInt32();
                    result.Add(new ArchiveIndexEntry(path, offset, length));
                }

                return result;
            }
        }

        public byte[] GetFile(string path)
        {
            var entry = GetEntry(path);
            using (var stream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek((long)entry.Offset, SeekOrigin.Begin);
                return reader.ReadBytes((int)entry.Length);
            }
        }

        public ArchiveIndexEntry GetEntry(string path)
        {
            var entry = Index.Find(f => f.Path == path);
            if (entry == null) throw new FileNotFoundException(string.Format("File ({0}) not found in ZZZ archive - {1}", path, ArchivePath));
            return entry;
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

        public void ReplaceFiles(Dictionary<string, byte[]> files)
        {
            using (var readStream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var writeStream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Write, FileShare.Read))
            using (var reader = new BinaryReader(readStream))
            using (var writer = new BinaryWriter(writeStream))
            {
                // skip to the first changed file
                var firstOffset = files.Keys.Min(f => Index.Find(i => i.Path == f).Offset);
                var orderedIndex = Index.Where(i => i.Offset >= firstOffset).OrderBy(i => i.Offset);
                readStream.Seek((long)firstOffset, SeekOrigin.Begin);
                writeStream.Seek((long)firstOffset, SeekOrigin.Begin);

                // delete files to be replaced
                foreach (var entry in orderedIndex)
                {
                    if (!files.ContainsKey(entry.Path))
                    {
                        readStream.Seek((long)entry.Offset, SeekOrigin.Begin);
                        Index[Index.FindIndex(i => i.Path == entry.Path)].Offset = (ulong)writeStream.Position;

                        var bufferSize = 64 * 1024;
                        var toRead = (int)entry.Length;
                        while (toRead > 0)
                        {
                            var buffer = reader.ReadBytes(Math.Min(toRead, bufferSize));
                            writer.Write(buffer);
                            toRead -= bufferSize;
                        }
                    }
                }

                // append new files to the end
                foreach (var path in files.Keys)
                {
                    var entry = Index.FindIndex(i => i.Path == path);
                    Index[entry].Offset = (ulong)writeStream.Position;
                    Index[entry].Length = (uint)files[path].Length;
                    Index[entry].Compressed = false;
                    writer.Write(files[path]);
                }

                // truncate
                writeStream.SetLength(writeStream.Position);

                // update header
                var header = RebuildIndex();
                writeStream.Seek(0, SeekOrigin.Begin);
                writer.Write(header);
            }
        }

        public void ReplaceFile(string path, byte[] data)
        {
            ReplaceFiles(new Dictionary<string, byte[]> { { path, data } });
        }
    }
}
