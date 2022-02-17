using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Archive
{
    public abstract class FileSource
    {
        public FileList FileList { get; set; }
        public FileIndex FileIndex { get; set; }
        public string ArchivePath { get; set; }
        public string IndexPath { get; set; }
        public string ListPath { get; set; }
        public FileSource Source { get; set; }
        public Dictionary<string, IEnumerable<byte>> UpdatedFiles { get; set; } = new Dictionary<string, IEnumerable<byte>>();

        public abstract IEnumerable<byte> GetFile(string path);

        protected IEnumerable<byte> ReadStream(Stream stream, string path)
        {
            var key = FileList.GetIndex(path);
            if (key == -1) throw new FileNotFoundException("Not found in archive (" + ArchivePath + "): " + path);
            var entry = FileIndex.Entries[key];
            if (entry.Length > int.MaxValue) throw new NotImplementedException("Unable to read large file: " + path);

            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(entry.Location, SeekOrigin.Begin);

                if (entry.Compression == 1)
                {
                    var length = reader.ReadUInt32();
                    return Lzss.Decompress(reader.ReadBytes((int)length));
                }

                if (entry.Compression == 2)
                {
                    var length = reader.ReadUInt32();
                    return Lz4.Decompress(reader.ReadBytes((int)length));
                }

                return reader.ReadBytes((int)entry.Length);
            }
        }

        public virtual void ReplaceFile(string path, IEnumerable<byte> data)
        {
            UpdatedFiles[path] = data;
        }

        public abstract IEnumerable<byte> Encode();

        internal string EncodeImpl(long oldArchiveSize)
        {
            var oldIndex = FileIndex.Entries;

            // set up temp storage for constructing the file
            var tempArchivePath = Path.GetTempFileName();
            using (var newStream = new FileStream(tempArchivePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var oldStream = GetArchiveStream())
            using (var writer = new BinaryWriter(newStream))
            using (var reader = new BinaryReader(oldStream))
            {
                newStream.SetLength(oldStream.Length);

                foreach (var path in UpdatedFiles.Keys.OrderBy(k => FileList.GetIndex(k)))
                {
                    var key = FileList.GetIndex(path);
                    var entry = FileIndex.Entries[key];

                    // calculate packed size change
                    var oldEntry = oldIndex[key];
                    var oldSize = oldArchiveSize - oldEntry.Location;
                    if (key != FileIndex.Entries.Count - 1) oldSize = oldIndex[key + 1].Location - oldEntry.Location;
                    var sizeChange = UpdatedFiles[path].LongCount() - oldSize;

                    // offset all the file entries displaced by the change
                    FileIndex.Entries[key] = new IndexEntry(entry.Location, (uint)UpdatedFiles[path].Count(), 0);
                    for (int i = key + 1; i < FileIndex.Entries.Count; i++)
                    {
                        var currEntry = FileIndex.Entries[i];
                        FileIndex.Entries[i] = new IndexEntry((uint)(currEntry.Location + sizeChange), currEntry.Length, currEntry.Compression);
                    }

                    // write data to temp file
                    newStream.SetLength(newStream.Length + sizeChange);
                    writer.Write(reader.ReadBytes((int)(entry.Location - newStream.Position)));
                    writer.Write(UpdatedFiles[path].ToArray());
                    oldStream.Seek(oldSize, SeekOrigin.Current);
                }

                writer.Write(reader.ReadBytes((int)(oldStream.Length - oldStream.Position)));
            }

            // clear out the file updates, no longer pending
            UpdatedFiles = new Dictionary<string, IEnumerable<byte>>();

            return tempArchivePath;
        }

        protected abstract Stream GetArchiveStream();

        public bool PathExists(string path)
        {
            var dir = FileList.GetDirectory(path);
            return dir != null;
        }
    }
}
