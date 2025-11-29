using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Main
{
    public class NonBattleItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ushort NameOffset { get; set; }
        public ushort DescriptionOffset { get; set; }

        public NonBattleItem(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                DescriptionOffset = reader.ReadUInt16();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.AddRange(BitConverter.GetBytes(DescriptionOffset));
            return result;
        }
    }
}
