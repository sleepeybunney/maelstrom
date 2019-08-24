using System;
using System.IO;

namespace FF8Mod.Archive
{
    public class FileSource
    {
        public FileList FileList;
        public FileIndex FileIndex;
        public string ArchivePath;
        public FileSource Source;

        public FileSource(string archivePath, FileList fileList, FileIndex fileIndex)
        {
            FileList = fileList;
            FileIndex = fileIndex;
            ArchivePath = archivePath;
            Source = null;
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
        }

        public byte[] GetFile(string path)
        {
            if (Source == null)
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
                if (!entry.Compressed) return reader.ReadBytes((int)entry.Length);

                var length = reader.ReadUInt32();
                return Lzss.Decompress(reader.ReadBytes((int)length));
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
            var key = FileList.GetIndex(path);
            var entry = FileIndex.Entries[key];

            // calculate change in file size
            long oldArchiveSize;
            if (Source == null) oldArchiveSize = new FileInfo(ArchivePath).Length;
            else oldArchiveSize = Source.GetFile(ArchivePath).LongLength;

            var oldSize = oldArchiveSize - entry.Location;
            if (key != FileIndex.Entries.Count - 1) oldSize = FileIndex.Entries[key + 1].Location - entry.Location;
            var sizeChange = data.Length - oldSize;

            // offset all the file entries displaced by the change
            FileIndex.Entries[key] = new IndexEntry(entry.Location, (uint)data.Length, false);
            for (int i = key + 1; i < FileIndex.Entries.Count; i++)
            {
                var currEntry = FileIndex.Entries[i];
                FileIndex.Entries[i] = new IndexEntry((uint)(currEntry.Location + sizeChange), currEntry.Length, currEntry.Compressed);
            }

            // write new index file to temp space
            var tempIndexPath = Path.GetTempFileName();
            using (var stream = new FileStream(tempIndexPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(FileIndex.Encode());
            }

            // write new archive file to temp space
            var tempArchivePath = Path.GetTempFileName();
            using (var newStream = new FileStream(tempArchivePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var oldStream = ArchiveStream())
            using (var writer = new BinaryWriter(newStream))
            using (var reader = new BinaryReader(oldStream))
            {
                newStream.SetLength(oldStream.Length + sizeChange);
                writer.Write(reader.ReadBytes((int)entry.Location));
                writer.Write(data);
                oldStream.Seek(oldSize, SeekOrigin.Current);
                writer.Write(reader.ReadBytes((int)(oldStream.Length - oldStream.Position)));
            }

            // copy list file to temp space
            var tempListPath = Path.GetTempFileName();
            File.WriteAllLines(tempListPath, FileList.Files.ToArray());

            if (Source == null)
            {
                File.Delete(ArchivePath);
                File.Delete(IndexPath);
                File.Delete(ListPath);
                File.Move(tempArchivePath, ArchivePath);
                File.Move(tempIndexPath, IndexPath);
                File.Move(tempListPath, ListPath);
            }
            else
            {
                Source.ReplaceFile(ArchivePath, File.ReadAllBytes(tempArchivePath));
                Source.ReplaceFile(IndexPath, File.ReadAllBytes(tempIndexPath));
                Source.ReplaceFile(ListPath, File.ReadAllBytes(tempListPath));
                File.Delete(tempArchivePath);
                File.Delete(tempIndexPath);
                File.Delete(tempListPath);
            }
        }

        private Stream ArchiveStream()
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
