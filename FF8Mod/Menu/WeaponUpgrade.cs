using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod.Menu
{
    public class WeaponUpgrade
    {
        public byte MsgOffset { get; set; }
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public byte Price { get; set; }
        public byte Item1 { get; set; }
        public byte Item1Quantity { get; set; }
        public byte Item2 { get; set; }
        public byte Item2Quantity { get; set; }
        public byte Item3 { get; set; }
        public byte Item3Quantity { get; set; }
        public byte Item4 { get; set; }
        public byte Item4Quantity { get; set; }

        public WeaponUpgrade() { }

        public WeaponUpgrade(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                MsgOffset = reader.ReadByte();
                Unknown1 = reader.ReadByte();
                Unknown2 = reader.ReadByte();
                Price = reader.ReadByte();
                Item1 = reader.ReadByte();
                Item1Quantity = reader.ReadByte();
                Item2 = reader.ReadByte();
                Item2Quantity = reader.ReadByte();
                Item3 = reader.ReadByte();
                Item3Quantity = reader.ReadByte();
                Item4 = reader.ReadByte();
                Item4Quantity = reader.ReadByte();
            }
        }

        public IEnumerable<byte> Encode()
        {
            return new byte[]
            {
                MsgOffset,
                Unknown1,
                Unknown2,
                Price,
                Item1,
                Item1Quantity,
                Item2,
                Item2Quantity,
                Item3,
                Item3Quantity,
                Item4,
                Item4Quantity
            };
        }

        public static List<WeaponUpgrade> ReadAllFromSource(FileSource menuSource)
        {
            var result = new List<WeaponUpgrade>();

            using (var stream = new MemoryStream(menuSource.GetFile(Globals.WeaponUpgradePath).ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < 33; i++)
                {
                    result.Add(new WeaponUpgrade(reader.ReadBytes(12)));
                }
            }

            return result;
        }
    }
}
