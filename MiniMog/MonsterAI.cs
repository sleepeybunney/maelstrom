using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiniMog
{
    class MonsterAI
    {
        public List<string> Strings;

        public MonsterAI()
        {
            this.Strings = new List<string>();
        }

        public static MonsterAI Load(Stream stream, uint offset)
        {
            var result = new MonsterAI();
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                var subSections = reader.ReadUInt32();
                var aiOffset = reader.ReadUInt32();
                var textCountOffset = reader.ReadUInt32();
                var textOffset = reader.ReadUInt32();
                Console.WriteLine(subSections + " subsections");
                Console.WriteLine("ai offset " + aiOffset);
                Console.WriteLine("text count offset " + textCountOffset);
                Console.WriteLine("text offset " + textOffset);

                stream.Seek(offset + textCountOffset, SeekOrigin.Begin);
                var textOffsets = new List<uint>();
                for (int i = 0; i < (textOffset - textCountOffset) / 4; i++)
                {
                    textOffsets.Add(reader.ReadUInt16());
                }
                Console.WriteLine("text offsets:");
                foreach (var o in textOffsets) Console.WriteLine(o);

                foreach (var o in textOffsets)
                {
                    stream.Seek(offset + textOffset + o, SeekOrigin.Begin);
                    var text = new List<byte>();
                    int bytes = 0;
                    byte next;
                    while (bytes < 65536)
                    {
                        next = reader.ReadByte();
                        if (next == 0) break;
                        text.Add(next);
                        bytes++;
                    }
                    result.Strings.Add(FF8String.Decode(text.ToArray()));
                    Console.WriteLine(result.Strings.Last());
                }
            }
            return result;
        }
    }
}
