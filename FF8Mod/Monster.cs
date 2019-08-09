using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8Mod
{
    public class Monster
    {
        public MonsterInfo Info;
        public MonsterAI AI;

        // save other sections as raw data to slot back in when rebuilding the file
        public byte[] SectionsOneToSix, SectionsNineToEleven;

        // currently only able to re-encode the text subsection so save some info & AI data too, for now
        public byte[] TempInfoData, TempAIData;
        public Dictionary<SectionIndex, Section> TempSectionInfo;

        public static Monster FromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Monster file not found");
            }

            return FromBytes(File.ReadAllBytes(path));
        }

        public static Monster FromSource(FileSource source, string path)
        {
            return FromBytes(source.GetFile(path));
        }

        public static Monster FromBytes(byte[] data)
        {
            var monster = new Monster();
            var sections = GetSectionInfo(data);    // offsets & sizes of each section

            if (sections.Count != 11)
            {
                throw new InvalidDataException("Invalid monster file (or you're trying to open Ultimecia's butt which is a special case I don't handle yet)");
            }

            // 1-6
            var oneToSixLength = (int)sections.Values.Where(s => (int)s.Type < 7).Sum(s => s.Length);
            monster.SectionsOneToSix = new ArraySegment<byte>(data, sections[SectionIndex.Skeleton].Offset, oneToSixLength).ToArray();

            // 7
            monster.TempInfoData = new ArraySegment<byte>(data, sections[SectionIndex.Info].Offset, sections[SectionIndex.Info].Length).ToArray();
            monster.Info = new MonsterInfo(monster.TempInfoData);

            // 8
            monster.TempAIData = new ArraySegment<byte>(data, sections[SectionIndex.AI].Offset, sections[SectionIndex.AI].Length).ToArray();
            monster.AI = new MonsterAI(monster.TempAIData);

            // 9-11
            var nineToElevenLength = data.Length - sections[(SectionIndex)9].Offset;
            monster.SectionsNineToEleven = new ArraySegment<byte>(data, sections[SectionIndex.Sounds].Offset, nineToElevenLength).ToArray();

            monster.TempSectionInfo = sections;

            return monster;
        }

        static Dictionary<SectionIndex, Section> GetSectionInfo(byte[] data)
        {
            var sections = new List<Section>();

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // don't bother reading past the header if there aren't exactly 11 sections
                var sectionCount = reader.ReadUInt32();
                if (sectionCount != 11) return new Dictionary<SectionIndex, Section>();

                for (int i = 1; i <= sectionCount; i++)
                {
                    var newSection = new Section
                    {
                        Type = (SectionIndex)i,
                        Offset = (int)reader.ReadUInt32()
                    };

                    // use this section's offset to fill in how long the previous section was
                    if (sections.Count > 0)
                    {
                        var prevSection = sections.Last();
                        prevSection.Length = newSection.Offset - prevSection.Offset;
                    }

                    sections.Add(newSection);
                }
            }

            // index by section number
            var result = new Dictionary<SectionIndex, Section>();
            foreach (var s in sections) result.Add(s.Type, s);
            return result;
        }
        
        public byte[] Encoded
        {
            get
            {
                var encodedAI = AI.Encoded;

                uint sectionCount = 11;
                uint sectionPosLength = sectionCount * 4;
                uint headerLength = 4 + sectionPosLength + 4;
                uint totalLength = (uint)(headerLength + SectionsOneToSix.Length + TempInfoData.Length + encodedAI.Length + SectionsNineToEleven.Length);

                // the rebuilt AI section may be a different size,
                // so everything after it in the file will be displaced by some number of bytes (+/-)
                // which needs to be accounted for in the header
                var originalAILength = TempSectionInfo[SectionIndex.AI].Length;
                var postAIOffset = encodedAI.Length - originalAILength;

                var result = new byte[totalLength];
                using (var stream = new MemoryStream(result))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(sectionCount);
                    
                    for (int i = 1; i <= 11; i++)
                    {
                        var offset = TempSectionInfo[(SectionIndex)i].Offset;
                        if (i > 8) offset += postAIOffset;
                        writer.Write((uint)offset);
                    }

                    writer.Write(totalLength);

                    writer.Write(SectionsOneToSix);
                    writer.Write(TempInfoData);
                    writer.Write(encodedAI);
                    writer.Write(SectionsNineToEleven);
                }

                return result;
            }
        }

        public override string ToString()
        {
            return Info.Name;
        }

        public enum SectionIndex
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

        public class Section
        {
            public SectionIndex Type;
            public int Offset;
            public int Length;
        }
    }
}
