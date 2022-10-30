using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Main
{
    public class EnemyAttack
    {
        public ushort NameOffset { get; set; }
        public ushort SpellID { get; set; }
        public byte CameraChange { get; set; }
        public byte Unknown1 { get; set; }
        public byte AttackType { get; set; }
        public byte Power { get; set; }
        public BitArray AttackFlags { get; set; }
        public byte Unknown2 { get; set; }
        public byte Element { get; set; }
        public byte Unknown3 { get; set; }
        public byte StatusPower { get; set; }
        public byte AttackParameter { get; set; }
        public BitArray StatusFlags { get; set; }
        public string Name { get; set; }

        public EnemyAttack(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                NameOffset = reader.ReadUInt16();
                SpellID = reader.ReadUInt16();
                CameraChange = reader.ReadByte();
                Unknown1 = reader.ReadByte();
                AttackType = reader.ReadByte();
                Power = reader.ReadByte();
                AttackFlags = new BitArray(reader.ReadBytes(1));
                Unknown2 = reader.ReadByte();
                Element = reader.ReadByte();
                Unknown3 = reader.ReadByte();
                StatusPower = reader.ReadByte();
                AttackParameter = reader.ReadByte();
                StatusFlags = new BitArray(reader.ReadBytes(6));
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(NameOffset));
            result.AddRange(BitConverter.GetBytes(SpellID));

            result.Add(CameraChange);
            result.Add(Unknown1);
            result.Add(AttackType);
            result.Add(Power);

            var attackFlags = new byte[1];
            AttackFlags.CopyTo(attackFlags, 0);
            result.AddRange(attackFlags);

            result.Add(Unknown2);
            result.Add(Element);
            result.Add(Unknown3);
            result.Add(StatusPower);
            result.Add(AttackParameter);

            var statuses = new byte[6];
            StatusFlags.CopyTo(statuses, 0);
            result.AddRange(statuses);

            return result;
        }
    }
}
