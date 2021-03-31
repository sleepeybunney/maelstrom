using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sleepey.FF8Mod.Archive
{
    public static class ZzzArchive
    {
        public static List<ArchiveIndexEntry> ReadIndex(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var fileCount = reader.ReadUInt32();
                var result = new List<ArchiveIndexEntry>();

                for (uint i = 0; i < fileCount; i++)
                {
                    var pathLength = reader.ReadUInt32();
                    var pathBytes = reader.ReadBytes((int)pathLength);
                    var path = Encoding.UTF8.GetString(pathBytes);
                    var offset = reader.ReadUInt64();
                    var length = reader.ReadUInt32();
                    result.Add(new ArchiveIndexEntry(path, offset, length));
                }

                return result;
            }
        }
    }
}
