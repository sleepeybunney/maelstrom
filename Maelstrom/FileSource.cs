using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Maelstrom
{
    class FileSource
    {
        public FileList FileList;
        public FileIndex FileIndex;
        public string ArchivePath;

        public FileSource(string archivePath, FileList fileList, FileIndex fileIndex)
        {
            FileList = fileList;
            FileIndex = fileIndex;
            ArchivePath = archivePath;
            if (!File.Exists(ArchivePath)) throw new FileNotFoundException("Archive file not found: " + ArchivePath);
            if (FileList.Files.Count != FileIndex.Entries.Count) throw new InvalidDataException("File list and index sizes don't match for archive " + ArchivePath);
        }

        public FileSource(string fsPath, string flPath, string fiPath) : this(fsPath, new FileList(flPath), new FileIndex(fiPath)) { }

        public FileSource(string singlePath) : this(singlePath + ".fs", singlePath + ".fl", singlePath + ".fi") { }

        public byte[] GetFile(string path)
        {
            var key = FileList.Files.IndexOf(path);
            if (key == -1) throw new FileNotFoundException("Not found in archive (" + ArchivePath + "): " + path);

            var entry = FileIndex.Entries[key];
            if (entry.Compressed) throw new NotImplementedException("Unable to open compressed file in archive " + ArchivePath);
            if (entry.Length > int.MaxValue) throw new NotImplementedException("Unable to read large file: " + path);

            using (var stream = new FileStream(ArchivePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(entry.Location, SeekOrigin.Begin);
                return reader.ReadBytes((int)entry.Length);
            }
        }
    }
}
