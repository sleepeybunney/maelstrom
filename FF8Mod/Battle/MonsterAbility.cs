using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterAbility
    {
        public byte Type { get; set; }
        public byte Something { get; set; }
        public ushort AbilityId { get; set; }

        public MonsterAbility() { }

        public MonsterAbility(IList<byte> data)
        {
            Type = data[0];
            Something = data[1];
            AbilityId = BitConverter.ToUInt16(data.ToArray(), 2);
        }

        public IEnumerable<byte> Encode()
        {
            var result = new byte[4];
            result[0] = Type;
            result[1] = Something;
            Array.Copy(BitConverter.GetBytes(AbilityId), 0, result, 2, 2);
            return result;
        }
    }
}
