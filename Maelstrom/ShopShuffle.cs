using FF8Mod.Archive;
using System;
using System.Collections.Generic;
using System.IO;

namespace FF8Mod.Maelstrom
{
    class ShopShuffle
    {
        public static void Apply(FileSource menuSource, int seed)
        {
            var shopBinPath = @"c:\ff8\data\eng\menu\shop.bin";
            var data = menuSource.GetFile(shopBinPath);
            var newData = new List<byte>();
            var items = new List<ShopItem>();
            var random = new Random(seed);

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Length - stream.Position > 1)
                {
                    items.Add(new ShopItem(reader.ReadByte(), reader.ReadByte() == 0x00));
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                items[i].ItemCode = (byte)random.Next(1, 199);
                newData.AddRange(items[i].Encode());
            }

            menuSource.ReplaceFile(shopBinPath, newData.ToArray());
        }
    }
}
