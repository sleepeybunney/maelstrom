using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.Maelstrom
{
    public static class PriceFix
    {
        public static void Apply(FileSource menuSource)
        {
            var prices = Price.ReadAllFromSource(menuSource);

            // hero-trial -> 9000g
            prices[18].BasePrice = 900;

            // holy war-trial -> 19000g
            prices[20].BasePrice = 1900;

            // shell stone -> 1000g
            prices[22].BasePrice = 100;
            prices[22].SellMultiplier = 5;

            // protect stone -> 1000g
            prices[23].BasePrice = 100;
            prices[23].SellMultiplier = 5;

            // aura stone -> 2000g
            prices[24].BasePrice = 200;
            prices[24].SellMultiplier = 5;

            // death stone -> 2000g
            prices[25].BasePrice = 200;
            prices[25].SellMultiplier = 5;

            // holy stone -> 3500g
            prices[26].BasePrice = 350;
            prices[26].SellMultiplier = 5;

            // flare stone -> 3500g
            prices[27].BasePrice = 350;
            prices[27].SellMultiplier = 5;

            // meteor stone -> 5000g
            prices[28].BasePrice = 500;
            prices[28].SellMultiplier = 5;

            // ultima stone -> 5000g
            prices[29].BasePrice = 500;
            prices[29].SellMultiplier = 5;

            // gysahl greens -> 500g
            prices[30].BasePrice = 50;

            // phoenix pinion -> 5000g
            prices[31].BasePrice = 500;

            // friendship -> 1000g
            prices[32].BasePrice = 100;

            // girl next door -> 5000g
            prices[163].BasePrice = 500;

            // hp up -> 5000g
            prices[169].BasePrice = 500;

            // str up -> 5000g
            prices[170].BasePrice = 500;

            // vit up -> 5000g
            prices[171].BasePrice = 500;

            // mag up -> 5000g
            prices[172].BasePrice = 500;

            // spr up -> 5000g
            prices[173].BasePrice = 500;

            // spd up -> 5000g
            prices[174].BasePrice = 500;

            // luck up -> 5000g
            prices[175].BasePrice = 500;

            var newPriceFile = new List<byte>();
            foreach (var price in prices) newPriceFile.AddRange(price.Encode());
            menuSource.ReplaceFile(Env.PricePath, newPriceFile);
        }
    }
}
