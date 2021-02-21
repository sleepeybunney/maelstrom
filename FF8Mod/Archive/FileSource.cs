using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Archive
{
    public class FileSource
    {
        public FileList FileList { get; set; }
        public FileIndex FileIndex { get; set; }
        public string ArchivePath { get; set; }
        public FileSource Source { get; set; }
        public Dictionary<string, IEnumerable<byte>> UpdatedFiles { get; set; } = new Dictionary<string, IEnumerable<byte>>();

        private string IndexPath { get => ArchivePath.Substring(0, ArchivePath.Length - 1) + "i"; }
        private string ListPath { get => ArchivePath.Substring(0, ArchivePath.Length - 1) + "l"; }

        public FileSource(string archivePath, FileList fileList, FileIndex fileIndex)
        {
            FileList = fileList;
            FileIndex = fileIndex;
            ArchivePath = archivePath;
            if (!File.Exists(ArchivePath)) throw new FileNotFoundException("Archive file not found: " + ArchivePath);
            if (FileList.Files.Count != FileIndex.Entries.Count) throw new InvalidDataException("File list and index sizes don't match for archive " + ArchivePath);
        }

        public FileSource(string fsPath, string flPath, string fiPath) : this(fsPath, new FileList(flPath), new FileIndex(fiPath)) { }

        public FileSource(string singlePath)
        {
            if (singlePath.EndsWith(".fs")) singlePath = singlePath.Substring(0, singlePath.Length - 3);
            FileList = new FileList(singlePath + ".fl");
            FileIndex = new FileIndex(singlePath + ".fi");
            ArchivePath = singlePath + ".fs";
        }

        public FileSource(string singlePath, FileSource source)
        {
            if (singlePath.EndsWith(".fs")) singlePath = singlePath.Substring(0, singlePath.Length - 3);
            FileList = new FileList(source.GetFile(singlePath + ".fl"));
            FileIndex = new FileIndex(source.GetFile(singlePath + ".fi"));
            ArchivePath = singlePath + ".fs";
            Source = source;
        }

        public IEnumerable<byte> GetFile(string path)
        {
            if (UpdatedFiles.Keys.Contains(path))
            {
                return UpdatedFiles[path];
            }
            else if (Source == null)
            {
                using (var stream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return ReadStream(stream, path);
                }
            }
            else
            {
                using (var stream = new MemoryStream(Source.GetFile(ArchivePath).ToArray()))
                {
                    return ReadStream(stream, path);
                }
            }
        }

        private IEnumerable<byte> ReadStream(Stream stream, string path)
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

        public void ReplaceFile(string path, IEnumerable<byte> data)
        {
            UpdatedFiles[path] = data;

            if (Source != null)
            {
                Source.UpdatedFiles[ArchivePath] = Encode();
                Source.UpdatedFiles[IndexPath] = FileIndex.Encode();
                Source.UpdatedFiles[ListPath] = FileList.Encode();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var oldArchiveSize = (Source == null) ? new FileInfo(ArchivePath).Length : Source.GetFile(ArchivePath).LongCount();
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

            if (Source == null)
            {
                // save directly to disk - don't want the big main archives in memory
                File.Delete(ArchivePath);
                File.Delete(IndexPath);
                File.Delete(ListPath);

                File.Move(tempArchivePath, ArchivePath);
                File.WriteAllBytes(IndexPath, FileIndex.Encode().ToArray());
                File.WriteAllLines(ListPath, FileList.Files);
                return Array.Empty<byte>();
            }
            else
            {
                // smaller inner archives may be returned whole
                var result = File.ReadAllBytes(tempArchivePath);
                File.Delete(tempArchivePath);
                return result;
            }
        }

        private Stream GetArchiveStream()
        {
            if (Source == null) return new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.None);
            else return new MemoryStream(Source.GetFile(ArchivePath).ToArray());
        }

        public bool PathExists(string path)
        {
            var dir = FileList.GetDirectory(path);
            return dir != null;
        }
    }
}
