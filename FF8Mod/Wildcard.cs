using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8Mod
{
    public static class WildcardPath
    {
        public static string DirectoryWildcard = "[x]";

        public static string[] DirectoryOptions
        {
            get { return new string[] { "x", Globals.RegionCode }; }
        }

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
