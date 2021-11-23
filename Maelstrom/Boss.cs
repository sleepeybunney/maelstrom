using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Battle;

namespace Sleepey.Maelstrom
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
        public List<int> SlotRanks { get; set; }
        public bool Disabled { get; set; } = false;
        public int Disc { get; set; }

        public static List<Boss> Bosses = JsonSerializer.Deserialize<List<Boss>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Bosses.json")).Where(b => !b.Disabled).ToList();
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
            new EncounterCheck(436, 39, 3, 2, 0),
            new EncounterCheck(436, 40, 3, 2, 0),
            new EncounterCheck(436, 54, 3, 2, 0),

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
            public int InstructionJP;

            public EncounterCheck(int encID, int monID, int script, int instruction, int instructionJP = -1)
            {
                EncounterID = encID;
                MonsterID = monID;
                Script = script;
                Instruction = instruction;
                InstructionJP = instructionJP;
            }
        }

        public static Dictionary<int, int> Randomise(int seed, State settings)
        {
            var random = new Random(seed + 2);
            var encounterIDs = Encounters.Keys.ToList();
            var encounterMap = new Dictionary<int, int>();
            var availableReplacements = Encounters.Keys.ToList();
            int replacementID;

            // only match tonberry king with other solo bosses
            var singlesOnly = Encounters.Values.Where(e => e.SlotRanks.Count == 1 && availableReplacements.Contains(e.EncounterID)).Select(e => e.EncounterID).ToList();
            if (settings.OmegaWeapon == "normal") singlesOnly.Remove(462);
            replacementID = singlesOnly[random.Next(singlesOnly.Count)];

            availableReplacements.Remove(replacementID);
            encounterIDs.Remove(236);
            encounterMap.Add(236, replacementID);
            encounterMap.Add(237, replacementID);
            encounterMap.Add(238, replacementID);

            foreach (var encID in encounterIDs)
            {
                var tailoredReplacements = new List<int>(availableReplacements);

                if (settings.OmegaWeapon == "normal")
                {
                    // only match omega weapon with itself
                    if (encID == 462) tailoredReplacements = new List<int> { 462 };
                    else tailoredReplacements.Remove(462);
                }
                else if (settings.OmegaWeapon == "afterdisc1")
                {
                    // don't replace bosses from disc 1 with omega weapon
                    if (Encounters[encID].Disc == 1) tailoredReplacements.Remove(462);
                }

                replacementID = tailoredReplacements[random.Next(tailoredReplacements.Count)];
                if (!settings.BossRandom) availableReplacements.Remove(replacementID);
                encounterMap.Add(encID, replacementID);

                // copy propagators to their twins
                if (encID == 814) encounterMap.Add(817, replacementID);
                if (encID == 816) encounterMap.Add(819, replacementID);

                // copy x-atm092 all over dollet
                if (encID == 28)
                {
                    encounterMap.Add(9, replacementID);
                    encounterMap.Add(10, replacementID);
                    encounterMap.Add(13, replacementID);
                    encounterMap.Add(26, replacementID);
                    encounterMap.Add(27, replacementID);
                }
            }

            return encounterMap;
        }

        public static void Apply(FileSource battleSource, Dictionary<int, int> encounterMap, bool rebalance)
        {
            var cleanEncFile = EncounterFile.FromSource(battleSource, Globals.EncounterFilePath);
            var newEncFile = EncounterFile.FromSource(battleSource, Globals.EncounterFilePath);
            var rebalancedStats = new Dictionary<int, MonsterInfo>();

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

                    // tonberry king
                    var tonberryIDs = new List<int>() { 236, 237, 238 };
                    if (tonberryIDs.Contains(matchedEncID))
                    {
                        if (i == 0)
                        {
                            // enable tonberry king
                            newEncFile.Encounters[encID].Slots[i].Enabled = true;
                            newEncFile.Encounters[encID].Slots[i].Hidden = false;
                            newEncFile.Encounters[encID].Slots[i].Unloaded = false;
                            newEncFile.Encounters[encID].Slots[i].Untargetable = false;
                        }
                        else
                        {
                            // disable everything else
                            newEncFile.Encounters[encID].Slots[i].Enabled = false;
                            newEncFile.Encounters[encID].Slots[i].Hidden = true;
                            newEncFile.Encounters[encID].Slots[i].Unloaded = true;
                            newEncFile.Encounters[encID].Slots[i].Untargetable = true;
                        }
                    }

                    if (tonberryIDs.Contains(encID))
                    {
                        // disable tonberry king's replacement, to be summoned by tonberry
                        newEncFile.Encounters[encID].Slots[i].Enabled = false;
                        break;
                    }
                }

                // apply rebalancing if enabled
                if (rebalance && Encounters.ContainsKey(encID))
                {
                    var sourceBossSlot = Encounters[encID].SlotRanks[0];
                    var sourceInfo = cleanEncFile.Encounters[encID].Slots[sourceBossSlot].GetMonster(battleSource).Info;

                    var destBossSlot = Encounters[matchedEncID].SlotRanks[0];
                    var destID = cleanEncFile.Encounters[matchedEncID].Slots[destBossSlot].MonsterID;
                    var destInfo = Monster.ByID(battleSource, destID).Info;

                    var newInfo = new MonsterInfo();
                    newInfo.CopyStatsFrom(sourceInfo);

                    // flip physical & magic stats where appropriate
                    var sourcePrefersStr = sourceInfo.StrAtLevel(100) > sourceInfo.MagAtLevel(100);
                    var destPrefersStr = destInfo.StrAtLevel(100) > destInfo.MagAtLevel(100);

                    if (sourcePrefersStr ^ destPrefersStr)
                    {
                        newInfo.Str = sourceInfo.Mag.ToList();
                        newInfo.Mag = sourceInfo.Str.ToList();
                    }

                    var sourcePrefersVit = sourceInfo.VitAtLevel(100) > sourceInfo.SprAtLevel(100);
                    var destPrefersVit = destInfo.VitAtLevel(100) > destInfo.SprAtLevel(100);

                    if (sourcePrefersVit ^ destPrefersVit)
                    {
                        newInfo.Vit = sourceInfo.Spr.ToList();
                        newInfo.Spr = sourceInfo.Vit.ToList();
                    }

                    // if a boss appears multiple times, keep the weakest version to avoid difficulty spikes
                    if (!rebalancedStats.ContainsKey(destID) || rebalancedStats[destID].HpAtLevel(100) > newInfo.HpAtLevel(100))
                    {
                        rebalancedStats[destID] = newInfo;
                    }
                }

                // move tonberry king's death flag to allow the replacement boss to spawn
                if (encID == 236) MoveTonberryFlag(battleSource, newEncFile, matchedEncID);

                // award cactuar GF for beating jumbo's replacement to avoid despawning the cactus early
                if (encID == 712) MoveGF(battleSource, newEncFile, 84, matchedEncID, 712, 13);

                // award diablos GF for beating whoever is in the lamp to avoid softlock
                if (encID == 811) MoveGF(battleSource, newEncFile, 66, matchedEncID, 811, 5);

                // force the sorceresses to fight in their normal scene to avoid crash
                if (matchedEncID == 813) FixSorceresses(cleanEncFile, newEncFile, encID);
            }

            // skip shot tutorial for iguion fight if irvine isn't present
            FixIguions(battleSource, cleanEncFile);

            // remove odin's instakill attack
            FixOdin(battleSource, cleanEncFile);

            // save changes
            battleSource.ReplaceFile(Globals.EncounterFilePath, newEncFile.Encode());

            foreach (var monsterID in rebalancedStats.Keys)
            {
                var monster = Monster.ByID(battleSource, monsterID);
                monster.Info.CopyStatsFrom(rebalancedStats[monsterID]);
                battleSource.ReplaceFile(Monster.GetPath(monsterID), monster.Encode());
            }
        }

        private static void FixEncounterChecks(FileSource battleSource, int monsterID, int encID, int origEncID)
        {
            foreach (var ec in EncounterChecks.Where(ec => ec.MonsterID == monsterID && ec.EncounterID == origEncID))
            {
                var monster = Monster.ByID(battleSource, monsterID);
                var script = monster.AI.Scripts.EventScripts[ec.Script];
                var instruction = (ec.InstructionJP == -1 || Globals.RegionCode != "jp") ? ec.Instruction : ec.InstructionJP;
                script[instruction].Args[3] = (short)encID;
                battleSource.ReplaceFile(Monster.GetPath(monsterID), monster.Encode());
            }
        }

        private static void MoveGF(FileSource battleSource, EncounterFile newEncFile, int sourceMonsterID, int origEncounterID, int targetEncounterID, short gfID)
        {
            var bossSlot = Bosses.Find(b => b.EncounterID == origEncounterID).SlotRanks[0];
            var targetMonsterID = newEncFile.Encounters[targetEncounterID].Slots[bossSlot].MonsterID;

            if (sourceMonsterID == targetMonsterID) return;

            // remove GF unlock from source (replace with another 2-byte op to avoid breaking jumps)
            var source = Monster.ByID(battleSource, sourceMonsterID);
            var sourceDeath = source.AI.Scripts.Death;
            for (int i = 0; i < sourceDeath.Count; i++)
            {
                if (sourceDeath[i].Op == BattleScriptInstruction.OpCodesReverse["award-gf"])
                {
                    sourceDeath[i] = new BattleScriptInstruction("wait-all", 0x00);
                }
            }

            // add GF unlock to target
            var target = Monster.ByID(battleSource, targetMonsterID);
            target.AI.Scripts.Init.InsertRange(0, new List<BattleScriptInstruction>
            {
                // if boss encounter
                new BattleScriptInstruction("if", 0x03, 0x00, 0x00, (short)targetEncounterID, 0x13),

                // if shared-var-4 == 0
                new BattleScriptInstruction("if", 0x64, 0xc8, 0x00, 0x00, 0x08),

                // give gf
                new BattleScriptInstruction("award-gf", gfID),

                // shared-var-4 = 1
                new BattleScriptInstruction("set-shared", 0x64, 0x01),

                // end if
                new BattleScriptInstruction("jmp", 0x00),

                // end if
                new BattleScriptInstruction("jmp", 0x00)
            });

            battleSource.ReplaceFile(Monster.GetPath(sourceMonsterID), source.Encode());
            battleSource.ReplaceFile(Monster.GetPath(targetMonsterID), target.Encode());
        }

        private static void MoveTonberryFlag(FileSource battleSource, EncounterFile newEncFile, int targetEncounterID)
        {
            var sourceMonsterID = 83;
            var targetMonsterID = newEncFile.Encounters[targetEncounterID].Slots[0].MonsterID;
            if (sourceMonsterID == targetMonsterID) return;

            // remove death flag from tonberry king (replace with another 3-byte op to avoid breaking jumps)
            var source = Monster.ByID(battleSource, 83);
            var sourceDeath = source.AI.Scripts.Death;
            for (int i = 0; i < sourceDeath.Count; i++)
            {
                if (sourceDeath[i].Op == BattleScriptInstruction.OpCodesReverse["set-global"] && sourceDeath[i].Args[0] == 82)
                {
                    sourceDeath[i] = new BattleScriptInstruction("set", 222, 1);
                }
            }

            // add death flag to replacement
            var target = Monster.ByID(battleSource, targetMonsterID);
            target.AI.Scripts.Death.Insert(0, new BattleScriptInstruction("set-global", 82, 1));

            battleSource.ReplaceFile(Monster.GetPath(sourceMonsterID), source.Encode());
            battleSource.ReplaceFile(Monster.GetPath(targetMonsterID), target.Encode());
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
            script.Insert(0, new BattleScriptInstruction("return"));
            battleSource.ReplaceFile(Monster.GetPath(monsterId), monster.Encode());
        }

        private static void FixIguions(FileSource battleSource, EncounterFile cleanEncFile)
        {
            var monsterId = cleanEncFile.Encounters[147].Slots[0].MonsterID;
            var monster = Monster.ByID(battleSource, monsterId);
            var script = monster.AI.Scripts.Init;

            script.InsertRange(0, new List<BattleScriptInstruction>
            {
                // if irvine is not alive
                new BattleScriptInstruction("if", 0x09, 0xc8, 0x03, 0x02, 0x06),

                // shared-var-1 (dialogue flag) = 1
                new BattleScriptInstruction("set-shared", 0x61, 0x01),

                // end if
                new BattleScriptInstruction("jmp", 0x00)
            });

            battleSource.ReplaceFile(Monster.GetPath(monsterId), monster.Encode());
        }

        public static void ApplyEdeaFix(FileSource battleSource, FileSource fieldSource)
        {
            // clone encounter
            var encFile = EncounterFile.FromSource(battleSource);
            encFile.Encounters[845] = encFile.Encounters[136];
            battleSource.ReplaceFile(Globals.EncounterFilePath, encFile.Encode());

            // redirect field script to clone
            var fieldName = "glyagu1";
            var field = FF8Mod.Field.FieldScript.FromSource(fieldSource, fieldName);
            var script = field.Entities[2].Scripts[4].Instructions;
            for (int i = 0; i < script.Count - 2; i++)
            {
                if (script[i].OpCode == FF8Mod.Field.FieldScript.OpCodesReverse["pshn_l"] && script[i].Param == 136)
                {
                    if (script[i + 2].OpCode == FF8Mod.Field.FieldScript.OpCodesReverse["battle"])
                    {
                        field.Entities[2].Scripts[4].Instructions[i].Param = 845;
                    }
                }
            }

            field.SaveToSource(fieldSource, fieldName);
        }
    }
}
