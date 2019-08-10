using System.IO;

namespace FF8Mod
{
    public class MonsterInfo
    {
        public string Name;
        public byte[] Hp, Str, Mag, Vit, Spr, Spd, Eva;
        public Ability[] AbilitiesLow, AbilitiesMed, AbilitiesHigh;
        byte MedStart, HighStart;
        byte MysteryFlags1, MysteryFlags2;
        byte StatusFlags1, StatusFlags2;
        byte CardDrop, CardMorph, CardRareMorph;
        byte DevourLow, DevourMed, DevourHigh;
        uint Exp, ExtraExp, Ap;
        byte[] DrawLow, DrawMed, DrawHigh;
        HeldItem[] MugLow, MugMed, MugHigh;
        HeldItem[] DropLow, DropMed, DropHigh;
        byte MugRate, DropRate;
        byte[] MysteryData;
        byte[] ElemDef, StatusDef;

        public MonsterInfo()
        {
            this.Name = "Unnamed";
        }

        public MonsterInfo(string name) : this()
        {
            this.Name = name;
        }

        public MonsterInfo(byte[] data)
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

                AbilitiesLow = new Ability[16];
                for (int i = 0; i < 16; i++) AbilitiesLow[i] = new Ability
                {
                    Type = reader.ReadByte(),
                    Something = reader.ReadByte(),
                    AbilityId = reader.ReadUInt16()
                };

                AbilitiesMed = new Ability[16];
                for (int i = 0; i < 16; i++) AbilitiesMed[i] = new Ability
                {
                    Type = reader.ReadByte(),
                    Something = reader.ReadByte(),
                    AbilityId = reader.ReadUInt16()
                };

                AbilitiesHigh = new Ability[16];
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

                DrawLow = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    DrawLow[i] = reader.ReadByte();
                    reader.ReadByte(); // padding
                }

                DrawMed = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    DrawMed[i] = reader.ReadByte();
                    reader.ReadByte(); // padding
                }

                DrawHigh = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    DrawHigh[i] = reader.ReadByte();
                    reader.ReadByte(); // padding
                }

                MugLow = new HeldItem[4];
                for (int i = 0; i < 4; i++) MugLow[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                MugMed = new HeldItem[4];
                for (int i = 0; i < 4; i++) MugMed[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                MugHigh = new HeldItem[4];
                for (int i = 0; i < 4; i++) MugHigh[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                DropLow = new HeldItem[4];
                for (int i = 0; i < 4; i++) DropLow[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                DropMed = new HeldItem[4];
                for (int i = 0; i < 4; i++) DropMed[i] = new HeldItem
                {
                    ItemId = reader.ReadByte(),
                    Quantity = reader.ReadByte()
                };

                DropHigh = new HeldItem[4];
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

        public struct Ability
        {
            public byte Type;
            public byte Something;
            public uint AbilityId;
        }

        public struct HeldItem
        {
            public byte ItemId;
            public byte Quantity;
        }
    }
}
