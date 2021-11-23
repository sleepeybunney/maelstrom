using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterInfo
    {
        public string Name { get; set; } = "Unnamed";
        public IList<byte> Hp { get; set; } = new byte[4];
        public IList<byte> Str { get; set; } = new byte[4];
        public IList<byte> Mag { get; set; } = new byte[4];
        public IList<byte> Vit { get; set; } = new byte[4];
        public IList<byte> Spr { get; set; } = new byte[4];
        public IList<byte> Spd { get; set; } = new byte[4];
        public IList<byte> Eva { get; set; } = new byte[4];
        public IList<MonsterAbility> AbilitiesLow { get; set; } = new MonsterAbility[16];
        public IList<MonsterAbility> AbilitiesMed { get; set; } = new MonsterAbility[16];
        public IList<MonsterAbility> AbilitiesHigh { get; set; } = new MonsterAbility[16];
        public byte MedStart { get; set; }
        public byte HighStart { get; set; }
        public byte MysteryFlags1 { get; set; }
        public byte MysteryFlags2 { get; set; }
        public byte StatusFlags1 { get; set; }
        public byte StatusFlags2 { get; set; }
        public byte CardDrop { get; set; }
        public byte CardMorph { get; set; }
        public byte CardRareMorph { get; set; }
        public byte DevourLow { get; set; }
        public byte DevourMed { get; set; }
        public byte DevourHigh { get; set; }
        public ushort Exp { get; set; }
        public ushort ExtraExp { get; set; }
        public byte Ap { get; set; }
        public IList<byte> DrawLow { get; set; } = new byte[4];
        public IList<byte> DrawMed { get; set; } = new byte[4];
        public IList<byte> DrawHigh { get; set; } = new byte[4];
        public IList<HeldItem> MugLow { get; set; } = new HeldItem[4];
        public IList<HeldItem> MugMed { get; set; } = new HeldItem[4];
        public IList<HeldItem> MugHigh { get; set; } = new HeldItem[4];
        public IList<HeldItem> DropLow { get; set; } = new HeldItem[4];
        public IList<HeldItem> DropMed { get; set; } = new HeldItem[4];
        public IList<HeldItem> DropHigh { get; set; } = new HeldItem[4];
        public byte MugRate { get; set; }
        public byte DropRate { get; set; }
        public IList<byte> MysteryData { get; set; } = new byte[16];
        public IList<byte> ElemDef { get; set; } = new byte[8];
        public IList<byte> StatusDef { get; set; } = new byte[20];

        public MonsterInfo() { }

        public MonsterInfo(string name) { Name = name; }

        public MonsterInfo(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
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

                for (int i = 0; i < 16; i++) AbilitiesLow[i] = new MonsterAbility(reader.ReadBytes(4));
                for (int i = 0; i < 16; i++) AbilitiesMed[i] = new MonsterAbility(reader.ReadBytes(4));
                for (int i = 0; i < 16; i++) AbilitiesHigh[i] = new MonsterAbility(reader.ReadBytes(4));

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
                var encodedName = FF8String.Encode(Name).Take(24).ToList();
                writer.Write(encodedName);
                writer.Write(Enumerable.Repeat<byte>(0, 24 - encodedName.Count));

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

        // hp2 & hp4 determine the lower boundary
        // mainly hp4 (thousands) with hp2 (tens) for fine tuning
        // hp1 & hp3 determine growth rate
        // hp1 sets the curve & hp3 adds a linear increase
        // there's no easy way to explain this in the ui is there...
        public int HpAtLevel(int level)
        {
            return (Hp[0] * level * level / 20) + (Hp[0] + Hp[2] * 100) * level + Hp[1] * 10 + Hp[3] * 1000;
        }

        private static int OffensiveStatAtLevel(int level, IList<byte> values)
        {
            return (level * values[0] / 10 + level / values[1] - level * level / 2 / values[3] + values[2]) / 4;
        }

        private static int NonOffensiveStatAtLevel(int level, IList<byte> values)
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

        public void CopyStatsFrom(MonsterInfo source)
        {
            Hp = source.Hp.ToList();
            Str = source.Str.ToList();
            Mag = source.Mag.ToList();
            Vit = source.Vit.ToList();
            Spr = source.Spr.ToList();
            Spd = source.Spd.ToList();
            Eva = source.Eva.ToList();
        }
    }
}
