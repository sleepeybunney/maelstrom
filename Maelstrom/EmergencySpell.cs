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
    public static class EmergencySpell
    {
        public static void Apply(FileSource mainSource)
        {
            // add one "the end" spell to squall's starting inventory
            var init = new Init(mainSource.GetFile(Env.InitPath));
            init.Characters[0].Spells[0] = 56;
            init.Characters[0].Spells[1] = 1;
            mainSource.ReplaceFile(Env.InitPath, init.Encode());
        }
    }
}
