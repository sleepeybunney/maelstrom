using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Main
{
    public class GFAbility
    {
        public byte Unlocker { get; set; }
        public byte Ability { get; set; }
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }

        public GFAbility(byte unlocker, byte unknown1, byte ability, byte unknown2)
        {
            Unlocker = unlocker;
            Ability = ability;
            Unknown1 = unknown1;
            Unknown2 = unknown2;
        }

        public IEnumerable<byte> Encode()
        {
            return new byte[] { Unlocker, Unknown1, Ability, Unknown2 };
        }
    }
}
