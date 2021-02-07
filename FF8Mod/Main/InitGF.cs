using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace FF8Mod.Main
{
    public class InitGF
    {
        public string Name;
        public int Exp;
        public byte Unknown;
        public byte Exists;
        public short HP;
        public BitArray Abilities;
        public byte[] AP;
        public short Kills;
        public short Deaths;
        public byte CurrentAbility;
        public BitArray ForgottenAbilities;

        public InitGF(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                Name = FF8String.Decode(reader.ReadBytes(12));
                Exp = reader.ReadInt32();
                Unknown = reader.ReadByte();
                Exists = reader.ReadByte();
                HP = reader.ReadInt16();
                Abilities = new BitArray(reader.ReadBytes(16));
                AP = reader.ReadBytes(24);
                Kills = reader.ReadInt16();
                Deaths = reader.ReadInt16();
                CurrentAbility = reader.ReadByte();
                ForgottenAbilities = new BitArray(reader.ReadBytes(3));
            }
        }

        public byte[] Encode()
        {
            var result = new List<byte>();

            var name = FF8String.Encode(Name).ToList();
            while (name.Count < 12) name.Add(0);
            result.AddRange(name);

            result.AddRange(BitConverter.GetBytes(Exp));
            result.Add(Unknown);
            result.Add(Exists);
            result.AddRange(BitConverter.GetBytes(HP));

            var abilities = new byte[16];
            Abilities.CopyTo(abilities, 0);
            result.AddRange(abilities);

            result.AddRange(AP);
            result.AddRange(BitConverter.GetBytes(Kills));
            result.AddRange(BitConverter.GetBytes(Deaths));
            result.Add(CurrentAbility);

            var forgotten = new byte[3];
            ForgottenAbilities.CopyTo(forgotten, 0);
            result.AddRange(forgotten);
            
            return result.ToArray();
        }
    }
}
