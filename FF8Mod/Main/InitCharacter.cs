using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;

namespace Sleepey.FF8Mod.Main
{
    public class InitCharacter
    {
        public short CurrentHP { get; set; }
        public short MaxHP { get; set; }
        public int Exp { get; set; }
        public byte ModelID { get; set; }
        public byte WeaponID { get; set; }
        public byte Str { get; set; }
        public byte Vit { get; set; }
        public byte Mag { get; set; }
        public byte Spr { get; set; }
        public byte Spd { get; set; }
        public byte Lck { get; set; }
        public List<byte> Spells { get; set; }
        public List<byte> Commands { get; set; }
        public List<byte> Abilities { get; set; }
        public BitArray GFs { get; set; }
        public byte Unknown1 { get; set; }
        public byte AltModel { get; set; }
        public byte HPJunction { get; set; }
        public byte StrJunction { get; set; }
        public byte VitJunction { get; set; }
        public byte MagJunction { get; set; }
        public byte SprJunction { get; set; }
        public byte SpdJunction { get; set; }
        public byte EvaJunction { get; set; }
        public byte HitJunction { get; set; }
        public byte LckJunction { get; set; }
        public byte ElemAtkJunction { get; set; }
        public byte StatusAtkJunction { get; set; }
        public List<byte> ElemDefJunction { get; set; }
        public List<byte> StatusDefJunction { get; set; }
        public byte Unknown2 { get; set; }
        public List<byte> GFCompatibility { get; set; }
        public short KillCount { get; set; }
        public short KOCount { get; set; }
        public byte Exists { get; set; }
        public byte Unknown3 { get; set; }
        public byte Status { get; set; }
        public byte Unknown4 { get; set; }

        public InitCharacter(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                CurrentHP = reader.ReadInt16();
                MaxHP = reader.ReadInt16();
                Exp = reader.ReadInt32();
                ModelID = reader.ReadByte();
                WeaponID = reader.ReadByte();
                Str = reader.ReadByte();
                Vit = reader.ReadByte();
                Mag = reader.ReadByte();
                Spr = reader.ReadByte();
                Spd = reader.ReadByte();
                Lck = reader.ReadByte();
                Spells = reader.ReadBytes(64).ToList();
                Commands = reader.ReadBytes(4).ToList();
                Abilities = reader.ReadBytes(4).ToList();
                GFs = new BitArray(reader.ReadBytes(2));
                Unknown1 = reader.ReadByte();
                AltModel = reader.ReadByte();
                HPJunction = reader.ReadByte();
                StrJunction = reader.ReadByte();
                VitJunction = reader.ReadByte();
                MagJunction = reader.ReadByte();
                SprJunction = reader.ReadByte();
                SpdJunction = reader.ReadByte();
                EvaJunction = reader.ReadByte();
                HitJunction = reader.ReadByte();
                LckJunction = reader.ReadByte();
                ElemAtkJunction = reader.ReadByte();
                StatusAtkJunction = reader.ReadByte();
                ElemDefJunction = reader.ReadBytes(4).ToList();
                StatusDefJunction = reader.ReadBytes(4).ToList();
                Unknown2 = reader.ReadByte();
                GFCompatibility = reader.ReadBytes(32).ToList();
                KillCount = reader.ReadInt16();
                KOCount = reader.ReadInt16();
                Exists = reader.ReadByte();
                Unknown3 = reader.ReadByte();
                Status = reader.ReadByte();
                Unknown4 = reader.ReadByte();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(CurrentHP));
            result.AddRange(BitConverter.GetBytes(MaxHP));
            result.AddRange(BitConverter.GetBytes(Exp));

            result.Add(ModelID);
            result.Add(WeaponID);
            result.Add(Str);
            result.Add(Vit);
            result.Add(Mag);
            result.Add(Spr);
            result.Add(Spd);
            result.Add(Lck);

            result.AddRange(Spells);
            result.AddRange(Commands);
            result.AddRange(Abilities);

            var gfs = new byte[2];
            GFs.CopyTo(gfs, 0);
            result.AddRange(gfs);

            result.Add(Unknown1);
            result.Add(AltModel);
            result.Add(HPJunction);
            result.Add(StrJunction);
            result.Add(VitJunction);
            result.Add(MagJunction);
            result.Add(SprJunction);
            result.Add(SpdJunction);
            result.Add(EvaJunction);
            result.Add(HitJunction);
            result.Add(LckJunction);
            result.Add(ElemAtkJunction);
            result.Add(StatusAtkJunction);

            result.AddRange(ElemDefJunction);
            result.AddRange(StatusDefJunction);

            result.Add(Unknown2);

            result.AddRange(GFCompatibility);
            result.AddRange(BitConverter.GetBytes(KillCount));
            result.AddRange(BitConverter.GetBytes(KOCount));

            result.Add(Exists);
            result.Add(Unknown3);
            result.Add(Status);
            result.Add(Unknown4);

            return result;
        }
    }
}
