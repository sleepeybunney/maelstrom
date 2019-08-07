using System;
using System.Collections.Generic;
using System.IO;

namespace FF8Mod.Maelstrom
{
    class Lzss
    {
        public static byte[] Decompress(byte[] data)
        {
            var output = new List<byte>();
            var buffer = new CircularBuffer(4096, 0xfee);

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length)
                {
                    var control = ReadControl(reader.ReadByte());
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

            return output.ToArray();
        }

        public static bool[] ReadControl(byte control)
        {
            var result = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                var bit = (byte)Math.Pow(2, i);
                int isSet = control & bit;
                result[i] = isSet == bit;
            }
            return result;
        }

        public class CircularBuffer
        {
            public byte[] Buffer;
            public int Position;

            public CircularBuffer(int length, int pos)
            {
                Buffer = new byte[length];
                for (int i = 0; i < length; i++) Buffer[i] = 0;
                Position = pos;
                while (Position >= length) Position -= length;
            }

            public void Put(byte value)
            {
                Buffer[Position] = value;
                Position++;
                if (Position == Buffer.Length) Position = 0;
            }

            public byte Get(int index)
            {
                while (index < 0) index += Buffer.Length;
                while (index >= Buffer.Length) index -= Buffer.Length;
                return Buffer[index];
            }
        }
    }
}
