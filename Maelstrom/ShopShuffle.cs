using FF8Mod.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FF8Mod.Maelstrom
{
    class ShopShuffle
    {
        public static List<Shop> Shops = JsonSerializer.Deserialize<List<Shop>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.Shops.json"));
        public static Dictionary<int, Item> Items = JsonSerializer.Deserialize<List<Item>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.Items.json")).ToDictionary(i => i.ID);

        public static List<Shop> Randomise(int seed)
        {
            var result = new List<Shop>(Shops);
            var random = new Random(seed);
            foreach (var s in result)
            {
                s.Items = GenerateShop(random);
            }
            return result;
        }

        private static List<ShopItem> GenerateShop(Random random)
        {
            var result = new List<ShopItem>();
            var items = Enumerable.Range(1, 198).ToList();

            for (int i = 0; i < 16; i++)
            {
                var itemIndex = random.Next(1, items.Count);
                result.Add(new ShopItem((byte)items[itemIndex], i < 5));
                items.RemoveAt(itemIndex);
            }

            return result.OrderBy(item => item.ItemCode).ToList();
        }

        public static void Apply(FileSource menuSource, List<Shop> shops)
        {
            var shopBinPath = @"c:\ff8\data\eng\menu\shop.bin";
            var newData = new List<byte>();
            foreach (var s in shops)
            {
                newData.AddRange(s.Encode());
            }
            menuSource.ReplaceFile(shopBinPath, newData.ToArray());
        }
    }
}
