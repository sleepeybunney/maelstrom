using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FF8Mod.Maelstrom
{
    public static class DrawPointShuffle
    {
        public static List<DrawPoint> DrawPoints = JsonSerializer.Deserialize<List<DrawPoint>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.DrawPoints.json"));

        // assign random spells to each draw point, retaining their other properties
        public static Dictionary<int, int> Randomise(int seed)
        {
            var random = new Random(seed);
            var spells = (int[])Enum.GetValues(typeof(DrawPoint.Magic));
            var result = new Dictionary<int, int>();

            foreach (var dp in DrawPoints)
            {
                if (dp.Location != null)
                {
                    result[dp.Offset] = spells[random.Next(spells.Length)];
                }
            }

            return result;
        }

        public static BinaryPatch GeneratePatch(Dictionary<int, int> newSpells)
        {
            for (int i = 0; i < DrawPoint.OriginalData.Length; i++)
            {
                var drawPoint = new DrawPoint(DrawPoint.OriginalData[i]);
                if (newSpells.ContainsKey(i))
                {
                    drawPoint.Spell = (DrawPoint.Magic)newSpells[i];
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
