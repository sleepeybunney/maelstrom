using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Archive
{
    public static class Lzss
    {
        public static IEnumerable<byte> Decompress(IEnumerable<byte> data)
        {
            var output = new List<byte>();
            var buffer = new CircularBuffer(4096, 0xfee);

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length)
                {
                    var control = new BitArray(reader.ReadBytes(1));
                    var blockLength = 0;
                    var blockCount = 0;
                    var remainingStream = stream.Length - stream.Position;

                    if (remainingStream >= 16) blockCount = 8;
                    else for (int i = 0; i < 8; i++)
                    {
                        var valueLength = 1;
                        if (!control[i]) valueLength++;
                        if (blockLength + valueLength > remainingStream) break;
                        blockLength += valueLength;
                        blockCount++;
                    }

                    for (int i = 0; i < blockCount; i++)
                    {
                        if (control[i])
                        {
                            var b = reader.ReadByte();
                            output.Add(b);
                            buffer.Put(b);
                            continue;
                        }

                        var reference = reader.ReadBytes(2);
                        var refLength = (reference[1] & 0x0F) + 3;
                        var refOffset = reference[0] + ((reference[1] & 0xF0) << 4);

                        for (int j = 0; j < refLength; j++)
                        {
                            var b = buffer.Get(refOffset + j);
                            output.Add(b);
                            buffer.Put(b);
                        }
                    }
                }
            }

            return output;
        }

        private class CircularBuffer
        {
            public List<byte> Buffer { get; set; }
            public int Position { get; set; }

            public CircularBuffer(int length, int pos)
            {
                Buffer = Enumerable.Repeat<byte>(0, length).ToList();
                Position = pos;
                while (Position >= length) Position -= length;
            }

            public void Put(byte value)
            {
                Buffer[Position] = value;
                Position++;
                if (Position == Buffer.Count) Position = 0;
            }

            public byte Get(int index)
            {
                while (index < 0) index += Buffer.Count;
                while (index >= Buffer.Count) index -= Buffer.Count;
                return Buffer[index];
            }
        }
    }
}
