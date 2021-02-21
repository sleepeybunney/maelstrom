using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using lz4;

namespace Sleepey.FF8Mod.Archive
{
    public static class Lz4
    {
        public static IEnumerable<byte> Decompress(IEnumerable<byte> data)
        {
            var framed = AddFrame(data);
            return LZ4Helper.Decompress(framed);
        }

        public static byte[] AddFrame(IEnumerable<byte> data)
        {
            var header = new byte[] { 0x04, 0x22, 0x4D, 0x18, 0x60, 0x70, 0x73 };
            var footer = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            var dataArray = data.ToArray();
            var compressedLength = dataArray.Length - 8;
            var outputLength = dataArray.Length + 7;
            var result = new byte[outputLength];

            using (var resultStream = new MemoryStream(result))
            using (var writer = new BinaryWriter(resultStream))
            {
                writer.Write(header);
                writer.Write(BitConverter.GetBytes(compressedLength));
                writer.Write(dataArray, 8, compressedLength);
                writer.Write(footer);
            }

            return result;
        }
    }
}
