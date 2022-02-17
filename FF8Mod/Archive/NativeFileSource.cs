using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sleepey.FF8Mod.Archive
{
    public class NativeFileSource : FileSource
    {
        public NativeFileSource(string fsPath, string fiPath, string flPath)
        {
            ArchivePath = fsPath;
            IndexPath = fiPath;
            ListPath = flPath;

            FileIndex = new FileIndex(fiPath);
            FileList = new FileList(flPath);

            if (!File.Exists(ArchivePath)) throw new FileNotFoundException("Archive file not found: " + ArchivePath);
            if (FileList.Files.Count != FileIndex.Entries.Count) throw new InvalidDataException("File list and index sizes don't match for archive " + ArchivePath);
        }

        public NativeFileSource(string fsPath) : this(Path.ChangeExtension(fsPath, "fs"), Path.ChangeExtension(fsPath, "fi"), Path.ChangeExtension(fsPath, "fl")) { }

        public override IEnumerable<byte> GetFile(string path)
        {
            if (UpdatedFiles.Keys.Contains(path))
            {
                return UpdatedFiles[path];
            }

            using (var stream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return ReadStream(stream, path);
            }
        }

        public override IEnumerable<byte> Encode()
        {
            var tempArchivePath = EncodeImpl(new FileInfo(ArchivePath).Length);

            // save directly to disk - don't want the big main archives in memory
            File.Delete(ArchivePath);
            File.Delete(IndexPath);
            File.Delete(ListPath);

            File.Move(tempArchivePath, ArchivePath);
            File.WriteAllBytes(IndexPath, FileIndex.Encode().ToArray());
            File.WriteAllLines(ListPath, FileList.Files);
            return Array.Empty<byte>();
        }

        protected override Stream GetArchiveStream() => new FileStream(ArchivePath, FileMode.Open, FileAccess.Read, FileShare.None);
    }
}
