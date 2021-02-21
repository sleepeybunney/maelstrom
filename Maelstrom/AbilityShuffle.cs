using System;
using System.Collections.Generic;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Main;
using System.Text.Json;
using System.Linq;
using System.Collections;

namespace Sleepey.Maelstrom
{
    public static class AbilityShuffle
    {
        public static List<AbilityMeta> Abilities = JsonSerializer.Deserialize<List<AbilityMeta>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Abilities.json"));
        public static List<GFMeta> GFNames = JsonSerializer.Deserialize<List<GFMeta>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.JunctionableGFs.json"));

        public static List<JunctionableGF> Randomise(FileSource mainSource, int seed, State settings)
        {
            var random = new Random(seed + 1);
            var kernel = new Kernel(mainSource.GetFile(Globals.KernelPath));
            var init = new Init(mainSource.GetFile(Globals.InitPath));

            if (settings.GfAbilitiesSwapSets)
            {
                var unmatchedGFs = Enumerable.Range(0, 16).ToList();
                var cleanKernel = new Kernel(mainSource.GetFile(Globals.KernelPath));
                var cleanInit = new Init(mainSource.GetFile(Globals.InitPath));

                for (int i = 0; i < 16; i++)
                {
                    var matchedGF = unmatchedGFs[random.Next(unmatchedGFs.Count)];
                    unmatchedGFs.Remove(matchedGF);
                    init.GFs[i].Abilities = cleanInit.GFs[matchedGF].Abilities;
                    init.GFs[i].CurrentAbility = cleanInit.GFs[matchedGF].CurrentAbility;
                    kernel.JunctionableGFs[i].Abilities = cleanKernel.JunctionableGFs[matchedGF].Abilities;
                }
            }
            else
            {
                var includedAbilities = Abilities.Where(a => !a.ItemExclusive || settings.GfAbilitiesIncludeItemOnly).ToList();

                for (int i = 0; i < 16; i++)
                {
                    // clear auto-unlocked abilities
                    init.GFs[i].Abilities = new BitArray(init.GFs[i].Abilities.Length, false);

                    var unusedAbilities = includedAbilities.Select(a => a.AbilityID).ToList();
                    for (int j = 0; j < 21; j++)
                    {
                        // guarantee basic commands (magic, gf, draw, item)
                        if (j < 4 && settings.GfAbilitiesBasics)
                        {
                            var id = j + 20;
                            kernel.JunctionableGFs[i].Abilities[j].Ability = (byte)id;
                            kernel.JunctionableGFs[i].Abilities[j].Unlocker = 0;
                            init.GFs[i].Abilities[id] = true;
                            unusedAbilities.Remove(id);
                            continue;
                        }

                        // roll abilities
                        var ability = unusedAbilities[random.Next(unusedAbilities.Count)];
                        kernel.JunctionableGFs[i].Abilities[j].Ability = (byte)ability;
                        kernel.JunctionableGFs[i].Abilities[j].Unlocker = 0;
                        init.GFs[i].Abilities[ability] = (ability >= 20 && ability <= 23);
                        unusedAbilities.Remove(ability);
                    }

                    // sort abilities
                    kernel.JunctionableGFs[i].Abilities = kernel.JunctionableGFs[i].Abilities.OrderBy(a => a.Ability == 0 ? byte.MaxValue : a.Ability).ToList();

                    // clear ability being learned
                    init.GFs[i].CurrentAbility = 0;
                }
            }

            // reduce "empty" cost
            kernel.Abilities[24].APCost = 60;

            // increase "ribbon" cost
            kernel.Abilities[77].APCost = 250;

            // save changes
            mainSource.ReplaceFile(Globals.KernelPath, kernel.Encode());
            mainSource.ReplaceFile(Globals.InitPath, init.Encode());
            return kernel.JunctionableGFs.ToList();
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
