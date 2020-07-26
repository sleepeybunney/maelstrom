using System;
using System.Collections.Generic;
using System.Text;
using FF8Mod;
using FF8Mod.Archive;
using FF8Mod.Main;

namespace FF8Mod.Maelstrom
{
    public static class AbilityShuffle
    {
        public static void Randomise(FileSource mainSource, int seed)
        {
            var random = new Random(seed);
            var kernel = new Kernel(mainSource.GetFile(Globals.KernelPath));
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 21; j++)
                {
                    kernel.JunctionableGFs[i].Abilities[j].Ability = (byte)random.Next(1, 116);
                    kernel.JunctionableGFs[i].Abilities[j].Unlocker = 0;
                }
            }
            mainSource.ReplaceFile(Globals.KernelPath, kernel.Encode());
        }
    }
}
