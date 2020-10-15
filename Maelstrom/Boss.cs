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

        public static List<Boss> Bosses = JsonSerializer.Deserialize<List<Boss>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.Bosses.json"));
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

        public static Dictionary<int, int> Shuffle(FileSource battleSource, bool rebalance, int seed)
        {
            var random = new Random(seed);

            var encFilePath = EncounterFile.Path;
            var sourceFile = EncounterFile.FromSource(battleSource, encFilePath);
            var newFile = EncounterFile.FromSource(battleSource, encFilePath);

            var bossEncounterIds = Encounters.Keys.ToList();
            var matchIdPool = Encounters.Keys.ToList();

            var encIdMap = new Dictionary<int, int>();

            foreach (var encId in bossEncounterIds)
            {
                // pick an encounter from the pool
                var matchedId = matchIdPool[random.Next(matchIdPool.Count)];
                matchIdPool.Remove(matchedId);
                encIdMap.Add(encId, matchedId);

                // copy the monster slots from the other encounter
                for (int i = 0; i < 8; i++)
                {
                    var monsterId = sourceFile.Encounters[matchedId].Slots[i].MonsterID;

                    newFile.Encounters[encId].Slots[i] = sourceFile.Encounters[matchedId].Slots[i];

                    if (rebalance)
                    {
                        // retain level
                        newFile.Encounters[encId].Slots[i].Level = sourceFile.Encounters[encId].Slots[i].Level;
                    }

                    // update any encounter ID checks in the monster's AI scripts
                    foreach (var ec in EncounterChecks.Where(ec => ec.MonsterID == monsterId && ec.EncounterID == matchedId))
                    {
                        var monster = sourceFile.Encounters[matchedId].Slots[i].GetMonster(battleSource);
                        var script = monster.AI.Scripts.EventScripts[ec.Script];
                        script[ec.Instruction].Args[3] = (short)encId;
                        battleSource.ReplaceFile(Monster.GetPath(monsterId), monster.Encode());
                    }
                }

                // force the nameless sorceresses to fight in the commencement room
                // so they can do the melty background thing without crashing the game
                if (matchedId == 813)
                {
                    var sorceressEncounter = sourceFile.Encounters[813];
                    newFile.Encounters[encId].Scene = sorceressEncounter.Scene;
                    newFile.Encounters[encId].MainCamera = sorceressEncounter.MainCamera;
                    newFile.Encounters[encId].MainCameraAnimation = sorceressEncounter.MainCameraAnimation;
                    newFile.Encounters[encId].SecondaryCamera = sorceressEncounter.SecondaryCamera;
                    newFile.Encounters[encId].SecondaryCameraAnimation = sorceressEncounter.SecondaryCameraAnimation;
                }

                // award diablos gf for defeating whoever is in the lamp to avoid softlock
                if (encId == 811)
                {
                    var bossSlot = Bosses.Find(b => b.EncounterID == matchedId).SlotRanks[0];
                    var monsterId = sourceFile.Encounters[matchedId].Slots[bossSlot].MonsterID;
                    var monster = Monster.ByID(battleSource, monsterId);
                    var script = monster.AI.Scripts.Init;

                    script.InsertRange(0, new List<Battle.Instruction>
                    {
                        // if shared-var-4 == 0
                        new Battle.Instruction(Battle.Instruction.OpCodesReverse["if"], new short[] { 0x64, 0xc8, 0x00, 0x00, 0x08 }),

                        // give diablos
                        new Battle.Instruction(Battle.Instruction.OpCodesReverse["award-gf"], new short[] { 0x05 }),

                        // shared-var-4 = 1
                        new Battle.Instruction(Battle.Instruction.OpCodesReverse["set-shared"], new short[] { 0x64, 0x01 }),

                        // end if
                        new Battle.Instruction(Battle.Instruction.OpCodesReverse["jmp"], new short[] { 0x00 })
                    }); ;

                    battleSource.ReplaceFile(Monster.GetPath(monsterId), monster.Encode());
                }

                // skip shot tutorial for iguion fight if irvine isn't present
                if (encId == 147)
                {
                    var monsterId = sourceFile.Encounters[encId].Slots[0].MonsterID;
                    var monster = Monster.ByID(battleSource, monsterId);
                    var script = monster.AI.Scripts.Init;

                    script.InsertRange(0, new List<Battle.Instruction>
                    {
                        // if irvine is not alive
                        new Battle.Instruction(Battle.Instruction.OpCodesReverse["if"], new short[] { 0x09, 0xc8, 0x03, 0x02, 0x06 }),

                        // shared-var-1 (dialogue flag) = 1
                        new Battle.Instruction(Battle.Instruction.OpCodesReverse["set-shared"], new short[] { 0x61, 0x01 }),

                        // end if
                        new Battle.Instruction(Battle.Instruction.OpCodesReverse["jmp"], new short[] { 0x00 })
                    });

                    battleSource.ReplaceFile(Monster.GetPath(monsterId), monster.Encode());
                }
            }

            // save new encounter file
            battleSource.ReplaceFile(encFilePath, newFile.Encode());

            return encIdMap;
        }

        public static void ApplyEdeaFix(FileSource battleSource, FileSource fieldSource)
        {
            // clone encounter
            var encFile = EncounterFile.FromSource(battleSource);
            encFile.Encounters[845] = encFile.Encounters[136];
            battleSource.ReplaceFile(EncounterFile.Path, encFile.Encode());

            // redirect field script to clone
            var fieldName = "glyagu1";
            var field = Field.FieldScript.FromSource(fieldSource, fieldName);
            var script = field.Entities[2].Scripts[4].Instructions;
            for (int i = 0; i < script.Count - 2; i++)
            {
                if (script[i].OpCode == Field.FieldScript.OpCodesReverse["pshn_l"] && script[i].Param == 136)
                {
                    if (script[i + 2].OpCode == Field.FieldScript.OpCodesReverse["battle"])
                    {
                        field.Entities[2].Scripts[4].Instructions[i].Param = 845;
                    }
                }
            }
            StorySkip.SaveToSource(fieldSource, fieldName, field.Encode());
        }
    }
}
