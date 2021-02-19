using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod
{
    public class Monster
    {
        public MonsterInfo Info;
        public MonsterAI AI;

        // save other sections as raw data to slot back in when rebuilding the file
        public byte[] SectionsOneToSix, SectionsNineToEleven;
        public Dictionary<SectionIndex, Section> SectionInfo;

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

        public static Monster ByID(FileSource source, int monsterID)
        {
            var path = GetPath(monsterID);
            return FromSource(source, path);
        }

        public static string GetPath(int monsterID)
        {
            return string.Format(@"{0}\battle\c0m{1:d3}.dat", Globals.DataPath, monsterID);
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
            var oneToSixLength = sections.Values.Where(s => (int)s.Type < 7).Sum(s => s.Length);
            monster.SectionsOneToSix = new ArraySegment<byte>(data, sections[SectionIndex.Skeleton].Offset, oneToSixLength).ToArray();

            // 7
            monster.Info = new MonsterInfo(new ArraySegment<byte>(data, sections[SectionIndex.Info].Offset, sections[SectionIndex.Info].Length).ToArray());

            // 8
            monster.AI = new MonsterAI(new ArraySegment<byte>(data, sections[SectionIndex.AI].Offset, sections[SectionIndex.AI].Length).ToArray());

            // 9-11
            var nineToElevenLength = data.Length - sections[(SectionIndex)9].Offset;
            monster.SectionsNineToEleven = new ArraySegment<byte>(data, sections[SectionIndex.Sounds].Offset, nineToElevenLength).ToArray();

            monster.SectionInfo = sections;

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

        public byte[] Encode()
        {
            var encodedInfo = Info.Encode();
            var encodedAI = AI.Encode();

            uint sectionCount = 11;
            uint sectionPosLength = sectionCount * 4;
            uint headerLength = 4 + sectionPosLength + 4;
            uint totalLength = (uint)(headerLength + SectionsOneToSix.Length + encodedInfo.Length + encodedAI.Length + SectionsNineToEleven.Length);

            // the rebuilt info & AI sections may be a different size,
            // so everything after them in the file will be displaced by some number of bytes (+/-)
            // which needs to be accounted for in the header
            var originalInfoLength = SectionInfo[SectionIndex.Info].Length;
            var originalAILength = SectionInfo[SectionIndex.AI].Length;
            var sizeDiff = encodedInfo.Length + encodedAI.Length - originalInfoLength - originalAILength;

            SectionInfo[SectionIndex.Info].Length = encodedInfo.Length;
            SectionInfo[SectionIndex.AI].Length = encodedAI.Length;
            for (int i = 9; i <= 11; i++)
            {
                SectionInfo[(SectionIndex)i].Offset += sizeDiff;
            }

            var result = new byte[totalLength];
            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(sectionCount);
                for (int i = 1; i <= 11; i++)
                {
                    writer.Write((uint)SectionInfo[(SectionIndex)i].Offset);
                }
                writer.Write(totalLength);

                writer.Write(SectionsOneToSix);
                writer.Write(encodedInfo);
                writer.Write(encodedAI);
                writer.Write(SectionsNineToEleven);
            }

            return result;
        }

        public override string ToString()
        {
            return Info.Name;
        }
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
