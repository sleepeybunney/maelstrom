using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterMeshVertexGroup
    {
        public ushort BoneID { get; set; }
        public IList<MonsterMeshVertex> Vertices { get; set; }

        public MonsterMeshVertexGroup(ushort boneID)
        {
            BoneID = boneID;
            Vertices = new List<MonsterMeshVertex>();
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(BoneID));
            result.AddRange(BitConverter.GetBytes((ushort)Vertices.Count()));
            foreach (var vertex in Vertices) result.AddRange(vertex.Encode());
            return result;
        }
    }
}
