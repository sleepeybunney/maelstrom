using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8Mod.Maelstrom
{
    public class Boss
    {
        public static int[] Encounters = new int[]
        {
            28,     // x-atm092
            29,     // biggs + wedge + elvoret
            62,     // granaldo
            63,     // norg
            79,     // oilboyles
            83,     // raijin + soldiers
            84,     // raijin + fujin (balamb)
            86,     // red propagator
            94,     // ifrit
            104,    // gerogero
            118,    // cerberus
            119,    // seifer (g-garden)
            120,    // seifer + edea (g-garden)
            136,    // edea (parade) [crash warning]
            147,    // iguions
            151,    // biggs + wedge
            161,    // gim52a x2 + elite soldier
            164,    // bgh251f2 (missile base)
            189,    // sacred
            190,    // sacred + minotaur
            194,    // bgh251f2 (fh)
            195,    // seifer (parade)
            216,    // abadon
            // 236-238 centra tonberries with hidden tonberry king
            317,    // odin [instakill warning]
            326,    // bahamut
            354,    // ultima weapon
            363,    // sphinxaur
            372,    // krysta [finisher warning]
            377,    // tri-point
            410,    // catoblepas [finisher warning]
            431,    // trauma
            436,    // gargantua
            441,    // red giant
            462,    // omega weapon
            483,    // tiamat
            // 511 ultimecia
            712,    // jumbo cactuar
            794,    // mobile type 8
            795,    // seifer (lunatic pandora)
            796,    // adel + rinoa
            810,    // fujin + raijin (lunatic pandora)
            811,    // diablos
            813,    // sorceresses
            // 814-819 more propagators
        };

        public static void Shuffle(FileSource battleSource)
        {
            var encFilePath = @"c:\ff8\data\eng\battle\scene.out";
            var encFile = EncounterFile.FromSource(battleSource, encFilePath);
            var random = new Random();
            var encList = Encounters.ToList();
            var monsterMap = new Dictionary<int, List<EncounterSlot>>();

            foreach (var e in Encounters)
            {
                var matchedEncounter = encList[random.Next(encList.Count)];
                encList.Remove(matchedEncounter);
                var monsters = encFile.Encounters[matchedEncounter].Slots.ToList();
                monsterMap.Add(e, monsters);
            }

            foreach (var e in Encounters)
            {
                for (int i = 0; i < 8; i++)
                {
                    encFile.Encounters[e].Slots[i] = monsterMap[e][i];
                }
            }

            battleSource.ReplaceFile(encFilePath, encFile.Encoded);
        }
    }
}
