using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterMesh
    {
        public IList<MonsterMeshObject> Objects { get; set; }

        public MonsterMesh(IEnumerable<byte> data)
        {
            Objects = new List<MonsterMeshObject>();

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                var objectCount = reader.ReadUInt32();
                var objectPositions = new List<uint>();

                for (int i = 0; i < objectCount; i++)
                {
                    objectPositions.Add(reader.ReadUInt32());
                }

                for (int i = 0; i < objectCount; i++)
                {
                    var endPosition = (i == objectCount - 1) ? stream.Length - 4 : objectPositions[i + 1];
                    var objectData = reader.ReadBytes((int)(endPosition - objectPositions[i]));
                    Objects.Add(new MonsterMeshObject(objectData));
                }
            }
        }

        public IEnumerable<byte> Encode()
        {
            var objectArray = Objects.ToArray();
            var objectCount = objectArray.Length;
            var objectPositions = new List<uint>();
            var objectData = new List<byte>();
            var headerLength = (objectCount * 4) + 4;

            for (int i = 0; i < objectCount; i++)
            {
                objectPositions.Add((uint)(objectData.Count + headerLength));
                objectData.AddRange(objectArray[i].Encode());
            }

            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((uint)objectCount));
            foreach (var pos in objectPositions) result.AddRange(BitConverter.GetBytes(pos));
            result.AddRange(objectData);

            var vertCount = (uint)objectArray.Sum(o => o.Groups.Sum(g => g.Vertices.Count));
            result.AddRange(BitConverter.GetBytes(vertCount));

            return result;
        }
    }
}
