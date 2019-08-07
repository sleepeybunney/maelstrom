using System;
using System.IO;

namespace FF8Mod.Maelstrom
{
    class FileSource
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
            if (Source != null) throw new NotImplementedException("Writing to nested file sources is not yet implemented");

            var key = FileList.GetIndex(path);
            var entry = FileIndex.Entries[key];

            var oldArchiveSize = new FileInfo(ArchivePath).Length;
            var oldSize = oldArchiveSize - entry.Location;
            if (key != FileIndex.Entries.Count - 1) oldSize = FileIndex.Entries[key + 1].Location - entry.Location;
            var sizeChange = data.Length - oldSize;

            FileIndex.Entries[key] = new FileIndex.Entry(entry.Location, (uint)data.Length, false);
            for (int i = key + 1; i < FileIndex.Entries.Count; i++)
            {
                var currEntry = FileIndex.Entries[i];
                FileIndex.Entries[i] = new FileIndex.Entry((uint)(currEntry.Location + sizeChange), currEntry.Length, currEntry.Compressed);
            }

            using (var stream = new FileStream(IndexPath + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(FileIndex.Encoded);
            }

            using (var newStream = new FileStream(ArchivePath + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            using (var oldStream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var writer = new BinaryWriter(newStream))
            using (var reader = new BinaryReader(oldStream))
            {
                newStream.SetLength(oldStream.Length + sizeChange);
                writer.Write(reader.ReadBytes((int)entry.Location));
                writer.Write(data);
                oldStream.Seek(oldSize, SeekOrigin.Current);
                writer.Write(reader.ReadBytes((int)(oldStream.Length - oldStream.Position)));
            }

            File.Delete(ArchivePath);
            File.Delete(IndexPath);
            File.Move(ArchivePath + ".tmp", ArchivePath);
            File.Move(IndexPath + ".tmp", IndexPath);
        }
    }
}
