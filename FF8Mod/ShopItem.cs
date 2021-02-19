using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod
{
    public class ShopItem
    {
        public byte ItemCode { get; set; }
        public bool Hidden { get; set; }

        public ShopItem() { }

        public ShopItem(byte item, bool hidden = false)
        {
            ItemCode = item;
            Hidden = hidden;
        }

        public byte[] Encode()
        {
            return new byte[] { ItemCode, (byte)(Hidden ? 0x00 : 0xff) };
        }
    }
}
