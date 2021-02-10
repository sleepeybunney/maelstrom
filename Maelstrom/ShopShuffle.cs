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

        public static List<Shop> Randomise(int seed, bool keyItems, bool summonItems, bool magazines, bool chocoboWorld)
        {
            var result = new List<Shop>(Shops);
            var random = new Random(seed);

            var pool = Item.Lookup.Values
                .Where(i => !i.KeyItem || keyItems)
                .Where(i => !i.SummonItem || summonItems)
                .Where(i => !i.Magazine || magazines)
                .Where(i => !i.ChocoboWorld || chocoboWorld)
                .Select(i => i.ID).ToList();

            foreach (var s in result)
            {
                s.Items = GenerateShop(random, pool);
            }
            return result;
        }

        private static List<ShopItem> GenerateShop(Random random, List<int> itemPool)
        {
            var result = new List<ShopItem>();
            var unusedItems = new List<int>(itemPool);

            for (int i = 0; i < 16; i++)
            {
                var itemIndex = random.Next(unusedItems.Count);
                result.Add(new ShopItem((byte)unusedItems[itemIndex], i < 5));
                unusedItems.RemoveAt(itemIndex);
            }

            return result.OrderBy(item => item.ItemCode).ToList();
        }

        public static void Apply(FileSource menuSource, List<Shop> shops)
        {
            var shopBinPath = Globals.DataPath + @"\menu\shop.bin";
            var newData = new List<byte>();
            foreach (var s in shops)
            {
                newData.AddRange(s.Encode());
            }
            menuSource.ReplaceFile(shopBinPath, newData.ToArray());
        }
    }
}
