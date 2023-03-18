using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod
{
    public static class Env
    {
        public static string ExePath { get; set; }
        public static bool Remastered { get; set; }

        public static string RegionCode { get; set; } = "eng";

        public static Dictionary<string, string> RegionExts { get; } = new Dictionary<string, string>()
        {
            { "eng", "en" },
            { "fre", "fr" },
            { "ita", "it" },
            { "ger", "de" },
            { "spa", "es" },
            { "jp", "jp" }
        };

        public static string RegionCodeFromPath(string path)
        {
            if (path == null || path.Length < 6) return "eng";
            var ext = path.Substring(path.Length - 6, 2).ToLower();
            if (!RegionExts.Values.Contains(ext)) return "eng";
            return RegionExts.First(x => x.Value == ext).Key;
        }

        public static string RegionExt { get => RegionExts[RegionCode]; }

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

        public static string GameDirectory { get => Path.GetDirectoryName(ExePath); }

        public static string Af3dnPath { get => Path.Combine(GameDirectory, "af3dn.p"); }

        public static string ArchivePath { get => Path.Combine(GameDirectory, "Data", string.Format("lang-{0}", RegionExt)); }

        public static string BattlePath { get => Path.Combine(ArchivePath, "battle"); }

        public static string FieldPath
        {
            get
            {
                if (Remastered) return Path.Combine(GameDirectory, "Data", "field");
                else return Path.Combine(ArchivePath, "field");
            }
        }

        public static string MenuPath { get => Path.Combine(ArchivePath, "menu"); }

        public static string MainPath { get => Path.Combine(ArchivePath, "main"); }

        public static string MainZzzPath { get => Path.Combine(GameDirectory, "main.zzz"); }

        public static string OtherZzzPath { get => Path.Combine(GameDirectory, "other.zzz"); }

        public static string HDTexturePath { get => Path.Combine(GameDirectory, @"Data\textures\battle.fs\hd_new"); }

        public static ArchiveStream FieldArchive { get => new ArchiveStream(FieldPath); }

        public static ArchiveStream MainArchive { get => new ArchiveStream(MainPath); }

        public static string DataPath { get => @"c:\ff8\data\" + WildcardPath.DirectoryWildcard; }

        public static string KernelPath { get => Path.Combine(DataPath, "kernel.bin"); }

        public static string InitPath { get => Path.Combine(DataPath, "init.out"); }

        public static string WeaponUpgradePath { get => Path.Combine(DataPath, @"menu\mwepon.bin"); }

        public static string EncounterFilePath { get => Path.Combine(DataPath, @"battle\scene.out"); }

        public static string DoomtrainPath { get => Path.Combine(DataPath, @"menu\mthomas.bin"); }

        public static string MagsortPath { get => Path.Combine(DataPath, @"menu\magsort.bin"); }

        public static string MngrpPath { get => Path.Combine(DataPath, @"menu\mngrp.bin"); }

        public static string PricePath { get => Path.Combine(DataPath, @"menu\price.bin"); }
    }
}
