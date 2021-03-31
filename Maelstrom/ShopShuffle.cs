using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Menu;

namespace Sleepey.Maelstrom
{
    class ShopShuffle
    {
        public static List<Shop> Shops = JsonSerializer.Deserialize<List<Shop>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Shops.json"));

        public static List<Shop> Randomise(int seed, State settings)
        {
            var result = new List<Shop>(Shops);
            var random = new Random(seed + 8);

            var pool = Item.Lookup.Values
                .Where(i => !i.KeyItem || settings.ShopKeyItems)
                .Where(i => !i.SummonItem || settings.ShopSummonItems)
                .Where(i => !i.Magazine || settings.ShopMagazines)
                .Where(i => !i.ChocoboWorld || settings.ShopChocoboWorld)
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
