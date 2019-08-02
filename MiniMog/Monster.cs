using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiniMog
{
    class Monster
    {
        MonsterInfo Info;
        MonsterAI AI;

        public Monster(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Monster file not found");
            }

            var rawData = File.ReadAllBytes(path);
            var sectionOffsets = GetSections(rawData);

            if (sectionOffsets.Length != 11)
            {
                throw new InvalidDataException("Invalid monster file (or you're trying to open Ultimecia's butt which is a special case I don't handle yet)");
            }

            this.Info = MonsterInfo.Load(new MemoryStream(rawData), sectionOffsets[(int)Section.Info]);
            Console.WriteLine(this.Info.Name + " - " + this.Info.HpAtLevel(6) + "HP at level 6");
            this.AI = MonsterAI.Load(new MemoryStream(rawData), sectionOffsets[(int)Section.AI]);
        }

        uint[] GetSections(byte[] data)
        {
            var sections = new List<uint>();

            using (var stream = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var sectionCount = reader.ReadUInt32();
                    if (sectionCount != 11) return new uint[] { };

                    for (uint i = 0; i < sectionCount; i++)
                    {
                        sections.Add(reader.ReadUInt32());
                        // Console.WriteLine(sections.Last());
                    }
                }
            }

            return sections.ToArray();
        }

        enum Section
        {
            Skeleton = 0,
            Mesh = 1,
            Animation = 2,
            Section4 = 3,
            Section5 = 4,
            Section6 = 5,
            Info = 6,
            AI = 7,
            Sounds = 8,
            Section10 = 9,
            Textures = 10
        }
    }
}
