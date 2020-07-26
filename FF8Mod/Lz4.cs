using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using lz4;

namespace FF8Mod
{
    public static class Lz4
    {
        public static byte[] Decompress(byte[] data)
        {
            var framed = AddFrame(data);
            return LZ4Helper.Decompress(framed);
        }

        public static byte[] AddFrame(byte[] data)
        {
            var header = new byte[] { 0x04, 0x22, 0x4D, 0x18, 0x60, 0x70, 0x73 };
            var footer = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            var compressedLength = data.Length - 8;
            var outputLength = data.Length + 7;
            var result = new byte[outputLength];

            using (var resultStream = new MemoryStream(result))
            using (var writer = new BinaryWriter(resultStream))
            {
                writer.Write(header);
                writer.Write(BitConverter.GetBytes(compressedLength));
                writer.Write(data, 8, compressedLength);
                writer.Write(footer);
            }

            return result;
        }
    }
}
