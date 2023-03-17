using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class MonsterTextureCollection
    {
        public List<Tim> Textures { get; set; } = new List<Tim>();

        public MonsterTextureCollection(IEnumerable<byte> data)
        {
            Textures = new List<Tim>();

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                var timCount = reader.ReadUInt32();

                var timPositions = new List<uint>();
                for (int i = 0; i < timCount; i++)
                {
                    timPositions.Add(reader.ReadUInt32());
                }
                var eofPosition = reader.ReadUInt32();

                var timLengths = new List<uint>();
                for (int i = 1; i < timPositions.Count; i++)
                {
                    timLengths.Add(timPositions[i] - timPositions[i - 1]);
                }
                timLengths.Add(eofPosition - timPositions[timPositions.Count - 1]);

                for (int i = 0; i < timLengths.Count; i++)
                {
                    Textures.Add(new Tim(reader.ReadBytes((int)timLengths[i])));
                }
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();

            // texture count
            result.AddRange(BitConverter.GetBytes((uint)Textures.Count));

            var encodedTims = new List<byte>();
            var headerLength = (uint)Textures.Count * 4 + 8;
            foreach (var texture in Textures)
            {
                // texture position
                result.AddRange(BitConverter.GetBytes(headerLength + (uint)encodedTims.Count));
                encodedTims.AddRange(texture.Encode());
            }

            // eof position
            result.AddRange(BitConverter.GetBytes(headerLength + (uint)encodedTims.Count));

            // texture data
            result.AddRange(encodedTims);

            return result;
        }
    }
}
