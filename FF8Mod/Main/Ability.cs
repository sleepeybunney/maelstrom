using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Main
{
    public class Ability
    {
        public ushort NameOffset { get; set; }
        public ushort DescOffset { get; set; }
        public byte APCost { get; set; }
        public IList<byte> Params { get; set; }

        public Ability(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                DescOffset = reader.ReadUInt16();
                APCost = reader.ReadByte();
                Params = reader.ReadBytes(3);
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.AddRange(BitConverter.GetBytes(DescOffset));
            result.Add(APCost);
            result.AddRange(Params);
            return result;
        }
    }
}
