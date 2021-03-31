using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Battle
{
    public class Argument
    {
        public string Name { get; set; }
        public ArgType Type { get; set; } = ArgType.Byte;

        public Argument(string name) { Name = name; }

        public Argument(string name, ArgType type) : this(name) { Type = type; }
    }
}
