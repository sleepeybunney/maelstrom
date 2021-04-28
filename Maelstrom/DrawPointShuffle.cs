using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Exe;

namespace Sleepey.Maelstrom
{
    public static class DrawPointShuffle
    {
        public static List<DrawPoint> DrawPoints = JsonSerializer.Deserialize<List<DrawPoint>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.DrawPoints.json"));
        public static List<Spell> Spells = JsonSerializer.Deserialize<List<Spell>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Spells.json"));

        // assign random spells to each draw point, retaining their other properties
        public static Dictionary<int, int> Randomise(int seed, State settings)
        {
            var random = new Random(seed + 4);
            var spellIDs = Spells
                .Where(s => s.SpellID != 20 || settings.DrawPointIncludeApoc)
                .Where(s => !s.SlotExclusive || settings.DrawPointIncludeSlot)
                .Where(s => !s.CutContent || settings.DrawPointIncludeCut)
                .Select(s => s.SpellID).ToArray();
            var result = new Dictionary<int, int>();

            foreach (var dp in DrawPoints)
            {
                if (dp.Location != null)
                {
                    result[dp.Offset] = spellIDs[random.Next(spellIDs.Length)];
                }
            }

            return result;
        }

        public static void Apply(FileSource menuSource, Dictionary<int, int> newSpells)
        {
            GeneratePatch(newSpells).Apply(Globals.ExePath);
        }

        public static BinaryPatch GeneratePatch(Dictionary<int, int> newSpells)
        {
            for (int i = 0; i < DrawPoint.OriginalData.Length; i++)
            {
                var drawPoint = new DrawPoint(DrawPoint.OriginalData[i]);
                if (newSpells.ContainsKey(i))
                {
                    drawPoint.SpellID = newSpells[i];
                }
                DrawPoint.UpdatedDrawPoints[i + 1] = drawPoint;
            }

            return new BinaryPatch(DrawPoint.DrawPointDefsLocation, DrawPoint.OriginalData, DrawPoint.EncodeAll());
        }

        public static void RemovePatch(string gameLocation)
        {
            new BinaryPatch(DrawPoint.DrawPointDefsLocation, DrawPoint.OriginalData, DrawPoint.OriginalData).Remove(gameLocation);
        }
    }
}
