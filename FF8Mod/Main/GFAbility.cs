using System;
using System.Collections.Generic;
using System.Text;

namespace FF8Mod.Main
{
    public class GFAbility
    {
        public byte Unlocker;
        public byte Ability;
        public byte Unknown1, Unknown2;

        public GFAbility(byte unlocker, byte unknown1, byte ability, byte unknown2)
        {
            Unlocker = unlocker;
            Ability = ability;
            Unknown1 = unknown1;
            Unknown2 = unknown2;
        }

        public byte[] Encode()
        {
            return new byte[] { Unlocker, Unknown1, Ability, Unknown2 };
        }
    }
}
