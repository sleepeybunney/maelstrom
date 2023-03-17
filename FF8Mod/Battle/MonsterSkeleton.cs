using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterSkeleton
    {
        public short Size { get; set; }
        public short Unknown1a { get; set; }
        public short Unknown1b { get; set; }
        public short ScaleX { get; set; }
        public short ScaleY { get; set; }
        public short ScaleZ { get; set; }
        public short Unknown2 { get; set; }
        public List<MonsterBone> Bones { get; set; }

        public MonsterSkeleton(IEnumerable<byte> data)
        {
            Bones = new List<MonsterBone>();

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                var boneCount = reader.ReadUInt16();

                Size = reader.ReadInt16();
                Unknown1a = reader.ReadInt16();
                Unknown1b = reader.ReadInt16();
                ScaleX = reader.ReadInt16();
                ScaleZ = reader.ReadInt16();
                ScaleY = reader.ReadInt16();
                Unknown2 = reader.ReadInt16();

                for (int i = 0; i < boneCount; i++)
                {
                    Bones.Add(new MonsterBone(reader.ReadBytes(48)));
                }
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((ushort)Bones.Count));
            result.AddRange(BitConverter.GetBytes(Size));
            result.AddRange(BitConverter.GetBytes(Unknown1a));
            result.AddRange(BitConverter.GetBytes(Unknown1b));
            result.AddRange(BitConverter.GetBytes(ScaleX));
            result.AddRange(BitConverter.GetBytes(ScaleZ));
            result.AddRange(BitConverter.GetBytes(ScaleY));
            result.AddRange(BitConverter.GetBytes(Unknown2));
            foreach (var bone in Bones) result.AddRange(bone.Encode());
            return result;
        }
    }
}
