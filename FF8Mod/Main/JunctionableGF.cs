using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Main
{
    public class JunctionableGF
    {
        public ushort AttackNameOffset { get; set; }
        public ushort AttackDescOffset { get; set; }
        public ushort MagicID { get; set; }
        public byte AttackType { get; set; }
        public byte AttackPower { get; set; }
        public ushort Unknown1 { get; set; }
        public byte AttackFlags { get; set; }
        public ushort Unknown2 { get; set; }
        public byte Element { get; set; }
        public List<byte> StatusFlags { get; set; }
        public byte HPMod { get; set; }
        public List<byte> Unknown3 { get; set; }
        public byte StatusAttackEnabler { get; set; }
        public List<GFAbility> Abilities { get; set; }
        public List<byte> Compatibility { get; set; }
        public ushort Unknown4 { get; set; }
        public byte PowerMod { get; set; }
        public byte LevelMod { get; set; }

        public JunctionableGF(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
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
                StatusFlags = reader.ReadBytes(6).ToList();
                HPMod = reader.ReadByte();
                Unknown3 = reader.ReadBytes(6).ToList();
                StatusAttackEnabler = reader.ReadByte();

                Abilities = new List<GFAbility>();
                for (int i = 0; i < 21; i++)
                {
                    Abilities.Add(new GFAbility(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
                }

                Compatibility = reader.ReadBytes(16).ToList();
                Unknown4 = reader.ReadUInt16();
                PowerMod = reader.ReadByte();
                LevelMod = reader.ReadByte();
            }
        }

        public IEnumerable<byte> Encode()
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
            return result;
        }
    }
}
