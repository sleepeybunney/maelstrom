using Sleepey.FF8Mod.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Menu
{
    public class Price
    {
        public ushort BasePrice { get; set; }
        public ushort SellMultiplier { get; set; }

        public uint BuyPrice { get => (uint)BasePrice * 10; }

        public uint SellPrice { get => (uint)BasePrice * SellMultiplier / 2; }

        public Price(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                BasePrice = reader.ReadUInt16();
                SellMultiplier = reader.ReadUInt16();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(BasePrice));
            result.AddRange(BitConverter.GetBytes(SellMultiplier));
            return result;
        }

        public static List<Price> ReadAllFromSource(FileSource menuSource)
        {
            var result = new List<Price>();

            using (var stream = new MemoryStream(menuSource.GetFile(Env.PricePath).ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Length - stream.Position >= 4)
                {
                    result.Add(new Price(reader.ReadBytes(4)));
                }
            }

            return result;
        }
    }
}
