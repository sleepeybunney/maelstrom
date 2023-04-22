using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.Maelstrom
{
    public static class Tweaks
    {
        public static void Apply(FileSource battleSource, State settings)
        {
            if (!settings.StaticSorcs || settings.StaticOmega)
            {
                var encounterFile = EncounterFile.FromSource(battleSource);

                // enable level scaling for sorceresses
                if (!settings.StaticSorcs)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        encounterFile.Encounters[813].Slots[i].Level = 254;
                    }
                }

                // disable level scaling for omega weapon
                if (settings.StaticOmega)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        encounterFile.Encounters[462].Slots[i].Level = 100;
                    }
                }

                battleSource.ReplaceFile(Env.EncounterFilePath, encounterFile.Encode());
            }

            if (settings.TonberryKillsMin != 20 || settings.TonberryKillsMax != 50)
            {
                var tonberry = Monster.ByID(battleSource, 56);
                var minKills = Math.Max(1, Math.Min(settings.TonberryKillsMin, 255));
                var maxKills = Math.Max(minKills, Math.Min(settings.TonberryKillsMax, 255));

                // set minimum kills for tonberry king
                if (settings.TonberryKillsMin != 20)
                {
                    var minCheckIndex = tonberry.AI.Scripts.Death.FindIndex(i => i.Op == BattleScriptInstruction.OpCodesReverse["if"] && i.Args[0] == 0x51 && i.Args[3] == 20);
                    tonberry.AI.Scripts.Death[minCheckIndex].Args[3] = (short)minKills;
                }

                // set maximum kills for tonberry king
                if (settings.TonberryKillsMax != 50)
                {
                    var maxCheckIndex = tonberry.AI.Scripts.Death.FindIndex(i => i.Op == BattleScriptInstruction.OpCodesReverse["if"] && i.Args[0] == 0x51 && i.Args[3] == 50);
                    tonberry.AI.Scripts.Death[maxCheckIndex].Args[3] = (short)maxKills;
                }

                battleSource.ReplaceFile(Monster.GetPath(56), tonberry.Encode());
            }
        }
    }
}
