using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public static class DrawPointShuffle
    {
        public static List<DrawPoint> DrawPoints = JsonSerializer.Deserialize<List<DrawPoint>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.DrawPoints.json"));
        public static List<Spell> Spells = JsonSerializer.Deserialize<List<Spell>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.Spells.json"));

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
            menuSource.ReplaceFile(Globals.DataPath + @"\menu\magsort.bin", GenerateSort());
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

        public static byte[] GenerateSort()
        {
            var attack = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x36, 0x11, 0x12, 0x0E, 0x0F, 0x10, 0x13, 0x14, 0x37 };
            var restore = new byte[] { 0x15, 0x16, 0x17, 0x33, 0x18, 0x19, 0x1A, 0x1B };
            var indirect = new byte[] { 0x32, 0x28, 0x26, 0x29, 0x27, 0x2E, 0x2A, 0x30, 0x2B, 0x21, 0x22, 0x1C, 0x1D, 0x1E, 0x34, 0x1F, 0x2F, 0x2C, 0x23, 0x24, 0x25, 0x31, 0x2D, 0x20, 0x35, 0x38 };

            var blockSize = 64;
            var spellCount = attack.Length + restore.Length + indirect.Length;

            var padding = new List<byte>();
            for (int i = spellCount; i < blockSize; i++) padding.Add(0);

            var manual = new List<byte>();
            for (int i = 0; i < blockSize; i++) manual.Add(0);

            var result = new List<byte>();
            result.AddRange(manual);

            result.AddRange(attack);
            result.AddRange(restore);
            result.AddRange(indirect);
            result.AddRange(padding);

            result.AddRange(attack);
            result.AddRange(indirect);
            result.AddRange(restore);
            result.AddRange(padding);

            result.AddRange(restore);
            result.AddRange(attack);
            result.AddRange(indirect);
            result.AddRange(padding);

            result.AddRange(restore);
            result.AddRange(indirect);
            result.AddRange(attack);
            result.AddRange(padding);

            result.AddRange(indirect);
            result.AddRange(attack);
            result.AddRange(restore);
            result.AddRange(padding);

            result.AddRange(indirect);
            result.AddRange(restore);
            result.AddRange(attack);
            result.AddRange(padding);

            return result.ToArray();
        }
    }
}
