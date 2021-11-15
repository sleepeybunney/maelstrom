using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Exe
{
    public class Spell
    {
        public int SpellID { get; set; }
        public string SpellName { get; set; }
        public bool SlotExclusive { get; set; } = false;
        public bool CutContent { get; set; } = false;
        public SpellTarget Target { get; set; } = SpellTarget.EnemySingle;
        public int DrawResist { get; set; } = 0;
        public int Animation { get; set; } = 0;
    }
}
