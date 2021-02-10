﻿using System;
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

        public static Dictionary<int, int> Randomise(int seed, State settings)
        {
            var random = new Random(seed);
            var encounterIDs = Encounters.Keys.ToList();
            var encounterMap = new Dictionary<int, int>();

            if (settings.BossRandom)
            {
                foreach (var encID in encounterIDs)
                {
                    encounterMap.Add(encID, encounterIDs[random.Next(encounterIDs.Count)]);
                }
            }
            else
            {
                var unmatchedIDs = Encounters.Keys.ToList();
                foreach (var encID in encounterIDs)
                {
                    var matchedID = unmatchedIDs[random.Next(unmatchedIDs.Count)];
                    unmatchedIDs.Remove(matchedID);
                    encounterMap.Add(encID, matchedID);
                }
            }

            return encounterMap;
        }

        public static void Apply(FileSource battleSource, Dictionary<int, int> encounterMap)
        {
            var cleanEncFile = EncounterFile.FromSource(battleSource, EncounterFile.Path);
            var newEncFile = EncounterFile.FromSource(battleSource, EncounterFile.Path);

            foreach (var encID in encounterMap.Keys)
            {
                var matchedEncID = encounterMap[encID];

                // copy monster slots from the incoming encounter
                for (int i = 0; i < 8; i++)
                {
                    var newMonsterID = cleanEncFile.Encounters[matchedEncID].Slots[i].MonsterID;
                    newEncFile.Encounters[encID].Slots[i] = cleanEncFile.Encounters[matchedEncID].Slots[i];

                    // update any encounter ID checks in the monster's AI scripts
                    FixEncounterChecks(battleSource, newMonsterID, encID, matchedEncID);
                }

                // award diablos GF for beating whoever is in the lamp to avoid softlock
                if (encID == 811) FixDiablos(battleSource, newEncFile, matchedEncID);

                // force the sorceresses to fight in their normal scene to avoid crash
                if (matchedEncID == 813) FixSorceresses(cleanEncFile, newEncFile, encID);
            }

            // skip shot tutorial for iguion fight if irvine isn't present
            FixIguions(battleSource, cleanEncFile);

            // remove odin's instakill attack
            FixOdin(battleSource, cleanEncFile);

            // save changes
            battleSource.ReplaceFile(EncounterFile.Path, newEncFile.Encode());
        }

        private static void FixEncounterChecks(FileSource battleSource, int monsterID, int encID, int origEncID)
        {
            foreach (var ec in EncounterChecks.Where(ec => ec.MonsterID == monsterID && ec.EncounterID == origEncID))
            {
                var monster = Monster.ByID(battleSource, monsterID);
                var script = monster.AI.Scripts.EventScripts[ec.Script];
                script[ec.Instruction].Args[3] = (short)encID;
                battleSource.ReplaceFile(Monster.GetPath(monsterID), monster.Encode());
            }
        }

        private static void FixDiablos(FileSource battleSource, EncounterFile newEncFile, int origEncID)
        {
            // find main boss monster of the encounter replacing diablos
            var bossSlot = Bosses.Find(b => b.EncounterID == origEncID).SlotRanks[0];
            var monsterId = newEncFile.Encounters[811].Slots[bossSlot].MonsterID;
            var monster = Monster.ByID(battleSource, monsterId);

            // add GF unlock to monster's init script
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

        private static void FixSorceresses(EncounterFile cleanEncFile, EncounterFile newEncFile, int encID)
        {
            // copy sorceress
            var sorceressEncounter = cleanEncFile.Encounters[813];
            newEncFile.Encounters[encID].Scene = sorceressEncounter.Scene;
            newEncFile.Encounters[encID].MainCamera = sorceressEncounter.MainCamera;
            newEncFile.Encounters[encID].MainCameraAnimation = sorceressEncounter.MainCameraAnimation;
            newEncFile.Encounters[encID].SecondaryCamera = sorceressEncounter.SecondaryCamera;
            newEncFile.Encounters[encID].SecondaryCameraAnimation = sorceressEncounter.SecondaryCameraAnimation;
        }

        private static void FixOdin(FileSource battleSource, EncounterFile cleanEncFile)
        {
            var monsterId = cleanEncFile.Encounters[317].Slots[0].MonsterID;
            var monster = Monster.ByID(battleSource, monsterId);
            var script = monster.AI.Scripts.Execute;
            script.Insert(0, new Battle.Instruction(Battle.Instruction.OpCodesReverse["return"]));
            battleSource.ReplaceFile(Monster.GetPath(monsterId), monster.Encode());
        }

        private static void FixIguions(FileSource battleSource, EncounterFile cleanEncFile)
        {
            var monsterId = cleanEncFile.Encounters[147].Slots[0].MonsterID;
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
