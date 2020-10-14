using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FF8Mod.Archive;

namespace FF8Mod
{
    public static class Globals
    {
        public static string ExePath;
        public static bool Remastered = false;

        public static string RegionCode = "eng";

        public static Dictionary<string, string> RegionExts = new Dictionary<string, string>()
        {
            { "eng", "en" },
            { "fre", "fr" },
            { "ita", "it" },
            { "ger", "de" },
            { "spa", "es" },
            { "jp", "jp" }
        };

        public static string RegionExt
        {
            get { return RegionExts[RegionCode]; }
        }

        public static string ScriptFileExtension
        {
            get
            {
                if (Remastered) return string.Format("_{0}.jsm", RegionExt);
                else return ".jsm";
            }
        }

        public static string MessageFileExtension
        {
            get
            {
                if (Remastered) return string.Format("_{0}.msd", RegionExt);
                else return ".msd";
            }
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
            get { return Path.Combine(GameDirectory, "data", string.Format("lang-{0}", RegionExt)); }
        }

        public static string BattlePath
        {
            get { return Path.Combine(ArchivePath, "battle"); }
        }

        public static string FieldPath
        {
            get
            {
                if (Remastered) return Path.Combine(GameDirectory, "data", "field");
                else return Path.Combine(ArchivePath, "field");
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
            get { return @"c:\ff8\data\" + WildcardPath.DirectoryWildcard; }
        }

        public static string KernelPath
        {
            get { return Path.Combine(DataPath, "kernel.bin"); }
        }
    }
}
