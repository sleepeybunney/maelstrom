using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Battle;
using Sleepey.FF8Mod.Field;

namespace Sleepey.Maelstrom
{
    public class Reward
    {
        public static List<Reward> Major = new List<Reward>()
        {
            //new Reward("Squall", RewardType.Character, 0),
            new Reward("Zell", RewardType.Character, 1),
            new Reward("Irvine", RewardType.Character, 2),
            //new Reward("Quistis", RewardType.Character, 3),
            new Reward("Rinoa", RewardType.Character, 4),
            new Reward("Selphie", RewardType.Character, 5),
            new Reward("Seifer", RewardType.Character, 6),
            new Reward("Edea", RewardType.Character, 7),
            //new Reward("laguna", RewardType.Character, 8),
            //new Reward("kiros", RewardType.Character, 9),
            //new Reward("ward", RewardType.Character, 10),

            //new Reward("quezacotl", RewardType.GF, 3),
            //new Reward("shiva", RewardType.GF, 4),
            new Reward("ifrit", RewardType.GF, 5),
            new Reward("siren", RewardType.GF, 6),
            new Reward("brothers", RewardType.GF, 7),
            new Reward("carbuncle", RewardType.GF, 8),
            new Reward("diablos", RewardType.GF, 9),
            new Reward("leviathan", RewardType.GF, 10),
            new Reward("pandemona", RewardType.GF, 11),
            new Reward("cerberus", RewardType.GF, 12),
            new Reward("alexander", RewardType.GF, 13),
            new Reward("doomtrain", RewardType.GF, 14),
            new Reward("bahamut", RewardType.GF, 15),
            new Reward("cactuar", RewardType.GF, 16),
            new Reward("tonberry", RewardType.GF, 17),
            new Reward("eden", RewardType.GF, 18),

            new Reward("Odin", RewardType.Special, 295),
            new Reward("Angel Wing", RewardType.Special, 294),

            new Reward("item", RewardType.Seal, 1),
            new Reward("magic", RewardType.Seal, 2),
            new Reward("gf", RewardType.Seal, 4),
            new Reward("draw", RewardType.Seal, 8),
            new Reward("command", RewardType.Seal, 16),
            new Reward("limit break", RewardType.Seal, 32),
            new Reward("resurrect", RewardType.Seal, 64),
            new Reward("save", RewardType.Seal, 128),

            new Reward("garden key", RewardType.Item, 163),
            new Reward("esthar key", RewardType.Item, 164),

            new Reward("magical lamp", RewardType.Item, 168),
            new Reward("solomon ring", RewardType.Item, 167),
            new Reward("power generator", RewardType.Item, 140),
            new Reward("dark matter", RewardType.Item, 141),
            new Reward("ribbon", RewardType.Item, 100),
            new Reward("phoenix pinion", RewardType.Item, 31),
            new Reward("holy war", RewardType.Item, 21),
            new Reward("hero", RewardType.Item, 19),
            new Reward("rosetta stone", RewardType.Item, 54),
            new Reward("hungry cookpot", RewardType.Item, 64),
        };

        public static List<Reward> Minor = new List<Reward>()
        {
            new Reward("mega potion", RewardType.Item, 6),
            new Reward("mega phoenix", RewardType.Item, 8),
            new Reward("elixir", RewardType.Item, 9),
            new Reward("megalixir", RewardType.Item, 10),
            new Reward("elixir", RewardType.Item, 9),
            new Reward("megalixir", RewardType.Item, 10),
            new Reward("elixir", RewardType.Item, 9),
            new Reward("megalixir", RewardType.Item, 10),
            new Reward("aura stone", RewardType.Item, 24),
            new Reward("death stone", RewardType.Item, 25),
            new Reward("holy stone", RewardType.Item, 26),
            new Reward("flare stone", RewardType.Item, 27),
            new Reward("meteor stone", RewardType.Item, 28),
            new Reward("ultima stone", RewardType.Item, 29),
            new Reward("g-returner", RewardType.Item, 39),
            new Reward("hp-j scroll", RewardType.Item, 42),
            new Reward("str-j scroll", RewardType.Item, 43),
            new Reward("vit-j scroll", RewardType.Item, 44),
            new Reward("mag-j scroll", RewardType.Item, 45),
            new Reward("spr-j scroll", RewardType.Item, 46),
            new Reward("spd-j scroll", RewardType.Item, 47),
            new Reward("luck-j scroll", RewardType.Item, 48),
            new Reward("aegis amulet", RewardType.Item, 49),
            new Reward("elem atk", RewardType.Item, 50),
            new Reward("elem guard", RewardType.Item, 51),
            new Reward("status atk", RewardType.Item, 52),
            new Reward("status guard", RewardType.Item, 53),
            new Reward("gaea's ring", RewardType.Item, 76),
            new Reward("samantha soul", RewardType.Item, 69),
            new Reward("hyper wrist", RewardType.Item, 79),
            new Reward("adamantine", RewardType.Item, 82),
            new Reward("magic armlet", RewardType.Item, 85),
            new Reward("royal crown", RewardType.Item, 88),
            new Reward("rocket engine", RewardType.Item, 90),
            new Reward("three stars", RewardType.Item, 99),
            new Reward("gambler spirit", RewardType.Item, 59),
            new Reward("friendship", RewardType.Item, 32),
            new Reward("mog's amulet", RewardType.Item, 65),
            new Reward("weapons mon 1st", RewardType.Item, 177),
            new Reward("weapons mon mar", RewardType.Item, 178),
            new Reward("weapons mon apr", RewardType.Item, 179),
            new Reward("weapons mon may", RewardType.Item, 180),
            new Reward("weapons mon jun", RewardType.Item, 181),
            new Reward("weapons mon jul", RewardType.Item, 182),
            new Reward("weapons mon aug", RewardType.Item, 183),
            new Reward("combat king 001", RewardType.Item, 184),
            new Reward("combat king 002", RewardType.Item, 185),
            new Reward("combat king 003", RewardType.Item, 186),
            new Reward("combat king 004", RewardType.Item, 187),
            new Reward("combat king 005", RewardType.Item, 188),
            new Reward("pet pals vol. 1", RewardType.Item, 189),
            new Reward("pet pals vol. 2", RewardType.Item, 190),
            new Reward("pet pals vol. 3", RewardType.Item, 191),
            new Reward("pet pals vol. 4", RewardType.Item, 192),
            new Reward("pet pals vol. 5", RewardType.Item, 193),
            new Reward("pet pals vol. 6", RewardType.Item, 194),
        };

        public string Name;
        public RewardType Type;
        public short ID;
        public int Quantity;

        public Reward(string name, RewardType type, short id = -1, int quantity = 1)
        {
            Name = name;
            Type = type;
            ID = id;
            Quantity = quantity;
        }

        public static void GiveCharacter(FileSource fieldSource, int encounterID, Reward reward)
        {
            GiveFieldReward(fieldSource, encounterID, FieldScript.OpCodesReverse["addmember"], new int[] { reward.ID }, reward.Name + " joined the party!");
        }

        public static void GiveGF(FileSource battleSource, EncounterFile encFile, int encounterID, short gfID)
        {
            GiveBattleReward(battleSource, encFile, encounterID, "award-gf", gfID);
        }

        public static void GiveSpecial(FileSource fieldSource, int encounterID, Reward reward)
        {
            var args = reward.Name == "angel wing" ? new int[] { 1 } : Array.Empty<int>();
            GiveFieldReward(fieldSource, encounterID, reward.ID, args, reward.Name + " unlocked!");
        }

        public static void GiveItem(FileSource battleSource, EncounterFile encFile, int encounterID, short itemID)
        {
            GiveBattleReward(battleSource, encFile, encounterID, "award-item", itemID);
        }

        private static void GiveBattleReward(FileSource battleSource, EncounterFile encFile, int encounterID, string rewardOp, short rewardID)
        {
            var encounter = encFile.Encounters[encounterID];
            var slot = encounter.Slots[Boss.Encounters[encounterID].SlotRanks[0]];
            var monster = slot.GetMonster(battleSource);
            var awardInstruction = new BattleScriptInstruction(rewardOp, rewardID);
            monster.AI.Scripts.Init.Insert(0, awardInstruction);
            battleSource.ReplaceFile(Monster.GetPath(slot.MonsterID), monster.Encode());
        }

        private static void GiveFieldReward(FileSource fieldSource, int encounterID, int opCode, int[] args, string message)
        {
            var boss = Boss.Encounters[encounterID];
            var fieldPath = FieldScript.GetFieldPath(boss.FieldID);
            var innerSource = new InnerFileSource(fieldPath, fieldSource);

            // add message
            var msdPath = Path.Combine(fieldPath, boss.FieldID + Globals.MessageFileExtension);
            var fieldText = MessageFile.FromSource(innerSource, msdPath);
            var msgID = fieldText.Messages.Count;
            fieldText.Messages.Add(message);

            // give reward
            var field = FieldScript.FromSource(fieldSource, boss.FieldID);
            var script = field.Entities[boss.FieldEntity].Scripts[boss.FieldScript];
            var index = script.Instructions.FindLastIndex(i => i.OpCode == FieldScript.OpCodesReverse["battle"]) + 1;

            var awardInstructions = new List<FieldScriptInstruction>();
            var push = FieldScript.OpCodesReverse["pshn_l"];
            foreach (var a in args)
            {
                awardInstructions.Add(new FieldScriptInstruction(push, a));
            }
            awardInstructions.Add(new FieldScriptInstruction(opCode));

            // show message
            awardInstructions.Add(new FieldScriptInstruction(push, 0));
            awardInstructions.Add(new FieldScriptInstruction(push, msgID));
            awardInstructions.Add(new FieldScriptInstruction(push, 70));
            awardInstructions.Add(new FieldScriptInstruction(push, 70));
            awardInstructions.Add(new FieldScriptInstruction(FieldScript.OpCodesReverse["amesw"]));

            // apply changes
            script.Instructions.InsertRange(index, awardInstructions);
            innerSource.ReplaceFile(msdPath, fieldText.Encode());
            innerSource.ReplaceFile(FieldScript.GetFieldPath(boss.FieldID) + "\\" + boss.FieldID + Globals.ScriptFileExtension, field.Encode());
        }

        public static void SetRewards(FileSource battleSource, FileSource fieldSource, int seed)
        {
            var random = new Random(seed + 11);
            var major = new List<Reward>(Major);
            var minor = new List<Reward>(Minor);
            var encounterFile = EncounterFile.FromSource(battleSource);

            // bosses with no fixed location are assigned rewards that don't require field scripting
            foreach (var boss in Boss.Bosses.Where(b => !b.FixedField))
            {
                var battleOnlyMajor = major.Where(r => r.Type == RewardType.GF || r.Type == RewardType.Item).ToList();
                var majorIndex = random.Next(0, battleOnlyMajor.Count);
                var minorIndex = random.Next(0, minor.Count);

                // remove any existing item drops
                ClearDrops(battleSource, encounterFile, boss.EncounterID);

                switch (battleOnlyMajor[majorIndex].Type)
                {
                    case RewardType.GF:
                        GiveGF(battleSource, encounterFile, boss.EncounterID, battleOnlyMajor[majorIndex].ID);
                        break;
                    case RewardType.Item:
                        GiveItem(battleSource, encounterFile, boss.EncounterID, battleOnlyMajor[majorIndex].ID);
                        break;
                }

                // minor rewards are all items
                GiveItem(battleSource, encounterFile, boss.EncounterID, minor[minorIndex].ID);

                // remove rewards from the pool
                major.Remove(battleOnlyMajor[majorIndex]);
                battleOnlyMajor.RemoveAt(majorIndex);
                minor.RemoveAt(minorIndex);
            }

            // the rest of the bosses can be assigned anything
            foreach (var boss in Boss.Bosses.Where(b => b.FixedField))
            {
                if (major.Count > 0)
                {
                    var majorIndex = random.Next(0, major.Count);

                    // remove any existing item drops
                    ClearDrops(battleSource, encounterFile, boss.EncounterID);

                    switch (major[majorIndex].Type)
                    {
                        case RewardType.Character:
                            GiveCharacter(fieldSource, boss.EncounterID, major[majorIndex]);
                            break;
                        case RewardType.GF:
                            GiveGF(battleSource, encounterFile, boss.EncounterID, major[majorIndex].ID);
                            break;
                        case RewardType.Special:
                            GiveSpecial(fieldSource, boss.EncounterID, major[majorIndex]);
                            break;
                        case RewardType.Seal:
                            break;
                        case RewardType.Item:
                            GiveItem(battleSource, encounterFile, boss.EncounterID, major[majorIndex].ID);
                            break;
                    }

                    major.RemoveAt(majorIndex);
                }

                // minor rewards are all items
                var minorIndex = random.Next(0, minor.Count);
                GiveItem(battleSource, encounterFile, boss.EncounterID, minor[minorIndex].ID);
                minor.RemoveAt(minorIndex);
            }
        }

        // remove existing rewards from an encounter
        private static void ClearDrops(FileSource battleSource, EncounterFile encFile, int encounterID)
        {
            foreach (var slot in encFile.Encounters[encounterID].Slots)
            {
                var monster = slot.GetMonster(battleSource);
                for (int i = 0; i < 4; i++)
                {
                    monster.Info.DropLow[i] = new HeldItem();
                    monster.Info.DropMed[i] = new HeldItem();
                    monster.Info.DropHigh[i] = new HeldItem();
                }
                battleSource.ReplaceFile(Monster.GetPath(slot.MonsterID), monster.Encode());
            }
        }
    }

    public enum RewardType
    {
        Character,
        GF,
        Seal,
        Special,
        Item
    }
}
