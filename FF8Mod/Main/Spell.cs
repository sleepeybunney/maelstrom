using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Main
{
    public class Spell
    {
        public ushort NameOffset { get; set; }
        public ushort DescriptionOffset { get; set; }
        public ushort SpellID { get; set; }
        public byte Unknown1 { get; set; }
        public byte AttackType { get; set; }
        public byte Power { get; set; }
        public byte Unknown2 { get; set; }
        public BitArray TargetFlags { get; set; }
        public BitArray AttackFlags { get; set; }
        public byte DrawResist { get; set; }
        public byte HitCount { get; set; }
        public byte Element { get; set; }
        public byte Unknown3 { get; set; }
        public BitArray StatusFlags { get; set; }
        public byte StatusPower { get; set; }
        public byte HpJunction { get; set; }
        public byte StrJunction { get; set; }
        public byte VitJunction { get; set; }
        public byte MagJunction { get; set; }
        public byte SprJunction { get; set; }
        public byte SpdJunction { get; set; }
        public byte EvaJunction { get; set; }
        public byte HitJunction { get; set; }
        public byte LckJunction { get; set; }
        public BitArray ElemAtkFlags { get; set; }
        public byte ElemAtkJunction { get; set; }
        public BitArray ElemDefFlags { get; set; }
        public byte ElemDefJunction { get; set; }
        public byte StatusAtkJunction { get; set; }
        public byte StatusDefJunction { get; set; }
        public BitArray StatusAtkFlags { get; set; }
        public BitArray StatusDefFlags { get; set; }
        public List<byte> Compatibility { get; set; }
        public ushort Unknown4 { get; set; }
        public string Name { get; set; }

        public Spell(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                DescriptionOffset = reader.ReadUInt16();
                SpellID = reader.ReadUInt16();
                Unknown1 = reader.ReadByte();
                AttackType = reader.ReadByte();
                Power = reader.ReadByte();
                Unknown2 = reader.ReadByte();
                TargetFlags = new BitArray(reader.ReadBytes(1));
                AttackFlags = new BitArray(reader.ReadBytes(1));
                DrawResist = reader.ReadByte();
                HitCount = reader.ReadByte();
                Element = reader.ReadByte();
                Unknown3 = reader.ReadByte();
                StatusFlags = new BitArray(reader.ReadBytes(6));
                StatusPower = reader.ReadByte();
                HpJunction = reader.ReadByte();
                StrJunction = reader.ReadByte();
                VitJunction = reader.ReadByte();
                MagJunction = reader.ReadByte();
                SprJunction = reader.ReadByte();
                SpdJunction = reader.ReadByte();
                EvaJunction = reader.ReadByte();
                HitJunction = reader.ReadByte();
                LckJunction = reader.ReadByte();
                ElemAtkFlags = new BitArray(reader.ReadBytes(1));
                ElemAtkJunction = reader.ReadByte();
                ElemDefFlags = new BitArray(reader.ReadBytes(1));
                ElemDefJunction = reader.ReadByte();
                StatusAtkJunction = reader.ReadByte();
                StatusDefJunction = reader.ReadByte();
                StatusAtkFlags = new BitArray(reader.ReadBytes(2));
                StatusDefFlags = new BitArray(reader.ReadBytes(2));
                Compatibility = reader.ReadBytes(16).ToList();
                Unknown4 = reader.ReadUInt16();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.AddRange(BitConverter.GetBytes(DescriptionOffset));
            result.AddRange(BitConverter.GetBytes(SpellID));

            result.Add(Unknown1);
            result.Add(AttackType);
            result.Add(Power);
            result.Add(Unknown2);

            var flags = new byte[2];
            TargetFlags.CopyTo(flags, 0);
            AttackFlags.CopyTo(flags, 1);
            result.AddRange(flags);

            result.Add(DrawResist);
            result.Add(HitCount);
            result.Add(Element);
            result.Add(Unknown3);

            var statuses = new byte[6];
            StatusFlags.CopyTo(statuses, 0);
            result.AddRange(statuses);

            result.Add(StatusPower);
            result.Add(HpJunction);
            result.Add(StrJunction);
            result.Add(VitJunction);
            result.Add(MagJunction);
            result.Add(SprJunction);
            result.Add(SpdJunction);
            result.Add(EvaJunction);
            result.Add(HitJunction);
            result.Add(LckJunction);

            var elemAtk = new byte[1];
            ElemAtkFlags.CopyTo(elemAtk, 0);
            result.AddRange(elemAtk);

            result.Add(ElemAtkJunction);

            var elemDef = new byte[1];
            ElemDefFlags.CopyTo(elemDef, 0);
            result.AddRange(elemDef);

            result.Add(ElemDefJunction);
            result.Add(StatusAtkJunction);
            result.Add(StatusDefJunction);

            var statusAtkDef = new byte[4];
            StatusAtkFlags.CopyTo(statusAtkDef, 0);
            StatusDefFlags.CopyTo(statusAtkDef, 2);
            result.AddRange(statusAtkDef);

            result.AddRange(Compatibility);
            result.AddRange(BitConverter.GetBytes(Unknown4));

            return result;
        }
    }
}
