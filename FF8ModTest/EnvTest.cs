using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Sleepey.FF8Mod;

namespace Sleepey.FF8ModTest
{
    public class EnvTest
    {
        public static TheoryData<string, string> RegionalPaths => new TheoryData<string, string>
        {
            { @"FF8_ES.exe", "spa" },
            { @"C:\Steam\steamapps\common\FINAL FANTASY VIII\FF8_EN.exe", "eng" },
            { @"D:\Steam\steamapps\common\FINAL FANTASY VIII\FF8_FR.exe", "fre" },
            { @"C:\Program Files (x86)\Final Fantasy VIII\FF8_IT.exe", "ita" },
            { @"/home/sleepey/.steam/debian-installation/steamapps/common/FF8_IT.exe", "ita" },
            { @"/media/sleepey/SSD2/SteamLibrary/steamapps/common/FF8_DE.exe", "ger" }
        };

        [Theory]
        [MemberData(nameof(RegionalPaths))]
        public void RegionCodeFromPathTest(string path, string code) => Assert.Equal(code, Env.RegionCodeFromPath(path));

        [Fact]
        public void RegionCodeFromPathNullTest() => Assert.Equal("eng", Env.RegionCodeFromPath(null));

        [Fact]
        public void RegionCodeFromPathEmptyTest() => Assert.Equal("eng", Env.RegionCodeFromPath(string.Empty));

        [Fact]
        public void RegionCodeFromPathNoCodeTest() => Assert.Equal("eng", Env.RegionCodeFromPath(@"C:\Steam\steamapps\common\FINAL FANTASY VIII Remastered\FFVIII.exe"));

        [Fact]
        public void RegionCodeFromPathUnknownCodeTest() => Assert.Equal("eng", Env.RegionCodeFromPath("FF8_X3.exe"));
    }
}
