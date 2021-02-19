using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Sleepey.FF8Mod.Archive
{
    public class ArchiveStream : Stream
    {
        private Stream baseStream;
        private readonly long length;
        private readonly long offset;
        private readonly ArchiveType type;
        private readonly string path;
        private readonly bool root;
        public List<ArchiveIndexEntry> Index;

        // Open an archive file (.fs or .zzz)
        // You can access archives within other archives using semicolons
        // eg. "c:\ff8r\main.zzz;data\field.fs" = open field.fs, which is inside main.zzz
        public ArchiveStream(string path)
        {
            // simple case - single file on disk
            if (!path.Contains(";"))
            {
                baseStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                length = baseStream.Length;
                offset = 0;
                type = GetType(path);
                this.path = path;
                root = true;
                Index = ReadIndex();
                return;
            }

            // nested files open recursively
            var basePath = path.Substring(0, path.LastIndexOf(';'));
            var archivePath = path.Substring(path.LastIndexOf(';') + 1);
            this.path = archivePath;
            baseStream = new ArchiveStream(basePath);

            var entry = ((ArchiveStream)baseStream).Index.Find(e => e.Path == archivePath);
            length = entry.Length;
            offset = (long)entry.Offset;
            type = GetType(archivePath);
            root = false;
            Index = ReadIndex();
            baseStream.Seek(offset, SeekOrigin.Begin);
        }

        private static ArchiveType GetType(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            if (extension == ".zzz") return ArchiveType.ZZZ;
            return ArchiveType.FS;
        }

        // Load index data - file names, locations, etc.
        // Note: FS archives must have corresponding .fl and .fi files in the same location
        private List<ArchiveIndexEntry> ReadIndex()
        {
            // ZZZ archives are a single file with the index in the header
            if (type == ArchiveType.ZZZ) return ZzzArchive.ReadIndex(baseStream);

            // FS archives are split into multiple files
            var blankPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            var indexPath = blankPath + ".fi";
            var listPath = blankPath + ".fl";

            var list = new List<string>();
            Stream index;
            if (root)
            {
                // open directly from the filesystem
                list = File.ReadAllLines(listPath).ToList();
                index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                // open from the base stream
                var listBytes = ((ArchiveStream)baseStream).GetFile(listPath);
                var indexBytes = ((ArchiveStream)baseStream).GetFile(indexPath);

                using (var listStream = new MemoryStream(listBytes))
                using (var reader = new StreamReader(listStream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line)) list.Add(line);
                    }
                }
                index = new MemoryStream(indexBytes);
            }

            var result = FsArchive.ReadIndex(index, list);
            index.Dispose();
            return result;
        }

        public byte[] GetFile(string path)
        {
            using (var reader = new BinaryReader(this, Encoding.UTF8, true))
            {
                var indexEntry = Index.Find(e => WildcardPath.Match(e.Path, path));
                if (indexEntry == null) throw new FileNotFoundException(string.Format("File not found in archive - {0}", path));
                Seek((long)indexEntry.Offset, SeekOrigin.Begin);
                var result = new byte[indexEntry.Length];
                Read(result, 0, (int)indexEntry.Length);
                return result;
            }
        }

        public List<string> ListFiles()
        {
            return Index.Select(e => e.Path).ToList();
        }

        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                CheckDisposed();
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return false;
            }
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                return length;
            }
        }

        // position is relative to the start of this archive
        public override long Position
        {
            get { return baseStream.Position - offset; }

            set { Seek(value, SeekOrigin.Begin); }
        }

        public override void Flush()
        {
            CheckDisposed();
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            var remaining = length - Position;
            if (remaining <= 0) return 0;
            if (remaining < count) count = (int)remaining;
            return baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    baseStream.Seek(offset + this.offset, SeekOrigin.Begin);
                    break;
                case SeekOrigin.End:
                    baseStream.Seek(this.offset + this.length + offset, SeekOrigin.Begin);
                    break;
                default:
                case SeekOrigin.Current:
                    baseStream.Seek(offset, SeekOrigin.Current);
                    break;
            }
            baseStream.Seek(offset + this.offset, SeekOrigin.Begin);
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (baseStream != null)
                {
                    try { baseStream.Dispose(); }
                    catch { }
                    baseStream = null;
                }
            }
        }

        private void CheckDisposed()
        {
            if (baseStream == null) throw new ObjectDisposedException(GetType().Name);
        }
    }

    public enum ArchiveType { FS, ZZZ };
}
