using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Main;

namespace Sleepey.Maelstrom
{
    public static class MagicDataFix
    {
        public static List<FF8Mod.Exe.Spell> Spells = JsonSerializer.Deserialize<List<FF8Mod.Exe.Spell>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Spells.json")).ToList();

        public static void Apply(FileSource mainSource)
        {
            var kernel = new Kernel(mainSource.GetFile(Globals.KernelPath));

            foreach (var s in Spells)
            {
                if (s.DrawResist > 0) kernel.MagicData[s.SpellID].DrawResist = (byte)s.DrawResist;
                if (s.Animation > 0) kernel.MagicData[s.SpellID].SpellID = (ushort)s.Animation;
            }

            mainSource.ReplaceFile(Globals.KernelPath, kernel.Encode());
        }
    }
}
