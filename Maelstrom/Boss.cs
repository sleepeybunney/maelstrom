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
            new EncounterCheck(164, 71, 0),
            new EncounterCheck(164, 72, 0),
            new EncounterCheck(164, 72, 1),

            // gargantua
            new EncounterCheck(436, 39, 3),
            new EncounterCheck(436, 40, 3),
            new EncounterCheck(436, 54, 3),

            // sacred
            new EncounterCheck(189, 63, 3),
            new EncounterCheck(189, 63, 1),

            // granaldo
            new EncounterCheck(62, 95, 1),
            new EncounterCheck(62, 96, 1),

            // gim52a
            new EncounterCheck(161, 72, 0),

            // raijin & soldiers
            new EncounterCheck(83, 120, 0),
            new EncounterCheck(83, 120, 1),
            new EncounterCheck(83, 120, 3),

            // raijin & fujin
            new EncounterCheck(84, 120, 2),
        };

        private struct EncounterCheck
        {
            public int EncounterID;
            public int MonsterID;
            public int Script;

            public EncounterCheck(int encID, int monID, int script)
            {
                EncounterID = encID;
                MonsterID = monID;
                Script = script;
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

            if (settings.RestrictOmega == "normal")
            {
                singlesOnly.Remove(462);
            }

            if (settings.RestrictUltimecia == "normal")
            {
                singlesOnly.Remove(846);
                singlesOnly.Remove(847);
            }

            replacementID = singlesOnly[random.Next(singlesOnly.Count)];

            if (!settings.BossRandom) availableReplacements.Remove(replacementID);
            encounterIDs.Remove(236);
            encounterMap.Add(236, replacementID);
            encounterMap.Add(237, replacementID);
            encounterMap.Add(238, replacementID);

            foreach (var encID in encounterIDs)
            {
                var tailoredReplacements = new List<int>(availableReplacements);

                if (settings.RestrictSorcs == "normal")
                {
                    // only match sorceress fight with itself
                    if (encID == 813) tailoredReplacements = new List<int> { 813 };
                    else tailoredReplacements.Remove(813);
                }
                else if (settings.RestrictSorcs == "afterdisc1")
                {
                    // don't replace bosses from disc 1 with sorceresses
                    if (Encounters[encID].Disc == 1) tailoredReplacements.Remove(813);
                }

                if (settings.RestrictOmega == "normal")
                {
                    // only match omega weapon with itself
                    if (encID == 462) tailoredReplacements = new List<int> { 462 };
                    else tailoredReplacements.Remove(462);
                }
                else if (settings.RestrictOmega == "afterdisc1")
                {
                    // don't replace bosses from disc 1 with omega weapon
                    if (Encounters[encID].Disc == 1) tailoredReplacements.Remove(462);
                }

                if (settings.RestrictUltimecia == "normal")
                {
                    // only match final boss phases with themselves
                    if (new int[] { 846, 847, 848 }.Contains(encID)) tailoredReplacements = new List<int> { encID };
                    else tailoredReplacements.RemoveAll(r => new int[] { 846, 847, 848 }.Contains(r));
                }
                else if (settings.RestrictUltimecia == "afterdisc1")
                {
                    // don't replace bosses from disc 1 with final boss phases
                    if (Encounters[encID].Disc == 1) tailoredReplacements.RemoveAll(r => new int[] { 846, 847, 848 }.Contains(r));
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

        public static void Apply(FileSource battleSource, Dictionary<int, int> encounterMap, State settings, bool splitFinalBoss)
        {
            if (splitFinalBoss) SplitFinalBoss(battleSource);

            var cleanEncFile = EncounterFile.FromSource(battleSource, Env.EncounterFilePath);
            var newEncFile = EncounterFile.FromSource(battleSource, Env.EncounterFilePath);
            var rebalancedStats = new Dictionary<int, MonsterInfo>();

            foreach (var encID in encounterMap.Keys)
            {
                var matchedEncID = encounterMap[encID];

                // copy monster slots from the incoming encounter
                for (int i = 0; i < 8; i++)
                {
                    var newMonsterID = cleanEncFile.Encounters[matchedEncID].Slots[i].MonsterID;
                    newEncFile.Encounters[encID].Slots[i] = new Encounter(cleanEncFile.Encounters[matchedEncID].Encode()).Slots[i];

                    // enable level scaling for sorceresses
                    if (matchedEncID == 813 && !settings.StaticSorcs)
                    {
                        newEncFile.Encounters[encID].Slots[i].Level = 254;
                    }

                    // disable level scaling for omega weapon
                    if (matchedEncID == 462 && settings.StaticOmega)
                    {
                        newEncFile.Encounters[encID].Slots[i].Level = 100;
                    }

                    // update any encounter ID checks in the monster's AI scripts
                    foreach (var ec in EncounterChecks.Where(ec => ec.MonsterID == newMonsterID && ec.EncounterID == matchedEncID))
                    {
                        FixEncounterCheck(battleSource, ec, encID);
                    }

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
                if (settings.BossRebalance && Encounters.ContainsKey(encID))
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

                    // norg
                    if (matchedEncID == 63 && Encounters[encID].Disc == 1)
                    {
                        // weaken norg pod on disc 1
                        var podInfo = Monster.ByID(battleSource, 68).Info;
                        var newPodInfo = new MonsterInfo();
                        newPodInfo.CopyStatsFrom(podInfo);

                        // 800 hp
                        newPodInfo.Hp[1] = 80;
                        newPodInfo.Hp[3] = 0;

                        // half(ish) vit & spr
                        newPodInfo.Vit[2] /= 2;
                        newPodInfo.Spr[2] /= 2;

                        rebalancedStats[68] = newPodInfo;
                    }

                    // fujin & raijin (1st)
                    if (matchedEncID == 84)
                    {
                        // copy raijin's new stats to fujin
                        var newFujinInfo = new MonsterInfo();
                        newFujinInfo.CopyStatsFrom(newInfo);

                        // swap phys/mag
                        newFujinInfo.Str = newInfo.Mag.ToList();
                        newFujinInfo.Mag = newInfo.Str.ToList();
                        newFujinInfo.Vit = newInfo.Spr.ToList();
                        newFujinInfo.Spr = newInfo.Vit.ToList();

                        rebalancedStats[119] = newFujinInfo;
                    }

                    // edea (2nd)
                    if (matchedEncID == 120)
                    {
                        // copy edea's new stats to seifer
                        var newSeiferInfo = new MonsterInfo();
                        newSeiferInfo.CopyStatsFrom(newInfo);

                        // copy mag to phys
                        newSeiferInfo.Str = newInfo.Mag.ToList();
                        newSeiferInfo.Vit = newInfo.Spr.ToList();

                        // reduce hp
                        if (newSeiferInfo.Hp[2] > 1) newSeiferInfo.Hp[2] /= 2;

                        rebalancedStats[87] = newSeiferInfo;
                    }

                    // sacred & minotaur
                    if (matchedEncID == 190)
                    {
                        // copy sacred's new stats to minotaur
                        var newMinotaurInfo = new MonsterInfo();
                        newMinotaurInfo.CopyStatsFrom(newInfo);

                        // raise hp & attack stats
                        if (newMinotaurInfo.Hp[0] < 80) newMinotaurInfo.Hp[0] = (byte)(newMinotaurInfo.Hp[0] * 1.2);
                        if (newMinotaurInfo.Str[0] < 160) newMinotaurInfo.Str[0] = (byte)(newMinotaurInfo.Str[0] * 1.2);
                        if (newMinotaurInfo.Mag[0] < 160) newMinotaurInfo.Mag[0] = (byte)(newMinotaurInfo.Mag[0] * 1.2);

                        rebalancedStats[62] = newMinotaurInfo;
                    }

                    // sphinxara
                    if (matchedEncID == 363)
                    {
                        // copy sphinxara's new stats to sphinxaur
                        var newSphinxaurInfo = new MonsterInfo();
                        newSphinxaurInfo.CopyStatsFrom(newInfo);
                        rebalancedStats[11] = newSphinxaurInfo;
                    }

                    // fujin & raijin (2nd)
                    if (matchedEncID == 810)
                    {
                        // copy raijin's new stats to fujin
                        var newFujinInfo = new MonsterInfo();
                        newFujinInfo.CopyStatsFrom(newInfo);

                        // swap phys/mag
                        newFujinInfo.Str = newInfo.Mag.ToList();
                        newFujinInfo.Mag = newInfo.Str.ToList();
                        newFujinInfo.Vit = newInfo.Spr.ToList();
                        newFujinInfo.Spr = newInfo.Vit.ToList();

                        rebalancedStats[136] = newFujinInfo;
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
            battleSource.ReplaceFile(Env.EncounterFilePath, newEncFile.Encode());

            foreach (var monsterID in rebalancedStats.Keys)
            {
                var monster = Monster.ByID(battleSource, monsterID);
                monster.Info.CopyStatsFrom(rebalancedStats[monsterID]);
                battleSource.ReplaceFile(Monster.GetPath(monsterID), monster.Encode());
            }
        }

        private static void SplitFinalBoss(FileSource battleSource)
        {
            var encFile = EncounterFile.FromSource(battleSource);

            // clone first phase to a new encounter
            var phase1 = new Encounter(encFile.Encounters[511].Encode());
            for (int i = 1; i < phase1.Slots.Count; i++)
            {
                phase1.Slots[i].Enabled = false;
                phase1.Slots[i].Unloaded = true;
            }
            encFile.Encounters[846] = phase1;

            // clone second phase to a new encounter
            var phase2 = new Encounter(encFile.Encounters[511].Encode());
            phase2.Slots[0] = new Encounter(encFile.Encounters[511].Encode()).Slots[2];
            phase2.Slots[0].Enabled = true;
            phase2.Slots[0].Hidden = false;
            phase2.Slots[0].Untargetable = false;
            phase2.Slots[0].Unloaded = false;
            for (int i = 1; i < phase2.Slots.Count; i++)
            {
                phase2.Slots[i].Enabled = false;
                phase2.Slots[i].Unloaded = true;
            }
            phase2.Scene = 37;
            encFile.Encounters[847] = phase2;

            // clone third phase to a new encounter
            var phase3 = new Encounter(encFile.Encounters[511].Encode());
            phase3.Slots[0].Enabled = false;
            phase3.Slots[0].Unloaded = true;
            phase3.Slots[1].Enabled = true;
            phase3.Slots[1].Unloaded = false;
            phase3.Slots[3].Enabled = true;
            for (int i = 4; i < phase3.Slots.Count; i++)
            {
                phase3.Slots[i].Enabled = false;
                phase3.Slots[i].Unloaded = true;
            }
            phase3.Scene = 37;
            encFile.Encounters[848] = phase3;

            // remove transition to phase 2 from ultimecia's death script
            var ultimecia = phase1.Slots[0].GetMonster(battleSource);
            ultimecia.AI.Scripts.Death.RemoveAll(i => i.Op != BattleScriptInstruction.OpCodesReverse["return"]);
            ultimecia.AI.Scripts.Death.Insert(0, new BattleScriptInstruction("die"));
            battleSource.ReplaceFile(Monster.GetPath(phase1.Slots[0].MonsterID), ultimecia.Encode());

            // remove phase transitions from griever's init & death scripts
            var griever = phase2.Slots[0].GetMonster(battleSource);
            griever.AI.Scripts.Init.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["target"]);
            griever.AI.Scripts.Init.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["use"]);
            var deathCheckIndex = griever.AI.Scripts.Death.FindIndex(i => i.Op == BattleScriptInstruction.OpCodesReverse["if"] && i.Args[0] == 0x06);
            griever.AI.Scripts.Death.RemoveRange(deathCheckIndex + 1, 7);
            griever.AI.Scripts.Death.Insert(deathCheckIndex + 1, new BattleScriptInstruction("die"));
            griever.AI.Scripts.Death[deathCheckIndex].Args[4] = 4;
            battleSource.ReplaceFile(Monster.GetPath(phase2.Slots[0].MonsterID), griever.Encode());

            // remove phase transition from grievermecia's death script
            var grievermecia = phase3.Slots[3].GetMonster(battleSource);
            grievermecia.AI.Scripts.Death.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["unknown-33"]);
            grievermecia.AI.Scripts.Death.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["friend-add-at"]);
            var firstIfIndex = grievermecia.AI.Scripts.Death.FindIndex(i => i.Op == BattleScriptInstruction.OpCodesReverse["if"]);
            grievermecia.AI.Scripts.Death[firstIfIndex].Args[4] -= 4;
            battleSource.ReplaceFile(Monster.GetPath(phase3.Slots[3].MonsterID), grievermecia.Encode());

            // remove phase transitions from the final form's init & death scripts
            var ultiFinal = encFile.Encounters[511].Slots[6].GetMonster(battleSource);
            ultiFinal.AI.Scripts.Init.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["target"]);
            ultiFinal.AI.Scripts.Init.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["use"]);
            ultiFinal.AI.Scripts.Death.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["target"]);
            ultiFinal.AI.Scripts.Death.RemoveAll(i => i.Op == BattleScriptInstruction.OpCodesReverse["use"]);
            var firstRemoveIndex = ultiFinal.AI.Scripts.Death.FindIndex(i => i.Op == BattleScriptInstruction.OpCodesReverse["friend-remove"]);
            ultiFinal.AI.Scripts.Death.Insert(firstRemoveIndex + 2, new BattleScriptInstruction("die"));
            ultiFinal.AI.Scripts.Death[0].Args[4] -= 3;
            ultiFinal.AI.Scripts.Death[1].Args[4] -= 3;
            battleSource.ReplaceFile(Monster.GetPath(encFile.Encounters[511].Slots[6].MonsterID), ultiFinal.Encode());

            // start actual final boss on phase 4
            encFile.Encounters[511].Slots[5] = new Encounter(encFile.Encounters[511].Encode()).Slots[6];
            encFile.Encounters[511].Scene = 38;
            encFile.Encounters[511].Slots[0].Enabled = false;
            encFile.Encounters[511].Slots[1].Enabled = false;
            encFile.Encounters[511].Slots[5].Enabled = true;
            encFile.Encounters[511].Slots[5].Hidden = false;

            // save changes to encounter file
            battleSource.ReplaceFile(Env.EncounterFilePath, encFile.Encode());
        }

        private static void FixEncounterCheck(FileSource battleSource, EncounterCheck encCheck, int newEncID)
        {
            var monster = Monster.ByID(battleSource, encCheck.MonsterID);
            var script = monster.AI.Scripts.EventScripts[encCheck.Script];
            for (int i = 0; i < script.Count; i++)
            {
                // find encounter ID checks ("if encounter-id == origEncID") & update the ID
                if (script[i].Op == BattleScriptInstruction.OpCodesReverse["if"] && script[i].Args[0] == 3 && script[i].Args[3] == encCheck.EncounterID)
                {
                    script[i].Args[3] = (short)newEncID;
                }
            }
            battleSource.ReplaceFile(Monster.GetPath(encCheck.MonsterID), monster.Encode());
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
            var flagScript = new List<BattleScriptInstruction>()
            {
                // if encounter == tonberry king 1
                new BattleScriptInstruction("if", 0x03, 0x00, 0x00, 0xec, 0x06),

                // set flag
                new BattleScriptInstruction("set-global", 82, 1),

                // end if
                new BattleScriptInstruction("jmp", 0x00),

                // if encounter == tonberry king 2
                new BattleScriptInstruction("if", 0x03, 0x00, 0x00, 0xed, 0x06),

                // set flag
                new BattleScriptInstruction("set-global", 82, 1),

                // end if
                new BattleScriptInstruction("jmp", 0x00)
            };

            // if this monster didn't have a death script before then it needs to be told to die
            var scriptIsEmpty = !target.AI.Scripts.Death.Exists(i => i.Op != BattleScriptInstruction.OpCodesReverse["return"]);
            if (scriptIsEmpty) flagScript.Add(new BattleScriptInstruction("die"));

            // save changes
            target.AI.Scripts.Death.InsertRange(0, flagScript);
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

        public static void UpdateFinalBossChamber(FileSource fieldSource)
        {
            var fieldName = "felast1";
            var field = FF8Mod.Field.FieldScript.FromSource(fieldSource, fieldName);
            field.ReplaceScript(10, 1, App.ReadEmbeddedFile(@"Sleepey.Maelstrom.FieldScripts.felast1.10.1.txt"));
            field.SaveToSource(fieldSource, fieldName);
        }

        public static void ApplyEdeaFix(FileSource battleSource, FileSource fieldSource)
        {
            // clone encounter
            var encFile = EncounterFile.FromSource(battleSource);
            encFile.Encounters[845] = encFile.Encounters[136];
            battleSource.ReplaceFile(Env.EncounterFilePath, encFile.Encode());

            // find any encounter checks that need updating again
            var monsters = encFile.Encounters[845].Slots.Select(s => s.MonsterID).ToHashSet();
            var encChecks = EncounterChecks.Where(ec => monsters.Contains((byte)ec.MonsterID));
            foreach (var ec in encChecks)
            {
                var newCheck = new EncounterCheck(136, ec.MonsterID, ec.Script);
                FixEncounterCheck(battleSource, newCheck, 845);
            }

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
