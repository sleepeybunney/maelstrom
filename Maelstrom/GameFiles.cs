using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public static class GameFiles
    {
        public static bool Remastered = false;  // todo

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
            get
            {
                if (Remastered) return MainZzzPath + @";data\field.fs";
                return Path.Combine(ArchivePath, "field");
            }
        }

        public static string MenuPath
        {
            get { return Path.Combine(ArchivePath, "menu"); }
        }

        public static string MainPath
        {
            get { return Path.Combine(ArchivePath, "main"); }
        }

        public static string MainZzzPath
        {
            get { return Path.Combine(GameDirectory, "main.zzz"); }
        }

        public static string OtherZzzPath
        {
            get { return Path.Combine(GameDirectory, "other.zzz"); }
        }

        public static ArchiveStream FieldArchive
        {
            get { return new ArchiveStream(FieldPath); }
        }

        public static ArchiveStream MainArchive
        {
            get { return new ArchiveStream(MainPath); }
        }

        public static string DataPath
        {
            get { return @"c:\ff8\data\eng"; }
        }

        public static string KernelPath
        {
            get { return Path.Combine(DataPath, "kernel.bin"); }
        }
    }
}
