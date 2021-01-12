using System;
using System.Collections.Generic;
using System.Text;
using FF8Mod;
using FF8Mod.Archive;
using FF8Mod.Main;
using System.Text.Json;

namespace FF8Mod.Maelstrom
{
    public static class AbilityShuffle
    {
        public static List<AbilityMeta> AbilityNames = JsonSerializer.Deserialize<List<AbilityMeta>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.Abilities.json"));
        public static List<GFMeta> GFNames = JsonSerializer.Deserialize<List<GFMeta>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.JunctionableGFs.json"));

        public static List<JunctionableGF> Randomise(FileSource mainSource, int seed)
        {
            var random = new Random(seed);
            var kernel = new Kernel(mainSource.GetFile(Globals.KernelPath));
            var result = new List<JunctionableGF>();

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 21; j++)
                {
                    kernel.JunctionableGFs[i].Abilities[j].Ability = (byte)random.Next(1, 116);
                    kernel.JunctionableGFs[i].Abilities[j].Unlocker = 0;
                }
                result.Add(kernel.JunctionableGFs[i]);
            }

            // reduce "empty" cost
            kernel.Abilities[24].APCost = 60;

            // increase "ribbon" cost
            kernel.Abilities[77].APCost = 250;

            mainSource.ReplaceFile(Globals.KernelPath, kernel.Encode());
            return result;
        }
    }

    // todo: finish implementing the kernel class & get this data from there instead
    public class AbilityMeta
    {
        public int AbilityID { get; set; }
        public string AbilityName { get; set; }
    }

    public class GFMeta
    {
        public int GFID { get; set; }
        public string GFName { get; set; }
    }
}
