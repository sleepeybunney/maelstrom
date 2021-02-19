using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sleepey.FF8Mod.Archive
{
    class FsArchive
    {
        public static List<ArchiveIndexEntry> ReadIndex(Stream stream, List<string> fileList)
        {
            var result = new List<ArchiveIndexEntry>();
            var count = 0;
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                while (stream.Position < stream.Length - 11)
                {
                    var entryLength = reader.ReadUInt32();
                    var entryOffset = reader.ReadUInt32();
                    var entryCompressed = reader.ReadUInt32() == 1;
                    result.Add(new ArchiveIndexEntry(fileList[count], entryOffset, entryLength));
                    count++;
                }
            }
            return result;
        }
    }
}
