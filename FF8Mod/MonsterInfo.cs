using System;
using System.IO;

namespace FF8Mod
{
    public class MonsterInfo
    {
        public string Name;
        public byte[] Hp, Str, Mag, Vit, Spr, Spd, Eva;
        public Ability[] AbilitiesLow, AbilitiesMed, AbilitiesHigh;
        public byte MedStart, HighStart;
        public byte MysteryFlags1, MysteryFlags2;
        public byte StatusFlags1, StatusFlags2;
        public byte CardDrop, CardMorph, CardRareMorph;
        public byte DevourLow, DevourMed, DevourHigh;
        public ushort Exp, ExtraExp;
        public byte Ap;
        public byte[] DrawLow, DrawMed, DrawHigh;
        public HeldItem[] MugLow, MugMed, MugHigh;
        public HeldItem[] DropLow, DropMed, DropHigh;
        public byte MugRate, DropRate;
        public byte[] MysteryData;
        public byte[] ElemDef, StatusDef;

        public MonsterInfo()
        {
            Name = "Unnamed";

            Hp = new byte[4];
            Str = new byte[4];
            Mag = new byte[4];
            Vit = new byte[4];
            Spr = new byte[4];
            Spd = new byte[4];
            Eva = new byte[4];

            AbilitiesLow = new Ability[16];
            AbilitiesMed = new Ability[16];
            AbilitiesHigh = new Ability[16];

            DrawLow = new byte[4];
            DrawMed = new byte[4];
            DrawHigh = new byte[4];

            MugLow = new HeldItem[4];
            MugMed = new HeldItem[4];
            MugHigh = new HeldItem[4];

            DropLow = new HeldItem[4];
            DropMed = new HeldItem[4];
            DropHigh = new HeldItem[4];

            MysteryData = new byte[16];
            ElemDef = new byte[8];
            StatusDef = new byte[20];
        }

        public MonsterInfo(string name) : this()
        {
            Name = name;
        }

        public MonsterInfo(byte[] data) : this()
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                Name = FF8String.Decode(reader.ReadBytes(24));

                Hp = reader.ReadBytes(4);
                Str = reader.ReadBytes(4);
                Vit = reader.ReadBytes(4);
                Mag = reader.ReadBytes(4);
                Spr = reader.ReadBytes(4);
                Spd = reader.ReadBytes(4);
                Eva = reader.ReadBytes(4);

                for (int i = 0; i < 16; i++) AbilitiesLow[i] = new Ability(reader.ReadBytes(4));
                for (int i = 0; i < 16; i++) AbilitiesMed[i] = new Ability(reader.ReadBytes(4));
                for (int i = 0; i < 16; i++) AbilitiesHigh[i] = new Ability(reader.ReadBytes(4));

                MedStart = reader.ReadByte();
                HighStart = reader.ReadByte();
                MysteryFlags1 = reader.ReadByte();
                StatusFlags1 = reader.ReadByte();

                CardDrop = reader.ReadByte();
                CardMorph = reader.ReadByte();
                CardRareMorph = reader.ReadByte();

                DevourLow = reader.ReadByte();
                DevourMed = reader.ReadByte();
                DevourHigh = reader.ReadByte();

                StatusFlags2 = reader.ReadByte();
                MysteryFlags2 = reader.ReadByte();

                ExtraExp = reader.ReadUInt16();
                Exp = reader.ReadUInt16();

                for (int i = 0; i < 4; i++)
                {
                    DrawLow[i] = reader.ReadByte();
                    reader.ReadByte(); // padding
                }

                for (int i = 0; i < 4; i++)
                {
                    DrawMed[i] = reader.ReadByte();
                    reader.ReadByte(); // padding
                }

                for (int i = 0; i < 4; i++)
                {
                    DrawHigh[i] = reader.ReadByte();
                    reader.ReadByte(); // padding
                }

                for (int i = 0; i < 4; i++) MugLow[i] = new HeldItem(reader.ReadBytes(2));
                for (int i = 0; i < 4; i++) MugMed[i] = new HeldItem(reader.ReadBytes(2));
                for (int i = 0; i < 4; i++) MugHigh[i] = new HeldItem(reader.ReadBytes(2));

                for (int i = 0; i < 4; i++) DropLow[i] = new HeldItem(reader.ReadBytes(2));
                for (int i = 0; i < 4; i++) DropMed[i] = new HeldItem(reader.ReadBytes(2));
                for (int i = 0; i < 4; i++) DropHigh[i] = new HeldItem(reader.ReadBytes(2));

                MugRate = reader.ReadByte();
                DropRate = reader.ReadByte();

                reader.ReadByte(); // padding
                Ap = reader.ReadByte();

                MysteryData = reader.ReadBytes(16);
                ElemDef = reader.ReadBytes(8);
                StatusDef = reader.ReadBytes(20);
            }
        }

        public byte[] Encode()
        {
            var result = new byte[380];
            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                var encodedName = FF8String.Encode(Name);
                var resizedName = new byte[24];
                Array.Copy(encodedName, resizedName, Math.Min(encodedName.Length, 24));
                writer.Write(resizedName);

                writer.Write(Hp);
                writer.Write(Str);
                writer.Write(Vit);
                writer.Write(Mag);
                writer.Write(Spr);
                writer.Write(Spd);
                writer.Write(Eva);

                for (int i = 0; i < 16; i++) writer.Write(AbilitiesLow[i].Encode());
                for (int i = 0; i < 16; i++) writer.Write(AbilitiesMed[i].Encode());
                for (int i = 0; i < 16; i++) writer.Write(AbilitiesHigh[i].Encode());

                writer.Write(MedStart);
                writer.Write(HighStart);
                writer.Write(MysteryFlags1);
                writer.Write(StatusFlags1);

                writer.Write(CardDrop);
                writer.Write(CardMorph);
                writer.Write(CardRareMorph);

                writer.Write(DevourLow);
                writer.Write(DevourMed);
                writer.Write(DevourHigh);

                writer.Write(StatusFlags2);
                writer.Write(MysteryFlags2);
                writer.Write(ExtraExp);
                writer.Write(Exp);

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(DrawLow[i]);
                    writer.Write((byte)0);
                }

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(DrawMed[i]);
                    writer.Write((byte)0);
                }

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(DrawHigh[i]);
                    writer.Write((byte)0);
                }

                for (int i = 0; i < 4; i++) writer.Write(MugLow[i].Encode());
                for (int i = 0; i < 4; i++) writer.Write(MugMed[i].Encode());
                for (int i = 0; i < 4; i++) writer.Write(MugHigh[i].Encode());

                for (int i = 0; i < 4; i++) writer.Write(DropLow[i].Encode());
                for (int i = 0; i < 4; i++) writer.Write(DropMed[i].Encode());
                for (int i = 0; i < 4; i++) writer.Write(DropHigh[i].Encode());

                writer.Write(MugRate);
                writer.Write(DropRate);
                writer.Write((byte)0);
                writer.Write(Ap);
                writer.Write(MysteryData);
                writer.Write(ElemDef);
                writer.Write(StatusDef);
            }
            return result;
        }

        public void CopyStats(MonsterInfo source)
        {
            source.Hp.CopyTo(Hp, 0);
            source.Str.CopyTo(Str, 0);
            source.Mag.CopyTo(Mag, 0);
            source.Vit.CopyTo(Vit, 0);
            source.Spr.CopyTo(Spr, 0);
            source.Spd.CopyTo(Spd, 0);
            source.Eva.CopyTo(Eva, 0);
        }

        // hp2 & hp4 determine the lower boundary
        // mainly hp4 (thousands) with hp2 (tens) for fine tuning
        // hp1 & hp3 determine growth rate
        // hp1 sets the curve & hp3 adds a linear increase
        // there's no easy way to explain this in the ui is there...
        public int HpAtLevel(int level)
        {
            return (Hp[0] * level * level / 20) + (Hp[0] + Hp[2] * 100) * level + Hp[1] * 10 + Hp[3] * 1000;
        }

        private int OffensiveStatAtLevel(int level, byte[] values)
        {
            return (level * values[0] / 10 + level / values[1] - level * level / 2 / values[3] + values[2]) / 4;
        }

        private int NonOffensiveStatAtLevel(int level, byte[] values)
        {
            return level / values[1] - level / values[3] + level * values[0] + values[2];
        }

        public int StrAtLevel(int level)
        {
            return OffensiveStatAtLevel(level, Str);
        }

        public int MagAtLevel(int level)
        {
            return OffensiveStatAtLevel(level, Mag);
        }

        public int VitAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Vit);
        }

        public int SprAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Spr);
        }

        public int SpdAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Spd);
        }

        public int EvaAtLevel(int level)
        {
            return NonOffensiveStatAtLevel(level, Eva);
        }
    }

    public class Ability
    {
        public byte Type;
        public byte Something;
        public ushort AbilityId;

        public Ability(byte[] data)
        {
            Type = data[0];
            Something = data[1];
            AbilityId = BitConverter.ToUInt16(data, 2);
        }

        public byte[] Encode()
        {
            var result = new byte[4];
            result[0] = Type;
            result[1] = Something;
            Array.Copy(BitConverter.GetBytes(AbilityId), 0, result, 2, 2);
            return result;
        }
    }

    public class HeldItem
    {
        public byte ItemId;
        public byte Quantity;

        public HeldItem()
        {
            ItemId = 0;
            Quantity = 0;
        }

        public HeldItem(byte[] data)
        {
            ItemId = data[0];
            Quantity = data[1];
        }

        public byte[] Encode()
        {
            return new byte[2] { ItemId, Quantity };
        }
    }
}
