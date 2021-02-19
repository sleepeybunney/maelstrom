using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace Sleepey.FF8Mod.Main
{
    public class Kernel
    {
        public JunctionableGF[] JunctionableGFs;
        public Weapon[] Weapons;
        public Ability[] Abilities;
        public byte[] WeaponText;

        private byte[] PreGFData, PostGFData, PostWeaponData, PostAbilityData, PostWeaponTextData;

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

                // section 3
                PostGFData = reader.ReadBytes((int)(sectionOffsets[4] - stream.Position));

                // section 4 = weapons
                Weapons = new Weapon[33];
                for (int i = 0; i < 33; i++)
                {
                    Weapons[i] = new Weapon(reader.ReadBytes(12));
                }

                //sections 5-10
                PostWeaponData = reader.ReadBytes((int)(sectionOffsets[11] - stream.Position));

                // sections 11-17 = abilities
                var abilities = new List<Ability>();
                while (sectionOffsets[18] - stream.Position >= 8)
                {
                    abilities.Add(new Ability(reader.ReadBytes(8)));
                }
                Abilities = abilities.ToArray();

                // sections 18-34
                PostAbilityData = reader.ReadBytes((int)(sectionOffsets[35] - stream.Position));

                // section 35 = weapon text
                WeaponText = reader.ReadBytes((int)(sectionOffsets[36] - stream.Position));
                foreach (var w in Weapons) w.Name = FF8String.Decode(WeaponText.Skip(w.NameOffset).ToArray());

                // sections 36-55
                PostWeaponTextData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public Kernel(byte[] data) : this(new MemoryStream(data)) { }

        public byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(PreGFData);
            foreach (var gf in JunctionableGFs) result.AddRange(gf.Encode());
            result.AddRange(PostGFData);
            foreach (var w in Weapons) result.AddRange(w.Encode());
            result.AddRange(PostWeaponData);
            foreach (var a in Abilities) result.AddRange(a.Encode());
            result.AddRange(PostAbilityData);
            result.AddRange(WeaponText);
            result.AddRange(PostWeaponTextData);
            return result.ToArray();
        }
    }
}
