using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public static class LootShuffle
    {
        public static List<MonsterInfo> Randomise(FileSource battleSource, int seed, State settings)
        {
            var random = new Random(seed);
            var result = new List<MonsterInfo>();

            for (int i = 0; i < 144; i++)
            {
                Monster monster;
                try
                {
                    monster = Monster.ByID(battleSource, i);

                    if (settings.LootSteals)
                    {
                        var mugPool = Item.Lookup.Values
                            .Where(item => !item.KeyItem || settings.LootStealsKeyItems)
                            .Where(item => !item.SummonItem || settings.LootStealsSummonItems)
                            .Where(item => !item.Magazine || settings.LootStealsMagazines)
                            .Where(item => !item.ChocoboWorld || settings.LootStealsChocoboWorld)
                            .Select(item => item.ID).ToList();

                        monster.Info.MugLow = FourRandomItems(random, mugPool);
                        monster.Info.MugMed = FourRandomItems(random, mugPool);
                        monster.Info.MugHigh = FourRandomItems(random, mugPool);
                    }

                    if (settings.LootDrops)
                    {
                        var dropPool = Item.Lookup.Values
                            .Where(item => !item.KeyItem || settings.LootDropsKeyItems)
                            .Where(item => !item.SummonItem || settings.LootDropsSummonItems)
                            .Where(item => !item.Magazine || settings.LootDropsMagazines)
                            .Where(item => !item.ChocoboWorld || settings.LootDropsChocoboWorld)
                            .Select(item => item.ID).ToList();

                        monster.Info.DropLow = FourRandomItems(random, dropPool);
                        monster.Info.DropMed = FourRandomItems(random, dropPool);
                        monster.Info.DropHigh = FourRandomItems(random, dropPool);
                    }

                    if (settings.LootDraws)
                    {
                        var drawPool = DrawPointShuffle.Spells
                            .Where(spell => spell.SpellID != 20 || settings.LootDrawsApoc)
                            .Where(spell => !spell.SlotExclusive || settings.LootDrawsSlot)
                            .Where(spell => !spell.CutContent || settings.LootDrawsCut)
                            .Select(spell => spell.SpellID).ToList();

                        var gf = monster.Info.DrawLow.Where(d => d >= 64).FirstOrDefault();
                        monster.Info.DrawLow = FourRandomSpells(random, drawPool, gf);
                        monster.Info.DrawMed = FourRandomSpells(random, drawPool, gf);
                        monster.Info.DrawHigh = FourRandomSpells(random, drawPool, gf);
                    }

                    battleSource.ReplaceFile(Monster.GetPath(i), monster.Encode());
                    result.Add(monster.Info);
                }
                catch { }
            }

            return result;
        }

        private static HeldItem RandomItem(Random random, List<int> itemPool)
        {
            // allow item slot to be empty
            if (!itemPool.Contains(0)) itemPool.Add(0);

            var id = itemPool[random.Next(itemPool.Count)];
            var quantity = id == 0 ? 0 : random.Next(1, 9);
            return new HeldItem(id, quantity);
        }

        private static HeldItem[] FourRandomItems(Random random, List<int> itemPool)
        {
            return new HeldItem[]
            {
                RandomItem(random, itemPool),
                RandomItem(random, itemPool),
                RandomItem(random, itemPool),
                RandomItem(random, itemPool)
            };
        }

        private static byte RandomSpell(Random random, List<int> spellPool)
        {
            return (byte)spellPool[random.Next(spellPool.Count)];
        }

        private static byte[] FourRandomSpells(Random random, List<int> spellPool, byte gf = 0)
        {
            return new byte[]
            {
                RandomSpell(random, spellPool),
                RandomSpell(random, spellPool),
                RandomSpell(random, spellPool),
                (gf > 0) ? gf : RandomSpell(random, spellPool)
            };
        }
    }
}
