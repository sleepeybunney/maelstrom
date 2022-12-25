using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod.Battle
{
    public class Monster
    {
        private Lazy<MonsterSkeleton> _skeleton;
        private Lazy<MonsterMesh> _mesh;
        private Lazy<MonsterAnimationCollection> _animations;

        private IEnumerable<byte> _skeletonData;
        private IEnumerable<byte> _meshData;
        private IEnumerable<byte> _animationsData;

        public MonsterSkeleton Skeleton { get => _skeleton.Value; }
        public MonsterMesh Mesh { get => _mesh.Value; }
        public MonsterAnimationCollection Animations { get => _animations.Value; }

        public IEnumerable<byte> SkeletonData
        {
            get
            {
                return _skeleton.IsValueCreated ? Skeleton.Encode() : _skeletonData;
            }
            set
            {
                _skeletonData = value;
                _skeleton = new Lazy<MonsterSkeleton>(() => new MonsterSkeleton(_skeletonData));
            }
        }

        public IEnumerable<byte> MeshData
        {
            get
            {
                return _mesh.IsValueCreated ? Mesh.Encode() : _meshData;
            }
            set
            {
                _meshData = value;
                _mesh = new Lazy<MonsterMesh>(() => new MonsterMesh(_meshData));
            }
        }

        public IEnumerable<byte> AnimationsData
        {
            get
            {
                return _animations.IsValueCreated ? Animations.Encode() : _animationsData;
            }
            set
            {
                _animationsData = value;
                _animations = new Lazy<MonsterAnimationCollection>(() => new MonsterAnimationCollection(_animationsData));
            }
        }

        public MonsterInfo Info { get; set; }
        public MonsterAI AI { get; set; }

        // save other sections as raw data to slot back in when rebuilding the file
        public IEnumerable<byte> SectionsFourToSix { get; set; }
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
            return string.Format(@"{0}\battle\c0m{1:d3}.dat", Env.DataPath, monsterID);
        }

        public static Monster FromBytes(IEnumerable<byte> data)
        {
            var monster = new Monster();
            var sections = GetSectionInfo(data);    // offsets & sizes of each section

            // only expecting files with 11 or 2 sections
            if (sections.Count != 11 && sections.Count != 2)
            {
                throw new InvalidDataException("Invalid monster file");
            }

            if (sections.Count == 11)
            {
                // 1-3
                monster.SkeletonData = data.Skip(sections[MonsterSectionIndex.Skeleton].Offset).Take(sections[MonsterSectionIndex.Skeleton].Length);
                monster.MeshData = data.Skip(sections[MonsterSectionIndex.Mesh].Offset).Take(sections[MonsterSectionIndex.Mesh].Length);
                monster.AnimationsData = data.Skip(sections[MonsterSectionIndex.Animation].Offset).Take(sections[MonsterSectionIndex.Animation].Length);

                // 4-6
                var fourToSixLength = sections[MonsterSectionIndex.Info].Offset - sections[MonsterSectionIndex.Section4].Offset;
                monster.SectionsFourToSix = data.Skip(sections[MonsterSectionIndex.Section4].Offset).Take(fourToSixLength);
            }

            // 7-8
            monster.Info = new MonsterInfo(data.Skip(sections[MonsterSectionIndex.Info].Offset).Take(sections[MonsterSectionIndex.Info].Length));
            monster.AI = new MonsterAI(data.Skip(sections[MonsterSectionIndex.AI].Offset).Take(sections[MonsterSectionIndex.AI].Length));

            if (sections.Count == 11)
            {
                // 9-11
                var nineToElevenLength = data.Count() - sections[(MonsterSectionIndex)9].Offset;
                monster.SectionsNineToEleven = data.Skip(sections[MonsterSectionIndex.Sounds].Offset).Take(nineToElevenLength);
            }

            monster.SectionInfo = sections;

            return monster;
        }

        static Dictionary<MonsterSectionIndex, MonsterSection> GetSectionInfo(IEnumerable<byte> data)
        {
            var sections = new List<MonsterSection>();

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                // don't bother reading past the header if the section count isn't something expected
                var sectionCount = reader.ReadUInt32();
                if (sectionCount != 11 && sectionCount != 2) return new Dictionary<MonsterSectionIndex, MonsterSection>();

                var firstSection = 1;
                if (sectionCount == 2) firstSection = 7;

                for (int i = firstSection; i < firstSection + sectionCount; i++)
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

                sections.Last().Length = (int)stream.Length - sections.Last().Offset;
            }

            // index by section number
            var result = new Dictionary<MonsterSectionIndex, MonsterSection>();
            foreach (var s in sections) result.Add(s.Type, s);
            return result;
        }

        public IEnumerable<byte> Encode()
        {
            uint sectionCount = (uint)SectionInfo.Count;
            uint sectionPosLength = sectionCount * 4;
            uint headerLength = 4 + sectionPosLength + 4;

            var encodedInfo = Info.Encode();
            var encodedAI = AI.Encode();

            uint totalLength = 0;
            IEnumerable<byte> encodedSkeleton = new byte[0];
            IEnumerable<byte> encodedMesh = new byte[0];
            IEnumerable<byte> encodedAnim = new byte[0];

            if (sectionCount == 11)
            {
                encodedSkeleton = SkeletonData;
                encodedMesh = MeshData;
                encodedAnim = AnimationsData;
                totalLength = (uint)(headerLength + encodedSkeleton.Count() + encodedMesh.Count() + encodedAnim.Count() + SectionsFourToSix.Count() + encodedInfo.Length + encodedAI.Count() + SectionsNineToEleven.Count());
            }
            else
            {
                totalLength = (uint)(headerLength + encodedInfo.Count() + encodedAI.Count());
            }

            // the rebuilt info & AI sections may be a different size,
            // so everything after them in the file will be displaced by some number of bytes (+/-)
            // which needs to be accounted for in the header
            var originalInfoLength = SectionInfo[MonsterSectionIndex.Info].Length;
            var originalAILength = SectionInfo[MonsterSectionIndex.AI].Length;
            var sizeDiff = encodedInfo.Length + encodedAI.Count() - originalInfoLength - originalAILength;

            SectionInfo[MonsterSectionIndex.Info].Length = encodedInfo.Length;
            SectionInfo[MonsterSectionIndex.AI].Length = encodedAI.Count();

            if (sectionCount == 11)
            {
                for (int i = 9; i <= 11; i++)
                {
                    SectionInfo[(MonsterSectionIndex)i].Offset += sizeDiff;
                }
            }

            var result = new byte[totalLength];
            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(sectionCount);

                var firstSection = 1;
                if (sectionCount == 2) firstSection = 7;
                for (int i = firstSection; i < firstSection + sectionCount; i++)
                {
                    writer.Write((uint)SectionInfo[(MonsterSectionIndex)i].Offset);
                }

                writer.Write(totalLength);

                if (sectionCount == 11)
                {
                    writer.Write(encodedSkeleton);
                    writer.Write(encodedMesh);
                    writer.Write(encodedAnim);
                    writer.Write(SectionsFourToSix);
                }

                writer.Write(encodedInfo);
                writer.Write(encodedAI);

                if (sectionCount == 11)
                {
                    writer.Write(SectionsNineToEleven);
                }
            }

            return result;
        }

        public override string ToString()
        {
            return Info.Name;
        }
    }
}
