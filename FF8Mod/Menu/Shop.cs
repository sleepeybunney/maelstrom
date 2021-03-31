using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Menu
{
    public class Shop
    {
        public string Name { get; set; }
        public List<ShopItem> Items { get; set; } = new List<ShopItem>();

        public Shop() { }

        public IEnumerable<byte> Encode()
        {
            return Items.SelectMany(i => i.Encode());
        }
    }
}
