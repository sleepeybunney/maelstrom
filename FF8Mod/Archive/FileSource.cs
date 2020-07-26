using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FF8Mod.Archive
{
    public class FileSource
    {
        public FileList FileList;
        public FileIndex FileIndex;
        public string ArchivePath;
        public FileSource Source;
        public Dictionary<string, byte[]> UpdatedFiles;

        public FileSource(string archivePath, FileList fileList, FileIndex fileIndex)
        {
            FileList = fileList;
            FileIndex = fileIndex;
            ArchivePath = archivePath;
            Source = null;
            UpdatedFiles = new Dictionary<string, byte[]>();
            if (!File.Exists(ArchivePath)) throw new FileNotFoundException("Archive file not found: " + ArchivePath);
            if (FileList.Files.Count != FileIndex.Entries.Count) throw new InvalidDataException("File list and index sizes don't match for archive " + ArchivePath);
        }

        public FileSource(string fsPath, string flPath, string fiPath) : this(fsPath, new FileList(flPath), new FileIndex(fiPath)) { }

        public FileSource(string singlePath) : this(singlePath + ".fs", singlePath + ".fl", singlePath + ".fi") { }

        public FileSource(string singlePath, FileSource source)
        {
            FileList = new FileList(source.GetFile(singlePath + ".fl"));
            FileIndex = new FileIndex(source.GetFile(singlePath + ".fi"));
            ArchivePath = singlePath + ".fs";
            Source = source;
            UpdatedFiles = new Dictionary<string, byte[]>();
        }

        public byte[] GetFile(string path)
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
                using (var stream = new MemoryStream(Source.GetFile(ArchivePath)))
                {
                    return ReadStream(stream, path);
                }
            }
        }

        private byte[] ReadStream(Stream stream, string path)
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

        private string IndexPath
        {
            get
            {
                return ArchivePath.Substring(0, ArchivePath.Length - 1) + "i";
            }
        }

        private string ListPath
        {
            get
            {
                return ArchivePath.Substring(0, ArchivePath.Length - 1) + "l";
            }
        }

        public void ReplaceFile(string path, byte[] data)
        {
            UpdatedFiles[path] = data;

            if (Source != null)
            {
                Source.UpdatedFiles[ArchivePath] = Encode();
                Source.UpdatedFiles[IndexPath] = FileIndex.Encode();
                Source.UpdatedFiles[ListPath] = FileList.Encode();
            }
        }

        public byte[] Encode()
        {
            long oldArchiveSize;
            if (Source == null) oldArchiveSize = new FileInfo(ArchivePath).Length;
            else oldArchiveSize = Source.GetFile(ArchivePath).LongLength;
            var oldIndex = FileIndex.Entries.ToArray();

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
                    var sizeChange = UpdatedFiles[path].Length - oldSize;

                    // offset all the file entries displaced by the change
                    FileIndex.Entries[key] = new IndexEntry(entry.Location, (uint)UpdatedFiles[path].Length, 0);
                    for (int i = key + 1; i < FileIndex.Entries.Count; i++)
                    {
                        var currEntry = FileIndex.Entries[i];
                        FileIndex.Entries[i] = new IndexEntry((uint)(currEntry.Location + sizeChange), currEntry.Length, currEntry.Compression);
                    }

                    // write data to temp file
                    newStream.SetLength(newStream.Length + sizeChange);
                    writer.Write(reader.ReadBytes((int)(entry.Location - newStream.Position)));
                    writer.Write(UpdatedFiles[path]);
                    oldStream.Seek(oldSize, SeekOrigin.Current);
                }

                writer.Write(reader.ReadBytes((int)(oldStream.Length - oldStream.Position)));
            }

            // clear out the file updates, no longer pending
            UpdatedFiles = new Dictionary<string, byte[]>();

            if (Source == null)
            {
                // save directly to disk - don't want the big main archives in memory
                File.Delete(ArchivePath);
                File.Delete(IndexPath);
                File.Delete(ListPath);

                File.Move(tempArchivePath, ArchivePath);
                File.WriteAllBytes(IndexPath, FileIndex.Encode());
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
            else return new MemoryStream(Source.GetFile(ArchivePath));
        }

        public bool PathExists(string path)
        {
            var dir = FileList.GetDirectory(path);
            return dir != null;
        }
    }
}
