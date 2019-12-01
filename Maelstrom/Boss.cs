using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public class Boss
    {
        public int EncounterID { get; set; }
        public string EncounterName { get; set; }
        public string FieldID { get; set; }
        public string FieldName { get; set; }
        public int FieldEntity { get; set; }
        public int FieldScript { get; set; }
        public bool FixedField { get; set; }
        public int[] SlotRanks { get; set; }

        public static List<Boss> Bosses = JsonSerializer.Deserialize<List<Boss>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Bosses.json"));
        public static Dictionary<int, Boss> Encounters = PopulateEncounterDictionary();

        private static Dictionary<int, Boss> PopulateEncounterDictionary()
        {
            var result = new Dictionary<int, Boss>();
            foreach (var b in Bosses) result.Add(b.EncounterID, b);
            return result;
        }

        // registry of boss monster battle scripts that check the current encounter ID
        private readonly static EncounterCheck[] EncounterChecks = new EncounterCheck[]
        {
            // bgh251f2
            new EncounterCheck(164, 71, 0, 0),
            new EncounterCheck(164, 72, 0, 0),
            new EncounterCheck(164, 72, 1, 0),

            // gargantua
            new EncounterCheck(436, 39, 3, 2),
            new EncounterCheck(436, 40, 3, 2),
            new EncounterCheck(436, 54, 3, 2),

            // sacred
            new EncounterCheck(189, 63, 1, 10),
            new EncounterCheck(189, 63, 3, 2),

            // granaldo
            new EncounterCheck(62, 95, 1, 3),
            new EncounterCheck(62, 96, 1, 3),

            // gim52a
            new EncounterCheck(161, 72, 0, 4),

            // raijin & soldiers
            new EncounterCheck(83, 120, 0, 5),
            new EncounterCheck(83, 120, 1, 0),
            new EncounterCheck(83, 120, 3, 2),

            // raijin & fujin
            new EncounterCheck(84, 120, 2, 0),
        };

        private struct EncounterCheck
        {
            public int EncounterID;
            public int MonsterID;
            public int Script;
            public int Instruction;

            public EncounterCheck(int encID, int monID, int script, int instruction)
            {
                EncounterID = encID;
                MonsterID = monID;
                Script = script;
                Instruction = instruction;
            }
        }

        public static void Shuffle(FileSource battleSource, bool rebalance, int seed)
        {
            var encFilePath = EncounterFile.Path;
            var encFile = EncounterFile.FromSource(battleSource, encFilePath);
            var random = new Random(seed);
            var encList = Encounters.Keys.ToList();
            var matchList = Encounters.Keys.ToList();
            var monsterMap = new Dictionary<int, List<EncounterSlot>>();
            var statMap = new Dictionary<int, MonsterInfo>();
            var encIdMap = new Dictionary<int, int>();

            foreach (var e in encList)
            {
                // assign boss monsters to encounters
                var matchedEncounter = matchList[random.Next(matchList.Count)];
                matchList.Remove(matchedEncounter);
                var monsters = encFile.Encounters[matchedEncounter].Slots.ToList();
                monsterMap.Add(e, monsters);
                encIdMap.Add(e, matchedEncounter);
                
                // calculate monster stats to match their assigned encounters
                if (rebalance)
                {
                    var origSlots = Encounters[e].SlotRanks;
                    var newSlots = Encounters[matchedEncounter].SlotRanks;
                    var origMainID = encFile.Encounters[e].Slots[origSlots[0]].MonsterID;
                    var newMainID = encFile.Encounters[matchedEncounter].Slots[newSlots[0]].MonsterID;
                    var origMainInfo = Monster.ByID(battleSource, origMainID).Info;
                    var newMainInfo = Monster.ByID(battleSource, newMainID).Info;

                    for (int i = 0; i < newSlots.Length; i++)
                    {
                        var newMonsterID = encFile.Encounters[matchedEncounter].Slots[newSlots[i]].MonsterID;
                        var newMonsterInfo = Monster.ByID(battleSource, newMonsterID).Info;

                        if (!statMap.Keys.Contains(newMonsterID))
                        {
                            var newNewMonsterInfo = new MonsterInfo();
                            newNewMonsterInfo.CopyStats(newMonsterInfo);

                            newNewMonsterInfo.Hp = ScaleStat(newNewMonsterInfo.Hp, newMainInfo.Hp, origMainInfo.Hp);
                            newNewMonsterInfo.Str = ScaleStat(newNewMonsterInfo.Str, newMainInfo.Str, origMainInfo.Str);
                            newNewMonsterInfo.Mag = ScaleStat(newNewMonsterInfo.Mag, newMainInfo.Mag, origMainInfo.Mag);
                            newNewMonsterInfo.Vit = ScaleStat(newNewMonsterInfo.Vit, newMainInfo.Vit, origMainInfo.Vit);
                            newNewMonsterInfo.Spr = ScaleStat(newNewMonsterInfo.Spr, newMainInfo.Spr, origMainInfo.Spr);
                            newNewMonsterInfo.Spd = ScaleStat(newNewMonsterInfo.Spd, newMainInfo.Spd, origMainInfo.Spd);
                            newNewMonsterInfo.Eva = ScaleStat(newNewMonsterInfo.Eva, newMainInfo.Eva, origMainInfo.Eva);

                            statMap.Add(newMonsterID, newNewMonsterInfo);
                        }
                    }
                }
            }

            // write shuffled bosses to the encounter file
            foreach (var e in encList)
            {
                for (int i = 0; i < 8; i++)
                {
                    encFile.Encounters[e].Slots[i] = monsterMap[e][i];

                    // update any encounter ID checks in the monster's AI scripts
                    var monsterID = encFile.Encounters[e].Slots[i].MonsterID;
                    foreach (var ec in EncounterChecks.Where(ec => ec.MonsterID == monsterID))
                    {
                        if (ec.EncounterID == encIdMap[e])
                        {
                            var monster = encFile.Encounters[e].Slots[i].GetMonster(battleSource);
                            var script = monster.AI.Scripts.EventScripts[ec.Script];
                            script[ec.Instruction].Args[3] = (short)e;
                            battleSource.ReplaceFile(Monster.GetPath(monsterID), monster.Encode());
                        }
                    }
                }
            }

            battleSource.ReplaceFile(encFilePath, encFile.Encode());

            // write rebalanced stats to the monster files
            if (rebalance)
            {
                foreach (var id in statMap.Keys)
                {
                    var monster = Monster.ByID(battleSource, id);
                    monster.Info.CopyStats(statMap[id]);
                    battleSource.ReplaceFile(Monster.GetPath(id), monster.Encode());
                }
            }
        }

        // scale the stats of a monster against another, relative to the main boss of its encounter
        // eg. swapping ifrit with adel/rinoa
        // --> ifrit & adel (designated the "main" bosses) swap stats directly
        // --> rinoa, without a counterpart, scales to whatever % weaker than adel she was before
        public static MonsterInfo ScaleMonster(MonsterInfo current, MonsterInfo main, MonsterInfo scaleTo)
        {
            var result = new MonsterInfo();
            result.CopyStats(current);

            result.Hp = ScaleStat(result.Hp, main.Hp, scaleTo.Hp);
            result.Str = ScaleStat(result.Str, main.Str, scaleTo.Str);
            result.Mag = ScaleStat(result.Mag, main.Mag, scaleTo.Mag);
            result.Vit = ScaleStat(result.Vit, main.Vit, scaleTo.Vit);
            result.Spr = ScaleStat(result.Spr, main.Spr, scaleTo.Spr);
            result.Spd = ScaleStat(result.Spd, main.Spd, scaleTo.Spd);
            result.Eva = ScaleStat(result.Eva, main.Eva, scaleTo.Eva);

            return result;
        }

        public static byte[] ScaleStat(byte[] current, byte[] main, byte[] scaleTo)
        {
            if (current.SequenceEqual(main)) return scaleTo;

            var result = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                var fractionOfMain = ((float)current[i] + 1) / ((float)main[i] + 1);
                result[i] = ByteClamp((int)(fractionOfMain + Math.Log10(fractionOfMain) * scaleTo[i]));
            }

            if (result.All(x => x == 0)) result[1] = 1;
            return result;
        }

        private static byte ByteClamp(int value)
        {
            if (value < 1) return 1;
            if (value > byte.MaxValue) return byte.MaxValue;
            return (byte)value;
        }
    }
}
