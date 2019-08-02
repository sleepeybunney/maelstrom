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
        public MonsterInfo Info;
        public MonsterAI AI;
        public byte[] SectionsOneToSix, SectionsNineToEleven;

        public static Monster FromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Monster file not found");
            }

            return FromBytes(File.ReadAllBytes(path));
        }

        public static Monster FromBytes(byte[] data)
        {
            var monster = new Monster();
            var sections = GetSectionInfo(data);

            if (sections.Count != 11)
            {
                throw new InvalidDataException("Invalid monster file (or you're trying to open Ultimecia's butt which is a special case I don't handle yet)");
            }

            var oneToSixLength = (int)sections.Values.Where(s => (int)s.Type < 7).Sum(s => s.Length);
            monster.SectionsOneToSix = new ArraySegment<byte>(data, sections[SectionIndex.Skeleton].Offset, oneToSixLength).ToArray();

            monster.Info = new MonsterInfo(new ArraySegment<byte>(data, sections[SectionIndex.Info].Offset, sections[SectionIndex.Info].Length).ToArray());
            Console.WriteLine(monster.Info.Name + " - " + monster.Info.HpAtLevel(6) + "HP at level 6");

            monster.AI = new MonsterAI(new ArraySegment<byte>(data, sections[SectionIndex.AI].Offset, sections[SectionIndex.AI].Length).ToArray());

            var nineToEleventLength = (int)sections.Values.Where(s => (int)s.Type > 8).Sum(s => s.Length);
            monster.SectionsNineToEleven = new ArraySegment<byte>(data, sections[SectionIndex.Sounds].Offset, nineToEleventLength).ToArray();

            return monster;
        }

        static Dictionary<SectionIndex, Section> GetSectionInfo(byte[] data)
        {
            var sections = new List<Section>();

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var sectionCount = reader.ReadUInt32();
                if (sectionCount != 11) return new Dictionary<SectionIndex, Section>();

                for (int i = 1; i <= sectionCount; i++)
                {
                    var newSection = new Section
                    {
                        Type = (SectionIndex)i,
                        Offset = (int)reader.ReadUInt32()
                    };

                    if (sections.Count > 0)
                    {
                        var prevSection = sections.Last();
                        prevSection.Length = newSection.Offset - prevSection.Offset;
                    }

                    sections.Add(newSection);
                }
            }

            var result = new Dictionary<SectionIndex, Section>();
            foreach (var s in sections) result.Add(s.Type, s);
            return result;
        }

        enum SectionIndex
        {
            Skeleton = 1,
            Mesh = 2,
            Animation = 3,
            Section4 = 4,
            Section5 = 5,
            Section6 = 6,
            Info = 7,
            AI = 8,
            Sounds = 9,
            Section10 = 10,
            Textures = 11
        }

        class Section
        {
            public SectionIndex Type;
            public int Offset;
            public int Length;
        }
    }
}
