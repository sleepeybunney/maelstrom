using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod.Battle
{
    public class EncounterSlot
    {
        public byte MonsterID { get; set; }
        public Coords Position { get; set; } = new Coords(0, 0, -5700);
        public byte Level { get; set; } = 0xff;
        public bool Enabled { get; set; }
        public bool Unloaded { get; set; }
        public bool Untargetable { get; set; }
        public bool Hidden { get; set; }
        public short Unknown1 { get; set; }
        public short Unknown2 { get; set; }
        public short Unknown3 { get; set; }
        public byte Unknown4 { get; set; }

        public EncounterSlot() { }

        public Monster GetMonster(FileSource battleSource)
        {
            return Monster.ByID(battleSource, MonsterID);
        }
    }

    
}
