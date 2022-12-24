using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterBone
    {
        public ushort ParentID { get; set; }
        public short BoneSize { get; set; }
        public List<byte> BoneData { get; set; }

        public MonsterBone(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                ParentID = reader.ReadUInt16();
                BoneSize = reader.ReadInt16();
                BoneData = reader.ReadBytes(44).ToList();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(ParentID));
            result.AddRange(BitConverter.GetBytes(BoneSize));
            result.AddRange(BoneData);
            return result;
        }
    }
}
