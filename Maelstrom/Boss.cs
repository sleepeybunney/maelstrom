using System;
using System.Collections.Generic;
using System.Linq;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public static class Boss
    {
        // boss encounters, with all the individual monsters ranked by overall strength/importance
        // eg. gerogero (slot 1) ranked over the fake president (slot 0)
        // so whoever takes their place gets scaled to match the actual boss & not some other thing
        public static Dictionary<int, List<int>> Encounters = new Dictionary<int, List<int>>()
        {
            { 28, new List<int>() { 0 } },              // x-atm092
            { 29, new List<int>() { 2, 0, 1 } },        // elvoret + biggs + wedge
            { 62, new List<int>() { 0, 1, 2, 3 } },     // granaldo + raldo x3
            { 63, new List<int>() { 3, 0, 1, 2 } },     // norg + pod + left/right orbs
            { 79, new List<int>() { 0, 1 } },           // oilboyle x2
            { 83, new List<int>() { 1, 0, 2 } },        // raijin + g-soldier x2
            { 84, new List<int>() { 0, 1 } },           // raijin + fujin (balamb)
            { 86, new List<int>() { 0 } },              // propagator (red)
            { 94, new List<int>() { 0 } },              // ifrit
            { 104, new List<int>() { 1, 0 } },          // gerogero + fake president
            { 118, new List<int>() { 0 } },             // cerberus
            { 119, new List<int>() { 0, 1 } },          // seifer + gunblade (g-garden)
            { 120, new List<int>() { 2, 0, 1 } },       // edea + seifer + gunblade (g-garden)
            { 136, new List<int>() { 0 } },             // edea (parade) [crash warning]
            { 147, new List<int>() { 0, 1 } },          // iguion x2
            { 151, new List<int>() { 1, 0 } },          // biggs + wedge
            { 161, new List<int>() { 0, 1, 2 } },       // gim52a x2 + elite soldier
            { 164, new List<int>() { 0, 2, 1, 3 } },    // bgh251f2 (missile base) + elite soldier + g-soldier x2
            { 189, new List<int>() { 0 } },             // sacred
            { 190, new List<int>() { 0, 1 } },          // minotaur + sacred
            { 194, new List<int>() { 0 } },             // bgh251f2 (fh)
            { 195, new List<int>() { 0, 1 } },          // seifer + gunblade (parade)
            { 216, new List<int>() { 0, 1 } },          // abadon + shitty abadon
                                                        // 236-238 centra tonberries w/ hidden tonberry king
            { 317, new List<int>() { 0 } },             // odin [instakill warning]
            { 326, new List<int>() { 0 } },             // bahamut
            { 354, new List<int>() { 0 } },             // ultima weapon
            { 363, new List<int>() { 1, 0, 4, 3, 2 } }, // sphinxara + sphinxaur + tri-face + forbidden + jelleye
            { 372, new List<int>() { 0 } },             // krysta [finisher warning]
            { 377, new List<int>() { 0 } },             // tri-point
            { 410, new List<int>() { 0 } },             // catoblepas [finisher warning]
            { 431, new List<int>() { 1, 0, 2 } },       // trauma + droma x2
            { 436, new List<int>() { 3, 0, 1, 2 } },    // gargantua + vysage + lefty + righty
            { 441, new List<int>() { 0 } },             // red giant
            { 462, new List<int>() { 0 } },             // omega weapon
            { 483, new List<int>() { 0 } },             // tiamat
                                                        // 511 ultimecia
            { 712, new List<int>() { 0 } },             // jumbo cactuar
            { 794, new List<int>() { 0, 1, 2 } },       // mobile type 8 + left/right probes
            { 795, new List<int>() { 0, 1 } },          // seifer + gunblade (lunatic pandora)
            { 796, new List<int>() { 0, 1 } },          // adel + rinoa
            { 810, new List<int>() { 0, 1 } },          // raijin + fujin (lunatic pandora)
            { 811, new List<int>() { 0 } },             // diablos
            { 813, new List<int>() { 4, 1, 2, 0, 3 } }, // sorceress x5
                                                        // 814-819 more propagators
        };

        private static EncounterCheck[] EncounterChecks = new EncounterCheck[]
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
            var encFilePath = @"c:\ff8\data\eng\battle\scene.out";
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
                    var origSlots = Encounters[e];
                    var newSlots = Encounters[matchedEncounter];
                    var origMainID = encFile.Encounters[e].Slots[origSlots[0]].MonsterID;
                    var newMainID = encFile.Encounters[matchedEncounter].Slots[newSlots[0]].MonsterID;
                    var origMainInfo = Monster.ByID(battleSource, origMainID).Info;
                    var newMainInfo = Monster.ByID(battleSource, newMainID).Info;

                    for (int i = 0; i < newSlots.Count; i++)
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
            if (value < byte.MinValue) return byte.MinValue;
            if (value > byte.MaxValue) return byte.MaxValue;
            return (byte)value;
        }
    }
}
