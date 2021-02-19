using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.Maelstrom
{
    public static class LootShuffle
    {
        public static List<MonsterInfo> Randomise(FileSource battleSource, int seed, State settings)
        {
            var dropRandom = new Random(seed + 5);
            var stealRandom = new Random(seed + 6);
            var drawRandom = new Random(seed + 7);
            var result = new List<MonsterInfo>();

            for (int i = 0; i < 144; i++)
            {
                Monster monster;
                try
                {
                    monster = Monster.ByID(battleSource, i);

                    // items to steal
                    if (settings.LootSteals)
                    {
                        var mugPool = Item.Lookup.Values
                            .Where(item => !item.KeyItem || settings.LootStealsKeyItems)
                            .Where(item => !item.SummonItem || settings.LootStealsSummonItems)
                            .Where(item => !item.Magazine || settings.LootStealsMagazines)
                            .Where(item => !item.ChocoboWorld || settings.LootStealsChocoboWorld)
                            .Select(item => item.ID).ToList();

                        monster.Info.MugLow = FourRandomItems(stealRandom, mugPool);
                        monster.Info.MugMed = FourRandomItems(stealRandom, mugPool);
                        monster.Info.MugHigh = FourRandomItems(stealRandom, mugPool);
                    }

                    // items dropped
                    if (settings.LootDrops)
                    {
                        var dropPool = Item.Lookup.Values
                            .Where(item => !item.KeyItem || settings.LootDropsKeyItems)
                            .Where(item => !item.SummonItem || settings.LootDropsSummonItems)
                            .Where(item => !item.Magazine || settings.LootDropsMagazines)
                            .Where(item => !item.ChocoboWorld || settings.LootDropsChocoboWorld)
                            .Select(item => item.ID).ToList();

                        monster.Info.DropLow = FourRandomItems(dropRandom, dropPool);
                        monster.Info.DropMed = FourRandomItems(dropRandom, dropPool);
                        monster.Info.DropHigh = FourRandomItems(dropRandom, dropPool);
                    }

                    // spells to draw
                    if (settings.LootDraws)
                    {
                        var drawPool = DrawPointShuffle.Spells
                            .Where(spell => spell.SpellID != 20 || settings.LootDrawsApoc)
                            .Where(spell => !spell.SlotExclusive || settings.LootDrawsSlot)
                            .Where(spell => !spell.CutContent || settings.LootDrawsCut)
                            .Select(spell => spell.SpellID).ToList();

                        var gf = monster.Info.DrawLow.Where(d => d >= 64).FirstOrDefault();
                        var slots = Math.Max(1, Math.Min(4, settings.LootDrawsAmount));

                        monster.Info.DrawLow = FourRandomSpells(drawRandom, drawPool, slots, gf);
                        monster.Info.DrawMed = FourRandomSpells(drawRandom, drawPool, slots, gf);
                        monster.Info.DrawHigh = FourRandomSpells(drawRandom, drawPool, slots, gf);
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

        private static byte[] FourRandomSpells(Random random, List<int> spellPool, int slots, byte gf = 0)
        {
            var result = new List<byte>();

            for (int i = 0; i < 4; i++)
            {
                byte spell = 0;

                // if there's a gf it takes the last slot
                if (i == slots - 1 && gf > 0) spell = gf;
                else if (i < slots) spell = RandomSpell(random, spellPool);

                result.Add(spell);
            }

            return result.ToArray();
        }
    }
}
