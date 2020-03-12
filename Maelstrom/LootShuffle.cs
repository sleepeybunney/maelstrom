using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public static class LootShuffle
    {
        public static List<MonsterInfo> Randomise(FileSource battleSource, int seed)
        {
            var random = new Random(seed);
            var result = new List<MonsterInfo>();

            for (int i = 0; i < 144; i++)
            {
                Monster monster;
                try
                {
                    monster = Monster.ByID(battleSource, i);
                    monster.Info.MugLow = FourRandomItems(random);
                    monster.Info.MugMed = FourRandomItems(random);
                    monster.Info.MugHigh = FourRandomItems(random);
                    monster.Info.DropLow = FourRandomItems(random);
                    monster.Info.DropMed = FourRandomItems(random);
                    monster.Info.DropHigh = FourRandomItems(random);
                    battleSource.ReplaceFile(Monster.GetPath(i), monster.Encode());
                    result.Add(monster.Info);
                }
                catch { }
            }

            return result;
        }

        private static HeldItem RandomItem(Random random)
        {
            var id = random.Next(0, 199);
            var quantity = id == 0 ? 0 : random.Next(1, 11);
            return new HeldItem(id, quantity);
        }

        private static HeldItem[] FourRandomItems(Random random)
        {
            return new HeldItem[]
            {
                RandomItem(random),
                RandomItem(random),
                RandomItem(random),
                RandomItem(random)
            };
        }
    }
}
