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
            if (path1 == null) throw new ArgumentNullException("path1");
            if (path2 == null) throw new ArgumentNullException("path2");

            path1 = path1.ToLower().Replace('\\', '/');
            path2 = path2.ToLower().Replace('\\', '/');

            foreach (var o in DirectoryOptions)
            {
                var pattern = string.Format("/{0}/", DirectoryWildcard);
                var replacement = string.Format("/{0}/", o);

                if (path1.Replace(pattern, replacement) == path2) return true;
                if (path2.Replace(pattern, replacement) == path1) return true;
            }
            return false;
        }
    }
}
