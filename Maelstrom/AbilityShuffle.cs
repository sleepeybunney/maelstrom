using System;
using System.Collections.Generic;
using System.Text;
using FF8Mod;
using FF8Mod.Archive;
using FF8Mod.Main;
using System.Text.Json;
using System.Linq;

namespace FF8Mod.Maelstrom
{
    public static class AbilityShuffle
    {
        public static List<AbilityMeta> Abilities = JsonSerializer.Deserialize<List<AbilityMeta>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.Abilities.json"));
        public static List<GFMeta> GFNames = JsonSerializer.Deserialize<List<GFMeta>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.JunctionableGFs.json"));

        public static List<JunctionableGF> Randomise(FileSource mainSource, int seed, bool includeItemExclusives)
        {
            var random = new Random(seed);
            var kernel = new Kernel(mainSource.GetFile(Globals.KernelPath));
            var includedAbilities = Abilities.Where(a => !a.ItemExclusive || includeItemExclusives).ToList();
            var result = new List<JunctionableGF>();

            for (int i = 0; i < 16; i++)
            {
                var used = new List<byte>();
                for (int j = 0; j < 21; j++)
                {
                    // prevent duplicates
                    var ability = (byte)includedAbilities[random.Next(includedAbilities.Count)].AbilityID;
                    if (used.Contains(ability)) ability = 0;
                    else used.Add(ability);

                    kernel.JunctionableGFs[i].Abilities[j].Ability = ability;
                    kernel.JunctionableGFs[i].Abilities[j].Unlocker = 0;
                }

                kernel.JunctionableGFs[i].Abilities = kernel.JunctionableGFs[i].Abilities.OrderBy(a => a.Ability == 0 ? byte.MaxValue : a.Ability).ToArray();
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
        public bool ItemExclusive { get; set; } = false;
    }

    public class GFMeta
    {
        public int GFID { get; set; }
        public string GFName { get; set; }
    }
}
