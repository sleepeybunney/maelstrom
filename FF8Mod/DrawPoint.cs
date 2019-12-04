using System;
using System.Collections.Generic;

namespace FF8Mod
{
    public class DrawPoint
    {
        public const int DrawPointDefsLocation = 0x792328;
        public const int DrawPointDefsLength = 0x100;

        public static Dictionary<int, DrawPoint> UpdatedDrawPoints = new Dictionary<int, DrawPoint>();

        public int Offset { get; set; }
        public string Location { get; set; }
        public Magic Spell { get; set; }
        public bool Renewable { get; set; }
        public bool Bountiful { get; set; }

        public DrawPoint()
        {
            Spell = Magic.Fire;
            Renewable = false;
            Bountiful = false;
        }

        public DrawPoint(Magic spell, bool renewable, bool bountiful)
        {
            Spell = spell;
            Renewable = renewable;
            Bountiful = bountiful;
        }

        public DrawPoint(byte code)
        {
            var spell = code & 0x3f;
            if (!Enum.IsDefined(typeof(Magic), spell)) throw new Exception("Draw point stocks an unknown spell");
            Spell = (Magic)spell;
            Renewable = (code & 0x40) == 1;
            Bountiful = (code & 0x80) == 1;
        }

        public byte Encode()
        {
            var result = (byte)Spell;
            if (Renewable) result += 0x40;
            if (Bountiful) result += 0x80;
            return result;
        }

        public static DrawPoint ById(int id)
        {
            if (UpdatedDrawPoints.ContainsKey(id))
            {
                return UpdatedDrawPoints[id];
            }
            else
            {
                return new DrawPoint(OriginalData[id - 1]);
            }
        }

        public static byte[] EncodeAll()
        {
            var result = (byte[])OriginalData.Clone();
            foreach (var id in UpdatedDrawPoints.Keys)
            {
                result[id - 1] = UpdatedDrawPoints[id].Encode();
            }
            return result;
        }

        public static byte[] OriginalData = new byte[DrawPointDefsLength]
        {
            0x55, 0x44, 0x99, 0x9B, 0x0D, 0xCC, 0xC7, 0xD5, 0xC1, 0xE9, 0xE6, 0x72,
            0x55, 0x06, 0xE3, 0xD8, 0xDE, 0xDD, 0x21, 0xA0, 0x55, 0x4A, 0x48, 0x70,
            0x5B, 0x4C, 0xC2, 0xEE, 0x09, 0x4B, 0xC5, 0xE6, 0x59, 0xEC, 0xDC, 0x17,
            0x1F, 0xDD, 0xEF, 0x56, 0xE3, 0xDE, 0xDA, 0x19, 0x93, 0xC9, 0x10, 0x57,
            0x44, 0xD1, 0x52, 0xE1, 0x2D, 0x0F, 0x65, 0x65, 0xD8, 0x1F, 0xEB, 0x0E,
            0x69, 0x13, 0x67, 0x6A, 0xD0, 0x57, 0x64, 0x57, 0x0F, 0x8E, 0x68, 0xE7,
            0x4B, 0x2C, 0x2D, 0xC9, 0x30, 0x20, 0xD3, 0x46, 0x43, 0xD2, 0xCE, 0x56,
            0x58, 0x19, 0x5C, 0x5B, 0xA2, 0x13, 0xF1, 0x10, 0xE3, 0xE4, 0xD7, 0xD8,
            0xE5, 0xDA, 0xE1, 0x22, 0x0F, 0x57, 0x56, 0x72, 0x5B, 0x64, 0x5C, 0x65,
            0x58, 0x0F, 0x20, 0x0E, 0x10, 0x31, 0x13, 0x19, 0x22, 0x81, 0x81, 0x81,
            0x81, 0x81, 0x81, 0x81, 0x81, 0x81, 0x81, 0x81, 0x55, 0x5B, 0x47, 0x42,
            0x48, 0x45, 0x44, 0x41, 0x55, 0x4A, 0x56, 0x5B, 0x72, 0x5E, 0x63, 0x4B,
            0x4C, 0x58, 0x4D, 0x5D, 0x4E, 0x49, 0x65, 0x43, 0x5A, 0x46, 0x67, 0x4F,
            0x5C, 0x64, 0x51, 0x57, 0x52, 0x19, 0x5F, 0x20, 0x11, 0x61, 0x6A, 0x10,
            0x13, 0x22, 0x67, 0x66, 0xD1, 0x68, 0x69, 0xCF, 0x6B, 0x6C, 0xED, 0x6E,
            0x6F, 0x70, 0x71, 0x93, 0xD2, 0xD1, 0xD0, 0xCE, 0xCF, 0xE0, 0xD3, 0xE2,
            0xD9, 0xD2, 0xD1, 0xD0, 0xCE, 0xCF, 0xE0, 0xD3, 0xE2, 0xD9, 0xD2, 0xD1,
            0xD0, 0xCE, 0xCF, 0xE0, 0xD3, 0xE2, 0xD9, 0xD3, 0xD0, 0xCE, 0xCF, 0xE0,
            0xD3, 0xE2, 0xD9, 0xD0, 0xCE, 0xE2, 0xE0, 0xD3, 0xE2, 0xD9, 0xD0, 0xCE,
            0xCF, 0xE0, 0xD3, 0xE2, 0xD9, 0xD0, 0xE2, 0xCF, 0xE0, 0xD3, 0xE2, 0xD9,
            0xD0, 0xCE, 0xCF, 0xE0, 0xD3, 0x44, 0x55, 0xDC, 0xE7, 0x10, 0x21, 0x20,
            0x0E, 0x0F, 0x13, 0xF2
        };

        public enum Magic
        {
            Fire = 1,
            Fira = 2,
            Firaga = 3,
            Blizzard = 4,
            Blizzara = 5,
            Blizzaga = 6,
            Thunder = 7,
            Thundara = 8,
            Thundaga = 9,
            Water = 10,
            Aero = 11,
            Bio = 12,
            Demi = 13,
            Holy = 14,
            Flare = 15,
            Meteor = 16,
            Quake = 17,
            Tornado = 18,
            Ultima = 19,
            Apocalypse = 20,
            Cure = 21,
            Cura = 22,
            Curaga = 23,
            Life = 24,
            FullLife = 25,
            Regen = 26,
            Esuna = 27,
            Dispel = 28,
            Protect = 29,
            Shell = 30,
            Reflect = 31,
            Aura = 32,
            Double = 33,
            Triple = 34,
            Haste = 35,
            Slow = 36,
            Stop = 37,
            Blind = 38,
            Confuse = 39,
            Sleep = 40,
            Silence = 41,
            Break = 42,
            Death = 43,
            Drain = 44,
            Pain = 45,
            Berserk = 46,
            Float = 47,
            Zombie = 48,
            Meltdown = 49,
            Scan = 50,
            FullCure = 51,
            Wall = 52,
            Rapture = 53,
            Percent = 54,
            Catastrophe = 55,
            TheEnd = 56
        }
    }
}
