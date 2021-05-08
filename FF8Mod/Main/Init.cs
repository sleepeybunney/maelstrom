using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Main
{
    public class Init
    {
        public IList<InitGF> GFs { get; set; } = new List<InitGF>();
        public IList<InitCharacter> Characters { get; set; } = new List<InitCharacter>();
        public IEnumerable<byte> OtherData { get; set; }

        public Init(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < 16; i++)
                {
                    GFs.Add(new InitGF(reader.ReadBytes(68)));
                }

                for (int i = 0; i < 8; i++)
                {
                    Characters.Add(new InitCharacter(reader.ReadBytes(152)));
                }

                OtherData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            foreach (var gf in GFs) result.AddRange(gf.Encode());
            foreach (var character in Characters) result.AddRange(character.Encode());
            result.AddRange(OtherData);
            return result;
        }
    }
}
