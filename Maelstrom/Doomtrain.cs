using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Menu;

namespace Sleepey.Maelstrom
{
    public static class Doomtrain
    {
        public static List<Item> FenceItems = Item.Lookup.Values.Where(i => i.FenceItem).ToList();
        public static List<Item> MonsterItems = Item.Lookup.Values.Where(i => i.MonsterItem).ToList();
        public static List<Item> Medicines = Item.Lookup.Values.Where(i => i.Medicine).ToList();

        public static List<Item> Randomise(int seed)
        {
            var random = new Random(seed + 12);

            return new List<Item>()
            {
                FenceItems[random.Next(FenceItems.Count)],
                MonsterItems[random.Next(MonsterItems.Count)],
                Medicines[random.Next(Medicines.Count)]
            };
        }

        public static void Apply(FileSource menuSource, List<Item> items)
        {
            // update doomtrain file
            var result = new byte[16];
            for (int i = 0; i < 3; i++)
            {
                result[i * 4] = (byte)items[i].ID;
                result[(i * 4) + 1] = 6;
            }
            menuSource.ReplaceFile(Globals.DoomtrainPath, result);

            // load magazine text file
            var mngrpOffset = 0x1a8000;
            var mngrpLength = 0x3000;

            var mngrp = menuSource.GetFile(Globals.MngrpPath);
            var str00Bytes = mngrp.Skip(mngrpOffset).Take(mngrpLength);
            var str00 = TextFile.FromBytes(str00Bytes, true, true);

            // update text for occult fan 1
            var fenceId = 97;
            var fenceLines = new List<string>()
            {
                "Mr. Burk said it",
                "occurred while he",
                "was making a fence",
                string.Format("with {0}s.", items[0].Name.ToLower()),
                string.Empty
            };
            var fenceText = string.Join("{02}", fenceLines);

            // update text for occult fan 2
            var monsterId = 102;
            var monsterLines = new List<string>()
            {
                "Have you ever seen any monster or airplane",
                "like this!?  Some say it may be Esthar`s secret",
                "weapon, but residents of the area say it appears",
                string.Format("when there is a major {0} outbreak.", items[1].MonsterName)
            };
            var monsterText = string.Join("{02}", monsterLines);

            // update text for occult fan 4
            var medicineId = 110;
            var medicineLines = new List<string>()
            {
                "Neither has been proven, but people",
                "that were attacked all owned",
                string.Format("{0} Medicine.", items[2].MedicineName),
                string.Empty
            };
            var medicineText = string.Join("{02}", medicineLines);

            // save text changes
            str00.Pages[0].Strings[fenceId] = fenceText;
            str00.Pages[0].Strings[monsterId] = monsterText;
            str00.Pages[0].Strings[medicineId] = medicineText;

            var newFile = mngrp.Take(mngrpOffset).ToList();
            newFile.AddRange(str00.Pages[0].Encode());
            newFile.AddRange(mngrp.Skip(mngrpOffset + mngrpLength));
            menuSource.ReplaceFile(Globals.MngrpPath, newFile);
        }
    }
}
