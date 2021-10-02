using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Main
{
    public class BattleCommand
    {
        public ushort NameOffset { get; set; }
        public ushort DescriptionOffset { get; set; }
        public byte AbilityID { get; set; }
        public byte Flags { get; set; }
        public byte Target { get; set; }
        public byte Unknown { get; set; }

        public BattleCommand(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                DescriptionOffset = reader.ReadUInt16();
                AbilityID = reader.ReadByte();
                Flags = reader.ReadByte();
                Target = reader.ReadByte();
                Unknown = reader.ReadByte();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.AddRange(BitConverter.GetBytes(DescriptionOffset));
            result.Add(AbilityID);
            result.Add(Flags);
            result.Add(Target);
            result.Add(Unknown);
            return result;
        }
    }
}
