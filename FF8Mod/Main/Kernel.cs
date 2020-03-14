using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FF8Mod.Main
{
    public class Kernel
    {
        public JunctionableGF[] JunctionableGFs;

        private byte[] PreGFData, PostGFData;

        public Kernel(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var sectionCount = reader.ReadUInt32();
                var sectionOffsets = new List<uint>();
                for (int i = 0; i < sectionCount; i++)
                {
                    sectionOffsets.Add(reader.ReadUInt32());
                }

                // todo: everything but GFs
                stream.Seek(0, SeekOrigin.Begin);
                PreGFData = reader.ReadBytes((int)(sectionOffsets[2]));
                JunctionableGFs = new JunctionableGF[16];
                for (int i = 0; i < 16; i++)
                {
                    JunctionableGFs[i] = new JunctionableGF(reader.ReadBytes(132));
                }
                PostGFData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public Kernel(byte[] data) : this(new MemoryStream(data)) { }

        public byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(PreGFData);
            foreach (var gf in JunctionableGFs) result.AddRange(gf.Encode());
            result.AddRange(PostGFData);
            return result.ToArray();
        }
    }
}
