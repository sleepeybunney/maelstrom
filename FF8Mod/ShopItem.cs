using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8Mod
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
            return new byte[] { ItemCode, Hidden ? (byte)0x00 : (byte)0xff };
        }
    }
}
