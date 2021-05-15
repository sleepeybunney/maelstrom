using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterAnimationData
    {
        public uint AnimationCount { get; set; }
        public List<uint> AnimationLocations { get; set; } = new List<uint>();
        public List<byte> AnimationData { get; set; }

        public MonsterAnimationData(IEnumerable<byte> data)
        {
            var dataArray = data.ToArray();
            AnimationCount = BitConverter.ToUInt32(dataArray, 0);
            for (int i = 0; i < AnimationCount; i++)
            {
                AnimationLocations.Add(BitConverter.ToUInt32(dataArray, i * 4 + 4));
            }
            AnimationData = data.Skip(4).ToList();
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(AnimationCount));
            result.AddRange(AnimationData);
            return result;
        }
    }
}
