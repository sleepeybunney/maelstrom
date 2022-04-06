using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Archive
{
    public static class WildcardPath
    {
        public static string DirectoryWildcard { get; } = "[x]";

        public static List<string> DirectoryOptions { get; } = new List<string>() { "x", Env.RegionCode };

        public static bool Match(string path1, string path2)
        {
            path1 = path1.ToLower();
            path2 = path2.ToLower();
            foreach (var o in DirectoryOptions)
            {
                if (path1.Replace("\\" + DirectoryWildcard + "\\", "\\" + o + "\\") == path2) return true;
                if (path2.Replace("\\" + DirectoryWildcard + "\\", "\\" + o + "\\") == path1) return true;
            }
            return false;
        }
    }
}
