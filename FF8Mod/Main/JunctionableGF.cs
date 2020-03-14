using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FF8Mod.Main
{
    public class JunctionableGF
    {
        public UInt16 AttackNameOffset;
        public UInt16 AttackDescOffset;
        public UInt16 MagicID;
        public byte AttackType;
        public byte AttackPower;
        public UInt16 Unknown1;
        public byte AttackFlags;
        public UInt16 Unknown2;
        public byte Element;
        public byte[] StatusFlags;
        public byte HPMod;
        public byte[] Unknown3;
        public byte StatusAttackEnabler;
        public GFAbility[] Abilities;
        public byte[] Compatibility;
        public UInt16 Unknown4;
        public byte PowerMod;
        public byte LevelMod;

        public JunctionableGF(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                AttackNameOffset = reader.ReadUInt16();
                AttackDescOffset = reader.ReadUInt16();
                MagicID = reader.ReadUInt16();
                AttackType = reader.ReadByte();
                AttackPower = reader.ReadByte();
                Unknown1 = reader.ReadUInt16();
                AttackFlags = reader.ReadByte();
                Unknown2 = reader.ReadUInt16();
                Element = reader.ReadByte();
                StatusFlags = reader.ReadBytes(6);
                HPMod = reader.ReadByte();
                Unknown3 = reader.ReadBytes(6);
                StatusAttackEnabler = reader.ReadByte();

                Abilities = new GFAbility[21];
                for (int i = 0; i < 21; i++)
                {
                    Abilities[i] = new GFAbility(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                }

                Compatibility = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    Compatibility[i] = reader.ReadByte();
                }

                Unknown4 = reader.ReadUInt16();
                PowerMod = reader.ReadByte();
                LevelMod = reader.ReadByte();
            }
        }

        public byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(AttackNameOffset));
            result.AddRange(BitConverter.GetBytes(AttackDescOffset));
            result.AddRange(BitConverter.GetBytes(MagicID));
            result.Add(AttackType);
            result.Add(AttackPower);
            result.AddRange(BitConverter.GetBytes(Unknown1));
            result.Add(AttackFlags);
            result.AddRange(BitConverter.GetBytes(Unknown2));
            result.Add(Element);
            result.AddRange(StatusFlags);
            result.Add(HPMod);
            result.AddRange(Unknown3);
            result.Add(StatusAttackEnabler);
            foreach (var a in Abilities) result.AddRange(a.Encode());
            result.AddRange(Compatibility);
            result.AddRange(BitConverter.GetBytes(Unknown4));
            result.Add(PowerMod);
            result.Add(LevelMod);
            return result.ToArray();
        }
    }
}
