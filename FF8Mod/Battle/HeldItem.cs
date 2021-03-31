using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Battle
{
    public class HeldItem
    {
        public byte ItemId { get; set; }
        public byte Quantity { get; set; }

        public HeldItem() { }

        public HeldItem(IList<byte> data)
        {
            ItemId = data[0];
            Quantity = data[1];
        }

        public HeldItem(int id, int quantity)
        {
            ItemId = (byte)id;
            Quantity = (byte)quantity;
        }

        public IEnumerable<byte> Encode()
        {
            return new byte[2] { ItemId, Quantity };
        }
    }
}
