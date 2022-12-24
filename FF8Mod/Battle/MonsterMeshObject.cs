using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterMeshObject
    {
        public IList<MonsterMeshVertexGroup> Groups { get; set; }

        private byte[] NonVertexData;

        public MonsterMeshObject(IEnumerable<byte> data)
        {
            Groups = new List<MonsterMeshVertexGroup>();

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                var groupCount = reader.ReadUInt16();
                for (int i = 0; i < groupCount; i++)
                {
                    var group = new MonsterMeshVertexGroup(reader.ReadUInt16());

                    var vertCount = reader.ReadUInt16();
                    for (int j = 0; j < vertCount; j++)
                    {
                        group.Vertices.Add(new MonsterMeshVertex(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()));
                    }

                    Groups.Add(group);
                }

                NonVertexData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((ushort)Groups.Count()));
            foreach (var group in Groups) result.AddRange(group.Encode());
            result.AddRange(NonVertexData);
            return result;
        }
    }
}
