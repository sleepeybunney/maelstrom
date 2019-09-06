using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public class Reward
    {
        public static List<Reward> Major = new List<Reward>()
        {
            new Reward("squall", RewardType.Character, 0),
            new Reward("zell", RewardType.Character, 1),
            new Reward("irvine", RewardType.Character, 2),
            new Reward("quistis", RewardType.Character, 3),
            new Reward("rinoa", RewardType.Character, 4),
            new Reward("selphie", RewardType.Character, 5),
            new Reward("seifer", RewardType.Character, 6),
            new Reward("edea", RewardType.Character, 7),
            new Reward("laguna", RewardType.Character, 8),
            new Reward("kiros", RewardType.Character, 9),
            new Reward("ward", RewardType.Character, 10),

            new Reward("quezacotl", RewardType.GF, 3),
            new Reward("shiva", RewardType.GF, 4),
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

            new Reward("odin", RewardType.Special, 127),
            new Reward("angel wing", RewardType.Special, 126),

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
        };

        public static List<Reward> Minor = new List<Reward>()
        {
            new Reward("mega potion", RewardType.Item, 6),
            new Reward("mega phoenix", RewardType.Item, 8),
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
            new Reward("rosetta stone", RewardType.Item, 54),
            new Reward("hungry cookpot", RewardType.Item, 64),
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
        public int ID;
        public int Quantity;

        public Reward(string name, RewardType type, int id = -1, int quantity = 1)
        {
            Name = name;
            Type = type;
            ID = id;
            Quantity = quantity;
        }

        public static void GiveItem(FileSource battleSource, int encounterID, int itemID)
        {
            var encounter = EncounterFile.FromSource(battleSource).Encounters[encounterID];
            var slot = encounter.Slots[Boss.Encounters[encounterID][0]];
            var monster = slot.GetMonster(battleSource);
            var awardInstruction = new Battle.Instruction(Battle.Instruction.OpCodes[0x38], new short[] { (short)itemID });
            monster.AI.Scripts.Init.Insert(0, awardInstruction);
            battleSource.ReplaceFile(Monster.GetPath(slot.MonsterID), monster.Encode());
        }

        public static void SetRewards(FileSource battleSource, int seed)
        {
            var random = new Random(seed);
            var major = new List<Reward>(Major);
            var minor = new List<Reward>(Minor);
            var rewardMap = new Dictionary<int, Reward>();
            var encounterFile = EncounterFile.FromSource(battleSource);

            foreach (var enc in Boss.Encounters.Keys)
            {
                // clear existing item drops
                foreach (var slot in encounterFile.Encounters[enc].Slots)
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

                var majorIndex = random.Next(0, major.Count);
                var minorIndex = random.Next(0, minor.Count);
                
                switch (major[majorIndex].Type)
                {
                    case RewardType.Item:
                        GiveItem(battleSource, enc, major[majorIndex].ID);
                        break;
                }

                // minor rewards are all items
                GiveItem(battleSource, enc, minor[minorIndex].ID);

                // remove rewards from the pool
                major.RemoveAt(majorIndex);
                minor.RemoveAt(minorIndex);
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
