using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Main
{
    public class Weapon
    {
        public ushort NameOffset { get; set; }
        public byte RenzokukenFinishers { get; set; }
        public byte Unknown { get; set; }
        public byte CharacterID { get; set; }
        public byte AttackType { get; set; }
        public byte AttackPower { get; set; }
        public byte AttackParameter { get; set; }
        public byte StrBonus { get; set; }
        public byte WeaponTier { get; set; }
        public byte CritBonus { get; set; }
        public byte Melee { get; set; }
        public string Name { get; set; }

        public Weapon(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                RenzokukenFinishers = reader.ReadByte();
                Unknown = reader.ReadByte();
                CharacterID = reader.ReadByte();
                AttackType = reader.ReadByte();
                AttackPower = reader.ReadByte();
                AttackParameter = reader.ReadByte();
                StrBonus = reader.ReadByte();
                WeaponTier = reader.ReadByte();
                CritBonus = reader.ReadByte();
                Melee = reader.ReadByte();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.Add(RenzokukenFinishers);
            result.Add(Unknown);
            result.Add(CharacterID);
            result.Add(AttackType);
            result.Add(AttackPower);
            result.Add(AttackParameter);
            result.Add(StrBonus);
            result.Add(WeaponTier);
            result.Add(CritBonus);
            result.Add(Melee);
            return result;
        }
    }
}
