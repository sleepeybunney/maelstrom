using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FF8Mod.Main
{
    public class Init
    {
        public List<InitGF> GFs;
        public byte[] OtherData;

        public Init(byte[] data)
        {
            GFs = new List<InitGF>();

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < 16; i++)
                {
                    GFs.Add(new InitGF(reader.ReadBytes(68)));
                }

                OtherData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public byte[] Encode()
        {
            var result = new List<byte>();
            foreach (var gf in GFs) result.AddRange(gf.Encode());
            result.AddRange(OtherData);
            return result.ToArray();
        }
    }
}
