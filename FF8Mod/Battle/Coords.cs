using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Battle
{
    public class Coords
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }

        public Coords(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
