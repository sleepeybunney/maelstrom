using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FF8Mod.Main
{
    public class Weapon
    {
        public ushort NameOffset;
        public byte RenzokukenFinishers;
        public byte Unknown;
        public byte CharacterID;
        public byte AttackType;
        public byte AttackPower;
        public byte AttackParameter;
        public byte StrBonus;
        public byte WeaponTier;
        public byte CritBonus;
        public byte Melee;
        public string Name;

        public Weapon(byte[] data)
        {
            using (var stream = new MemoryStream(data))
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

        public byte[] Encode()
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
            return result.ToArray();
        }
    }
}
