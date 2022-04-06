using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Main;

namespace Sleepey.Maelstrom
{
    public static class CutNameFix
    {
        public static string[] ApplicableRegions = new string[] { "fre", "ger", "spa" };

        public static void Apply(FileSource mainSource)
        {
            var kernel = new Kernel(mainSource.GetFile(Env.KernelPath));
            var percentString = FF8String.Encode("Percent");
            var catString = FF8String.Encode("Catastrophe");

            // add new strings to the magic text section in the kernel
            var magicText = kernel.MagicText.ToList();
            var percentOffset = magicText.Count;
            var catOffset = percentOffset + percentString.Count();
            var totalChange = percentString.Count() + catString.Count();

            magicText.AddRange(percentString);
            magicText.AddRange(catString);
            kernel.MagicText = magicText;

            // apply new names to spells
            kernel.MagicData[54].NameOffset = (ushort)percentOffset;
            kernel.MagicData[55].NameOffset = (ushort)catOffset;

            // adjust section offsets in the header
            for (int i = 33; i < kernel.SectionCount; i++)
            {
                kernel.SectionOffsets[i] += (uint)totalChange;
            }

            mainSource.ReplaceFile(Env.KernelPath, kernel.Encode());
        }
    }
}
