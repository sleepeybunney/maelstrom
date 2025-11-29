using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Main
{
    public class BattleItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ushort NameOffset { get; set; }
        public ushort DescriptionOffset { get; set; }
        public ushort MagicID { get; set; }
        public byte AttackType { get; set; }
        public byte AttackPower { get; set; }
        public byte BattleFlag { get; set; }
        public byte TargetInfo { get; set; }
        public byte Unknown { get; set; }
        public byte AttackFlags { get; set; }
        public byte Unknown2 { get; set; }
        public byte StatusAttackEnabler { get; set; }
        public ushort StatusFlags1 { get; set; }
        public uint StatusFlags2 { get; set; }
        public byte AttackParam { get; set; }
        public byte Unknown3 { get; set; }
        public byte HitCount { get; set; }
        public byte Element { get; set; }

        public BattleItem(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                DescriptionOffset = reader.ReadUInt16();
                MagicID = reader.ReadUInt16();
                AttackType = reader.ReadByte();
                AttackPower = reader.ReadByte();
                BattleFlag = reader.ReadByte();
                TargetInfo = reader.ReadByte();
                Unknown = reader.ReadByte();
                AttackFlags = reader.ReadByte();
                Unknown2 = reader.ReadByte();
                StatusAttackEnabler = reader.ReadByte();
                StatusFlags1 = reader.ReadUInt16();
                StatusFlags2 = reader.ReadUInt32();
                AttackParam = reader.ReadByte();
                Unknown3 = reader.ReadByte();
                HitCount = reader.ReadByte();
                Element = reader.ReadByte();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.AddRange(BitConverter.GetBytes(DescriptionOffset));
            result.AddRange(BitConverter.GetBytes(MagicID));
            result.Add(AttackType);
            result.Add(AttackPower);
            result.Add(BattleFlag);
            result.Add(TargetInfo);
            result.Add(Unknown);
            result.Add(AttackFlags);
            result.Add(Unknown2);
            result.Add(StatusAttackEnabler);
            result.AddRange(BitConverter.GetBytes(StatusFlags1));
            result.AddRange(BitConverter.GetBytes(StatusFlags2));
            result.Add(AttackParam);
            result.Add(Unknown3);
            result.Add(HitCount);
            result.Add(Element);
            return result;
        }
    }
}
