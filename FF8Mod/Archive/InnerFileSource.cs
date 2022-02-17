using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sleepey.FF8Mod.Archive
{
    public class InnerFileSource : FileSource
    {
        public InnerFileSource(string fsPath, FileSource source)
        {
            Source = source;
            ArchivePath = Path.ChangeExtension(fsPath, "fs");
            IndexPath = Path.ChangeExtension(fsPath, "fi");
            ListPath = Path.ChangeExtension(fsPath, "fl");

            FileIndex = new FileIndex(Source.GetFile(IndexPath));
            FileList = new FileList(Source.GetFile(ListPath));
        }

        public override IEnumerable<byte> GetFile(string path)
        {
            if (UpdatedFiles.Keys.Contains(path))
            {
                return UpdatedFiles[path];
            }

            using (var stream = new MemoryStream(Source.GetFile(ArchivePath).ToArray()))
            {
                return ReadStream(stream, path);
            }
        }

        public override void ReplaceFile(string path, IEnumerable<byte> data)
        {
            base.ReplaceFile(path, data);

            Source.UpdatedFiles[ArchivePath] = Encode();
            Source.UpdatedFiles[IndexPath] = FileIndex.Encode();
            Source.UpdatedFiles[ListPath] = FileList.Encode();
        }

        public override IEnumerable<byte> Encode()
        {
            var tempArchivePath = EncodeImpl(Source.GetFile(ArchivePath).LongCount());

            // smaller inner archives may be returned whole
            var result = File.ReadAllBytes(tempArchivePath);
            File.Delete(tempArchivePath);
            return result;
        }

        protected override Stream GetArchiveStream() => new MemoryStream(Source.GetFile(ArchivePath).ToArray());
    }
}
