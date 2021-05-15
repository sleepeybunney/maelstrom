using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.Json;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Battle;
using Sleepey.FF8Mod.Exe;

namespace Sleepey.Maelstrom
{
    public static class LootShuffle
    {
        public static List<MonsterMeta> Monsters = JsonSerializer.Deserialize<List<MonsterMeta>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Monsters.json")).ToList();
        public static List<Spell> Spells = JsonSerializer.Deserialize<List<Spell>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Spells.json")).ToList();

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

                        var meta = Monsters.First(m => m.MonsterID == i);
                        if (settings.LootDrawsUse && meta.SpellAnimationID > 0)
                        {
                            var lowCount = monster.Info.AbilitiesLow.Where(a => a.Type != AbilityType.None).Count();
                            var medCount = monster.Info.AbilitiesMed.Where(a => a.Type != AbilityType.None).Count();
                            var highCount = monster.Info.AbilitiesHigh.Where(a => a.Type != AbilityType.None).Count();
                            var filledSlots = Math.Max(lowCount, Math.Max(medCount, highCount));
                            var maxAbilities = 16;

                            if (filledSlots > 0 && filledSlots < maxAbilities)
                            {
                                var low = Spells.First(s => s.SpellID == monster.Info.DrawLow[0]);
                                var med = Spells.First(s => s.SpellID == monster.Info.DrawMed[0]);
                                var high = Spells.First(s => s.SpellID == monster.Info.DrawHigh[0]);

                                monster.Info.AbilitiesLow[15] = new MonsterAbility(AbilityType.Magic, meta.SpellAnimationID, (ushort)low.SpellID);
                                monster.Info.AbilitiesMed[15] = new MonsterAbility(AbilityType.Magic, meta.SpellAnimationID, (ushort)med.SpellID);
                                monster.Info.AbilitiesHigh[15] = new MonsterAbility(AbilityType.Magic, meta.SpellAnimationID, (ushort)high.SpellID);

                                var script = new List<BattleScriptInstruction>()
                                {
                                    // if rand(1/12)
                                    new BattleScriptInstruction("if", 0x02, 0x0c, 0x00, 0x00, 0x25),

                                    // if level == low
                                    new BattleScriptInstruction("if", 0x0e, 0xc8, 0x00, 0x00, 0x05),

                                    // target for low spell
                                    new BattleScriptInstruction("target", (short)low.Target),

                                    // else
                                    new BattleScriptInstruction("jmp", 0x12),

                                    // if level == med
                                    new BattleScriptInstruction("if", 0x0e, 0xc8, 0x00, 0x01, 0x05),

                                    // target for med spell
                                    new BattleScriptInstruction("target", (short)med.Target),

                                    // else
                                    new BattleScriptInstruction("jmp", 0x05),

                                    // target for high spell
                                    new BattleScriptInstruction("target", (short)high.Target),

                                    // end if
                                    new BattleScriptInstruction("jmp", 0x00),

                                    // cast spell
                                    new BattleScriptInstruction("use", 15),

                                    // end turn
                                    new BattleScriptInstruction("return"),

                                    // end if
                                    new BattleScriptInstruction("jmp", 0x00)
                                };

                                monster.AI.Scripts.Execute.InsertRange(0, script);
                            }
                        }
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
