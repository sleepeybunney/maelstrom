using System;
using System.Collections.Generic;
using System.Linq;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public class Boss
    {
        public int EncounterID;
        public string FieldName;
        public int FieldEntity;
        public int FieldScript;
        public bool FixedField;
        public int[] SlotRanks;

        public Boss(int encounterID, string fieldName, int fieldEntity, int fieldScript, bool fixedField, params int[] slotRanks)
        {
            EncounterID = encounterID;
            FieldName = fieldName;
            FieldEntity = fieldEntity;
            FieldScript = fieldScript;
            FixedField = fixedField;
            SlotRanks = slotRanks;
        }

        // boss encounters, with all the individual monsters ranked by overall strength/importance
        // eg. gerogero (slot 1) ranked over the fake president (slot 0)
        // so whoever takes their place gets scaled to match the actual boss & not some other thing
        public static List<Boss> Bosses = new List<Boss>()
        {
            new Boss(28, "domt2_1", 18, 5, false, 0),               // x-atm092 (also in domt3_2/10.4 + several instances of enc 27)
            new Boss(29, "doani4_2", 10, 4, true, 2, 0, 1),         // elvoret + biggs + wedge
            new Boss(62, "bgmon_1", 0, 7, true, 0, 1, 2, 3),        // granaldo + raldo x3
            new Boss(63, "bgmast_1", 13, 14, true, 3, 0, 1, 2),     // norg + pod + left/right orbs
            new Boss(79, "bgmd3_1", 11, 4, true, 0, 1),             // oilboyle x2
            new Boss(83, "bcsaka1a", 6, 6, true, 1, 0, 2),          // raijin + g-soldier x2 (also in script 0.7)
            new Boss(84, "bchtl_1", 0, 5, true, 0, 1),              // raijin + fujin (balamb)
            new Boss(85, "rgroad2", 10, 1, true, 0),                // propagator (red) (also in rgroad3/8.1)
            new Boss(94, "bdifrit1", 14, 4, true, 0),               // ifrit
            new Boss(104, "titrain1", 9, 1, true, 1, 0),            // gerogero + fake president
            new Boss(118, "gghall1", 14, 2, true, 0),               // cerberus
            new Boss(119, "ggwitch2", 11, 1, true, 0, 1),           // seifer + gunblade (g-garden)
            new Boss(120, "ggkodo2", 8, 4, true, 2, 0, 1),          // edea + seifer + gunblade (g-garden)
            new Boss(136, "glyagu1", 2, 4, true, 0),                // edea (parade) [crash warning]
            new Boss(147, "glwitch1", 12, 1, true, 0, 1),           // iguion x2
            new Boss(151, "gpbigin3", 12, 4, true, 1, 0),           // biggs + wedge
            new Boss(161, "gpexit1", 9, 1, true, 0, 1, 2),          // gim52a x2 + elite soldier
            new Boss(164, "gmpark2", 0, 7, true, 0, 2, 1, 3),       // bgh251f2 (missile base) + elite soldier + g-soldier x2 (also in 1.7)
            new Boss(189, "gnroom1", 13, 4, true, 0),               // sacred
            new Boss(190, "gnroom4", 0, 7, true, 0, 1),             // minotaur + sacred
            new Boss(194, "fhtown23", 0, 9, true, 0),               // bgh251f2 (fh)
            new Boss(195, "glyagu1", 2, 4, true, 0, 1),             // seifer + gunblade (parade)
            new Boss(216, "elstop1", 8, 4, true, 0, 1),             // abadon + shitty abadon
                                                                    // 236-238 centra tonberries w/ hidden tonberry king
            new Boss(317, "crodin1", 7, 3, true, 0),                // odin [instakill warning]
            new Boss(326, "sdcore1", 1, 5, true, 0),                // bahamut
            new Boss(354, "ddruins6", 17, 1, true, 0),              // ultima weapon
            new Boss(363, "fehall2", 9, 3, true, 1, 0, 4, 3, 2),    // sphinxara + sphinxaur + tri-face + forbidden + jelleye
            new Boss(372, "feteras1", 9, 3, true, 0),               // krysta [finisher warning]
            new Boss(377, "fewine1", 7, 3, true, 0),                // tri-point
            new Boss(410, "fetre1", 22, 3, true, 0),                // catoblepas [finisher warning]
            new Boss(431, "feart1f1", 17, 3, true, 1, 0, 2),        // trauma + droma x2
            new Boss(436, "febarac1", 8, 3, true, 3, 0, 1, 2),      // gargantua + vysage + lefty + righty
            new Boss(441, "fejail1", 14, 2, true, 0),               // red giant (also in 14.4)
            new Boss(462, "fewor1", 7, 3, true, 0),                 // omega weapon
            new Boss(483, "feclock3", 8, 3, true, 0),               // tiamat
                                                                    // 511 ultimecia (felast1/10.1)
            new Boss(712, "", 0, 0, false, 0),                      // jumbo cactuar
            new Boss(794, "ebinhi1a", 18, 1, true, 0, 1, 2),        // mobile type 8 + left/right probes
            new Boss(795, "ebcont1", 12, 1, true, 0, 1),            // seifer + gunblade (lunatic pandora)
            new Boss(796, "ebadele3", 4, 1, true, 0, 1),            // adel + rinoa
            new Boss(810, "ebinto1", 0, 7, true, 0, 1),             // raijin + fujin (lunatic pandora)
            new Boss(811, "", 0, 0, false, 0),                      // diablos
            new Boss(813, "glwitch3", 10, 4, true, 4, 1, 2, 0, 3),  // sorceress x5
                                                                    // 814-819 more propagators
        };

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

                            List<Battle.Instruction> script;
                            switch (ec.Script)
                            {
                                case 0:
                                default:
                                    script = monster.AI.Scripts.Init;
                                    break;
                                case 1:
                                    script = monster.AI.Scripts.Execute;
                                    break;
                                case 2:
                                    script = monster.AI.Scripts.Counter;
                                    break;
                                case 3:
                                    script = monster.AI.Scripts.Death;
                                    break;
                                case 4:
                                    script = monster.AI.Scripts.PreCounter;
                                    break;
                            }

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
