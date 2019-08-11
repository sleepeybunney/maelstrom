using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8Mod
{
    public class EncounterSlot
    {
        public byte MonsterID;
        public Coords Position;
        public byte Level;
        public bool Enabled, Unloaded, Untargetable, Hidden;
        public short Unknown1, Unknown2, Unknown3;
        public byte Unknown4;

        public EncounterSlot()
        {
            MonsterID = 0x10;
            Level = 0xff;
            Position = new Coords(0, 0, -5700);
            Enabled = false;
            Unloaded = false;
            Untargetable = false;
            Hidden = false;
            Unknown1 = 0;
            Unknown2 = 0;
            Unknown3 = 0;
            Unknown4 = 0;
        }

        public Monster GetMonster(FileSource battleSource)
        {
            return Monster.FromSource(battleSource, string.Format(@"c:\ff8\data\eng\battle\c0m{0:d3}.dat", MonsterID - 0x10));
        }

        public class Coords
        {
            public short X, Y, Z;

            public Coords(short x, short y, short z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }
    }
}
