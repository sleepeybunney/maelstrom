using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;

namespace Sleepey.FF8Mod.Main
{
    public class InitGF
    {
        public string Name { get; set; }
        public int Exp { get; set; }
        public byte Unknown { get; set; }
        public byte Exists { get; set; }
        public short HP { get; set; }
        public BitArray Abilities { get; set; }
        public List<byte> AP { get; set; }
        public short Kills { get; set; }
        public short Deaths { get; set; }
        public byte CurrentAbility { get; set; }
        public BitArray ForgottenAbilities { get; set; }

        public InitGF(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                Name = FF8String.Decode(reader.ReadBytes(12));
                Exp = reader.ReadInt32();
                Unknown = reader.ReadByte();
                Exists = reader.ReadByte();
                HP = reader.ReadInt16();
                Abilities = new BitArray(reader.ReadBytes(16));
                AP = reader.ReadBytes(24).ToList();
                Kills = reader.ReadInt16();
                Deaths = reader.ReadInt16();
                CurrentAbility = reader.ReadByte();
                ForgottenAbilities = new BitArray(reader.ReadBytes(3));
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();

            var encodedName = FF8String.Encode(Name).Take(12).ToList();
            result.AddRange(encodedName);
            result.AddRange(Enumerable.Repeat<byte>(0, 12 - encodedName.Count));

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

            return result;
        }
    }
}
