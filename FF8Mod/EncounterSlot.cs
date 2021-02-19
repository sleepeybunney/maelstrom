using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod
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
            MonsterID = 0;
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
            return Monster.ByID(battleSource, MonsterID);
        }
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
