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

                for (int i = 0; i < 16; i++) AbilitiesLow[i] = new Ability
                {
                    Type = reader.ReadByte(),
                    Something = reader.ReadByte(),
                    AbilityId = reader.ReadUInt16()
                };

                for (int i = 0; i < 16; i++) AbilitiesMed[i] = new Ability
                {
                    Type = reader.ReadByte(),
                    Something = reader.ReadByte(),
                    AbilityId = reader.ReadUInt16()
                };

                for (int i = 0; i < 16; i++) AbilitiesHigh[i] = new Ability
                {
                    Type = reader.ReadByte(),
                    Something = reader.ReadByte(),
                    AbilityId = reader.ReadUInt16()
                };

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

                for (int i = 0; i < 4; i++) MugLow[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                for (int i = 0; i < 4; i++) MugMed[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                for (int i = 0; i < 4; i++) MugHigh[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                for (int i = 0; i < 4; i++) DropLow[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                for (int i = 0; i < 4; i++) DropMed[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                for (int i = 0; i < 4; i++) DropHigh[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

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

                for (int i = 0; i < 16; i++)
                {
                    writer.Write(AbilitiesLow[i].Type);
                    writer.Write(AbilitiesLow[i].Something);
                    writer.Write(AbilitiesLow[i].AbilityId);
                }

                for (int i = 0; i < 16; i++)
                {
                    writer.Write(AbilitiesMed[i].Type);
                    writer.Write(AbilitiesMed[i].Something);
                    writer.Write(AbilitiesMed[i].AbilityId);
                }

                for (int i = 0; i < 16; i++)
                {
                    writer.Write(AbilitiesHigh[i].Type);
                    writer.Write(AbilitiesHigh[i].Something);
                    writer.Write(AbilitiesHigh[i].AbilityId);
                }

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

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(MugLow[i].ItemId);
                    writer.Write(MugLow[i].Quantity);
                }

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(MugMed[i].ItemId);
                    writer.Write(MugMed[i].Quantity);
                }

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(MugHigh[i].ItemId);
                    writer.Write(MugHigh[i].Quantity);
                }

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(DropLow[i].ItemId);
                    writer.Write(DropLow[i].Quantity);
                }

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(DropMed[i].ItemId);
                    writer.Write(DropMed[i].Quantity);
                }

                for (int i = 0; i < 4; i++)
                {
                    writer.Write(DropHigh[i].ItemId);
                    writer.Write(DropHigh[i].Quantity);
                }

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
            Hp[0] = source.Hp[0];
            Hp[1] = source.Hp[1];
            Hp[2] = source.Hp[2];
            Hp[3] = source.Hp[3];

            Str[0] = source.Str[0];
            Str[1] = source.Str[1];
            Str[2] = source.Str[2];
            Str[3] = source.Str[3];

            Mag[0] = source.Mag[0];
            Mag[1] = source.Mag[1];
            Mag[2] = source.Mag[2];
            Mag[3] = source.Mag[3];

            Vit[0] = source.Vit[0];
            Vit[1] = source.Vit[1];
            Vit[2] = source.Vit[2];
            Vit[3] = source.Vit[3];

            Spr[0] = source.Spr[0];
            Spr[1] = source.Spr[1];
            Spr[2] = source.Spr[2];
            Spr[3] = source.Spr[3];

            Spd[0] = source.Spd[0];
            Spd[1] = source.Spd[1];
            Spd[2] = source.Spd[2];
            Spd[3] = source.Spd[3];

            Eva[0] = source.Eva[0];
            Eva[1] = source.Eva[1];
            Eva[2] = source.Eva[2];
            Eva[3] = source.Eva[3];
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
    }

    public class HeldItem
    {
        public byte ItemId;
        public byte Quantity;
    }
}
