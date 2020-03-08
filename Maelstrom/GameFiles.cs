using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FF8Mod.Maelstrom
{
    public static class GameFiles
    {
        public static string ExePath
        {
            get { return Properties.Settings.Default.GameLocation; }
        }
        
        public static string GameDirectory
        {
            get { return Path.GetDirectoryName(ExePath); }
        }

        public static string Af3dnPath
        {
            get { return Path.Combine(GameDirectory, "af3dn.p"); }
        }

        public static string ArchivePath
        {
            get { return Path.Combine(GameDirectory, "data", "lang-en"); }
        }

        public static string BattlePath
        {
            get { return Path.Combine(ArchivePath, "battle"); }
        }

        public static string FieldPath
        {
            get { return Path.Combine(ArchivePath, "field"); }
        }

        public static string MenuPath
        {
            get { return Path.Combine(ArchivePath, "menu"); }
        }
    }
}
