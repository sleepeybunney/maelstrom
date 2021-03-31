using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod.Battle
{
    public class Monster
    {
        public MonsterInfo Info { get; set; }
        public MonsterAI AI { get; set; }

        // save other sections as raw data to slot back in when rebuilding the file
        public IEnumerable<byte> SectionsOneToSix { get; set; }
        public IEnumerable<byte> SectionsNineToEleven { get; set; }
        public Dictionary<MonsterSectionIndex, MonsterSection> SectionInfo { get; set; }

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

        public static Monster FromBytes(IEnumerable<byte> data)
        {
            var monster = new Monster();
            var sections = GetSectionInfo(data);    // offsets & sizes of each section

            if (sections.Count != 11)
            {
                throw new InvalidDataException("Invalid monster file (or you're trying to open Ultimecia's butt which is a special case I don't handle yet)");
            }

            // 1-6
            var oneToSixLength = sections.Values.Where(s => (int)s.Type < 7).Sum(s => s.Length);
            monster.SectionsOneToSix = data.Skip(sections[MonsterSectionIndex.Skeleton].Offset).Take(oneToSixLength);

            // 7
            monster.Info = new MonsterInfo(data.Skip(sections[MonsterSectionIndex.Info].Offset).Take(sections[MonsterSectionIndex.Info].Length));

            // 8
            monster.AI = new MonsterAI(data.Skip(sections[MonsterSectionIndex.AI].Offset).Take(sections[MonsterSectionIndex.AI].Length));

            // 9-11
            var nineToElevenLength = data.Count() - sections[(MonsterSectionIndex)9].Offset;
            monster.SectionsNineToEleven = data.Skip(sections[MonsterSectionIndex.Sounds].Offset).Take(nineToElevenLength);

            monster.SectionInfo = sections;

            return monster;
        }

        static Dictionary<MonsterSectionIndex, MonsterSection> GetSectionInfo(IEnumerable<byte> data)
        {
            var sections = new List<MonsterSection>();

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                // don't bother reading past the header if there aren't exactly 11 sections
                var sectionCount = reader.ReadUInt32();
                if (sectionCount != 11) return new Dictionary<MonsterSectionIndex, MonsterSection>();

                for (int i = 1; i <= sectionCount; i++)
                {
                    var newSection = new MonsterSection
                    {
                        Type = (MonsterSectionIndex)i,
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
            var result = new Dictionary<MonsterSectionIndex, MonsterSection>();
            foreach (var s in sections) result.Add(s.Type, s);
            return result;
        }

        public IEnumerable<byte> Encode()
        {
            var encodedInfo = Info.Encode();
            var encodedAI = AI.Encode();

            uint sectionCount = 11;
            uint sectionPosLength = sectionCount * 4;
            uint headerLength = 4 + sectionPosLength + 4;
            uint totalLength = (uint)(headerLength + SectionsOneToSix.Count() + encodedInfo.Length + encodedAI.Count() + SectionsNineToEleven.Count());

            // the rebuilt info & AI sections may be a different size,
            // so everything after them in the file will be displaced by some number of bytes (+/-)
            // which needs to be accounted for in the header
            var originalInfoLength = SectionInfo[MonsterSectionIndex.Info].Length;
            var originalAILength = SectionInfo[MonsterSectionIndex.AI].Length;
            var sizeDiff = encodedInfo.Length + encodedAI.Count() - originalInfoLength - originalAILength;

            SectionInfo[MonsterSectionIndex.Info].Length = encodedInfo.Length;
            SectionInfo[MonsterSectionIndex.AI].Length = encodedAI.Count();
            for (int i = 9; i <= 11; i++)
            {
                SectionInfo[(MonsterSectionIndex)i].Offset += sizeDiff;
            }

            var result = new byte[totalLength];
            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(sectionCount);
                for (int i = 1; i <= 11; i++)
                {
                    writer.Write((uint)SectionInfo[(MonsterSectionIndex)i].Offset);
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
}
