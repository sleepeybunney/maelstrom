using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Menu;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.Maelstrom
{
    public class WeaponShuffle
    {
        public static List<WeaponUpgrade> Randomise(int seed, State settings)
        {
            var result = new List<WeaponUpgrade>();
            var random = new Random(seed + 9);

            for (int i = 0; i < 33; i++)
            {
                var itemPool = Item.Lookup.Values.Where(item => !item.KeyItem && !item.Magazine && !item.SummonItem && !item.ChocoboWorld).Select(item => item.ID).ToList();

                var upgrade = new WeaponUpgrade()
                {
                    Price = (byte)(random.Next(1, 251)),
                    Item1 = (byte)itemPool[random.Next(itemPool.Count)],
                    Item1Quantity = (byte)random.Next(1, 21)
                };

                if (random.Next(4) == 0)
                {
                    result.Add(upgrade);
                    continue;
                }

                itemPool.Remove(upgrade.Item1);
                upgrade.Item2 = (byte)itemPool[random.Next(itemPool.Count)];
                upgrade.Item2Quantity = (byte)random.Next(1, 21);

                if (random.Next(3) == 0)
                {
                    result.Add(upgrade);
                    continue;
                }

                itemPool.Remove(upgrade.Item2);
                upgrade.Item3 = (byte)itemPool[random.Next(itemPool.Count)];
                upgrade.Item3Quantity = (byte)random.Next(1, 21);

                if (random.Next(2) == 0)
                {
                    result.Add(upgrade);
                    continue;
                }

                itemPool.Remove(upgrade.Item3);
                upgrade.Item4 = (byte)itemPool[random.Next(itemPool.Count)];
                upgrade.Item4Quantity = (byte)random.Next(1, 21);

                result.Add(upgrade);
            }

            return result;
        }

        public static void Apply(FileSource menuSource, List<WeaponUpgrade> upgrades)
        {
            // fill in data from existing upgrade file
            var existingUpgrades = WeaponUpgrade.ReadAllFromSource(menuSource);
            for (int i = 0; i < existingUpgrades.Count; i++)
            {
                upgrades[i].MsgOffset = existingUpgrades[i].MsgOffset;
            }

            // write out new file
            var result = new List<byte>();
            foreach (var u in upgrades) result.AddRange(u.Encode());
            menuSource.ReplaceFile(Globals.WeaponUpgradePath, result.ToArray());
        }
    }
}
