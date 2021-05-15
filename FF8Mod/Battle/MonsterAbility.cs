using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterAbility
    {
        public AbilityType Type { get; set; }
        public byte AnimationID { get; set; }
        public ushort AbilityID { get; set; }

        public MonsterAbility(AbilityType type, byte animationID, ushort abilityID)
        {
            Type = type;
            AnimationID = animationID;
            AbilityID = abilityID;
        }

        public MonsterAbility(IList<byte> data)
        {
            Type = (AbilityType)data[0];
            AnimationID = data[1];
            AbilityID = BitConverter.ToUInt16(data.ToArray(), 2);
        }

        public IEnumerable<byte> Encode()
        {
            var result = new byte[4];
            result[0] = (byte)Type;
            result[1] = AnimationID;
            Array.Copy(BitConverter.GetBytes(AbilityID), 0, result, 2, 2);
            return result;
        }
    }
}
