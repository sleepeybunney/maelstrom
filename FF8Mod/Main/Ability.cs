using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sleepey.FF8Mod.Main
{
    public class Ability
    {
        public UInt16 NameOffset;
        public UInt16 DescOffset;
        public byte APCost;
        public byte[] Params;

        public Ability(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                DescOffset = reader.ReadUInt16();
                APCost = reader.ReadByte();
                Params = reader.ReadBytes(3);
            }
        }

        public byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.AddRange(BitConverter.GetBytes(DescOffset));
            result.Add(APCost);
            result.AddRange(Params);
            return result.ToArray();
        }
    }
}
