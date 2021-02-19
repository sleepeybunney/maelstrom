using System;
using System.Collections.Generic;
using System.Text;

namespace Sleepey.FF8Mod
{
    public class Shop
    {
        public string Name { get; set; }
        public List<ShopItem> Items { get; set; }

        public Shop()
        {
            Items = new List<ShopItem>();
        }

        public byte[] Encode()
        {
            var result = new List<byte>();
            foreach (var i in Items) result.AddRange(i.Encode());
            return result.ToArray();
        }
    }
}
