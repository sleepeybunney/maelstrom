using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Battle
{
    public class OpCode
    {
        public string Name { get; set; }
        public byte Code { get; set; }
        public IList<Argument> Args { get; set; }

        public OpCode(string name, byte code, params Argument[] args)
        {
            Name = name;
            Code = code;
            Args = args;
        }

        public int Length { get => Args.Sum(a => a.Type == ArgType.Short ? 2 : 1) + 1; }
    }
}
