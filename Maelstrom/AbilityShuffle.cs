using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Collections;
using Sleepey.FF8Mod.Main;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod;

namespace Sleepey.Maelstrom
{
    public static class AbilityShuffle
    {
        public static List<AbilityMeta> Abilities = JsonSerializer.Deserialize<List<AbilityMeta>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Abilities.json"));
        public static List<GFMeta> GFNames = JsonSerializer.Deserialize<List<GFMeta>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.JunctionableGFs.json"));

        public static List<JunctionableGF> Randomise(FileSource mainSource, int seed, State settings)
        {
            var random = new Random(seed + 1);
            var kernel = new Kernel(mainSource.GetFile(Env.KernelPath));
            var init = new Init(mainSource.GetFile(Env.InitPath));

            if (settings.GfAbilitiesSwapSets)
            {
                SwapSets(mainSource, random, kernel, init);
            }
            else
            {
                GenerateRandomSets(settings, random, kernel, init);
            }

            ModifyAPCosts(kernel);

            SaveChanges(mainSource, kernel, init);
            return kernel.JunctionableGFs.ToList();
        }

        private static void SaveChanges(FileSource mainSource, Kernel kernel, Init init)
        {
            mainSource.ReplaceFile(Env.KernelPath, kernel.Encode());
            mainSource.ReplaceFile(Env.InitPath, init.Encode());
        }

        private static void ModifyAPCosts(Kernel kernel)
        {
            // reduce "empty" cost
            kernel.Abilities[24].APCost = 60;

            // increase "ribbon" cost
            kernel.Abilities[77].APCost = 250;
        }

        private static void GenerateRandomSets(State settings, Random random, Kernel kernel, Init init)
        {
            var nonMenuAbilities = Abilities
                .Where(a => !a.ItemExclusive || settings.GfAbilitiesIncludeItemOnly)
                .Where(a => !a.MenuAbility)
                .ToList();

            var menuAbilities = Abilities
                .Where(a => !a.ItemExclusive || settings.GfAbilitiesIncludeItemOnly)
                .Where(a => a.MenuAbility)
                .ToList();

            var includedAbilities = Abilities.Where(a => !a.ItemExclusive || settings.GfAbilitiesIncludeItemOnly).ToList();

            for (int gfId = 0; gfId < 16; gfId++)
            {
                // clear auto-unlocked abilities
                init.GFs[gfId].Abilities = new BitArray(init.GFs[gfId].Abilities.Length, false);

                List<int> unusedAbilities = nonMenuAbilities.Select(a => a.AbilityID)
                    .Concat(menuAbilities.Select(a => a.AbilityID)).ToList();

                var basicsEligible = settings.GfAbilitiesBasicsType == "all" || (settings.GfAbilitiesBasicsType == "first" && gfId < 3);

                for (int learnSlotIndex = 0; learnSlotIndex < 21; learnSlotIndex++)
                {
                    if (learnSlotIndex < 4 && basicsEligible)
                    {
                        AddBasicAbilities(kernel.JunctionableGFs[gfId].Abilities, init, gfId, unusedAbilities);
                        learnSlotIndex = 3;
                    }
                    else
                    {
                        AddRandomAbility(random, kernel.JunctionableGFs[gfId].Abilities, learnSlotIndex, init.GFs[gfId], unusedAbilities, menuAbilities, settings.GFAbilitiesNoMenuDuplicates);
                    }
                }

                // sort abilities
                kernel.JunctionableGFs[gfId].Abilities = kernel.JunctionableGFs[gfId].Abilities.OrderBy(a => a.Ability == 0 ? byte.MaxValue : a.Ability).ToList();

                // clear ability being learned
                init.GFs[gfId].CurrentAbility = 0;
            }
        }

        private static void AddBasicAbilities(IList<GFAbility> abilities, Init init, int gfId, List<int> unusedAbilities)
        {
            var initGF = init.GFs[gfId];

            AddAbility(abilities, 0, initGF, 20, true);
            AddAbility(abilities, 1, initGF, 21, true);
            AddAbility(abilities, 2, initGF, 22, true);
            AddAbility(abilities, 3, initGF, 23, true);

            unusedAbilities.Remove(20);
            unusedAbilities.Remove(21);
            unusedAbilities.Remove(22);
            unusedAbilities.Remove(23);
        }

        private static void SwapSets(FileSource mainSource, Random random, Kernel kernel, Init init)
        {
            var unmatchedGFs = Enumerable.Range(0, 16).ToList();
            var cleanKernel = new Kernel(mainSource.GetFile(Env.KernelPath));
            var cleanInit = new Init(mainSource.GetFile(Env.InitPath));

            for (int i = 0; i < 16; i++)
            {
                var matchedGF = unmatchedGFs[random.Next(unmatchedGFs.Count)];
                unmatchedGFs.Remove(matchedGF);
                init.GFs[i].Abilities = cleanInit.GFs[matchedGF].Abilities;
                init.GFs[i].CurrentAbility = cleanInit.GFs[matchedGF].CurrentAbility;
                kernel.JunctionableGFs[i].Abilities = cleanKernel.JunctionableGFs[matchedGF].Abilities;
            }
        }

        private static void AddAbility(IList<GFAbility> abilities, int abilityIndex, InitGF initGF, byte abilityId, bool learned = false)
        {
            abilities[abilityIndex] = new GFAbility(1, 255, abilityId, 0);
            initGF.Abilities[abilityId] = learned;
        }

        private static void AddRandomAbility(Random random, IList<GFAbility> abilities, int abilityIndex, InitGF initGF, List<int> unusedAbilities, List<AbilityMeta> menuAbilities, bool noMenuDuplicates)
        {
            var ability = (byte)unusedAbilities[random.Next(unusedAbilities.Count)];

            AddAbility(abilities, abilityIndex, initGF, ability);

            initGF.Abilities[ability] = (ability >= 20 && ability <= 23);
            unusedAbilities.Remove(ability);

            if (noMenuDuplicates)
            {
                AbilityMeta menuAbility = menuAbilities.Find(a => a.AbilityID == ability);
                menuAbilities.Remove(menuAbility);
            }
        }

        public static void ForceScrollsInBalambShop(FileSource menuSource)
        {
            var shopData = menuSource.GetFile(Env.ShopPath).ToArray();

            // normal ammo -> magic scroll
            shopData[54] = 55;
            shopData[55] = 255;

            // shotgun ammo -> gf scroll
            shopData[56] = 56;
            shopData[57] = 255;

            // g-potion -> draw scroll
            shopData[58] = 57;
            shopData[59] = 255;

            // g-hi-potion -> item scroll
            shopData[60] = 58;
            shopData[61] = 255;

            // g-returner -> amnesia greens
            shopData[62] = 41;
            shopData[63] = 255;

            menuSource.ReplaceFile(Env.ShopPath, shopData);
        }
    }
}
