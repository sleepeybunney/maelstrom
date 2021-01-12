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
        public Ability[] Abilities;

        private byte[] PreGFData, PostGFData, PostAbilityData;

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

                // sections 0-1
                stream.Seek(0, SeekOrigin.Begin);
                PreGFData = reader.ReadBytes((int)(sectionOffsets[2]));

                // section 2 = junctionable gf
                JunctionableGFs = new JunctionableGF[16];
                for (int i = 0; i < 16; i++)
                {
                    JunctionableGFs[i] = new JunctionableGF(reader.ReadBytes(132));
                }

                // sections 3-10
                PostGFData = reader.ReadBytes((int)(sectionOffsets[11] - stream.Position));

                // sections 11-17 = abilities
                var abilities = new List<Ability>();
                while (sectionOffsets[18] - stream.Position >= 8)
                {
                    abilities.Add(new Ability(reader.ReadBytes(8)));
                }
                Abilities = abilities.ToArray();

                // sections 18-55
                PostAbilityData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public Kernel(byte[] data) : this(new MemoryStream(data)) { }

        public byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(PreGFData);
            foreach (var gf in JunctionableGFs) result.AddRange(gf.Encode());
            result.AddRange(PostGFData);
            foreach (var a in Abilities) result.AddRange(a.Encode());
            result.AddRange(PostAbilityData);
            return result.ToArray();
        }
    }
}
