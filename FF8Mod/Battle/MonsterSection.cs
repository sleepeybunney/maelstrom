using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterSection
    {
        public MonsterSectionIndex Type { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
    }
}
