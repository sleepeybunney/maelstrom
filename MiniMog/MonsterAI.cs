using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiniMog
{
    public class MonsterAI
    {
        public byte[] AI;
        public List<string> Strings;

        public MonsterAI()
        {
            this.Strings = new List<string>();
        }

        public MonsterAI(byte[] data) : this()
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var subSections = reader.ReadUInt32();
                var aiOffset = reader.ReadUInt32();
                var textIndexOffset = reader.ReadUInt32();
                var textOffset = reader.ReadUInt32();

                stream.Position = aiOffset;
                var aiLength = textIndexOffset - aiOffset;
                AI = reader.ReadBytes((int)aiLength);
                var ai = new BattleScript(AI);

                stream.Position = textIndexOffset;
                var textOffsets = new List<uint>();
                var textLengths = new List<uint>();
                for (int i = 0; i < (textOffset - textIndexOffset) / 2; i++)
                {
                    var newOffset = reader.ReadUInt16();
                    if (i > 0)
                    {
                        var prevOffset = textOffsets.Last();
                        if (newOffset < prevOffset) break;
                        textLengths.Add(newOffset - prevOffset);
                    }
                    textOffsets.Add(newOffset);
                }

                if (textOffsets.Count > 0)
                {
                    textLengths.Add((uint)stream.Length - (textOffset + textOffsets.Last()));
                }

                for (int i = 0; i < textOffsets.Count; i++)
                {
                    stream.Position = textOffset + textOffsets[i];
                    var newText = reader.ReadBytes((int)textLengths[i]);
                    this.Strings.Add(FF8String.Decode(newText));
                }
            }

            Console.WriteLine("TEXT");
            foreach (var s in this.Strings) Console.WriteLine(s);
        }

        public byte[] Encoded
        {
            get
            {
                var encodedStrings = Strings.ConvertAll(s => FF8String.Encode(s));

                uint subSections = 3;
                uint headerLength = (subSections + 1) * 4;
                uint aiLength = (uint)AI.Length;
                uint textIndexLength = (uint)Strings.Count * 2;
                uint textLength = (uint)encodedStrings.Sum(s => s.Length);
                uint totalLength = headerLength + aiLength + textIndexLength + textLength;

                uint aiOffset = headerLength;
                uint textIndexOffset = aiOffset + aiLength;
                uint textOffset = textIndexOffset + textIndexLength;

                byte[] result = new byte[totalLength];
                using (var stream = new MemoryStream(result))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(subSections);
                    writer.Write(aiOffset);
                    writer.Write(textIndexOffset);
                    writer.Write(textOffset);
                    writer.Write(AI);

                    ushort offset = 0;
                    for (int i = 0; i < encodedStrings.Count; i++)
                    {
                        writer.Write(offset);
                        offset += (ushort)encodedStrings[i].Length;
                    }

                    for (int i = 0; i < encodedStrings.Count; i++)
                    {
                        writer.Write(encodedStrings[i]);
                    }
                }

                return result;
            }
        }
    }
}
