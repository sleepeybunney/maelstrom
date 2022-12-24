using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterMeshVertex
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }

        public MonsterMeshVertex(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(X));
            result.AddRange(BitConverter.GetBytes(Y));
            result.AddRange(BitConverter.GetBytes(Z));
            return result;
        }
    }
}
