using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xunit;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8ModTest.Archive
{
    public class WildcardTest
    {
        [Theory]
        [InlineData(null, "not null")]
        [InlineData("not null", null)]
        [InlineData(null, null)]
        public void MatchNullTest(string path1, string path2) => Assert.Throws<ArgumentNullException>(() => WildcardPath.Match(path1, path2));

        [Theory]
        [InlineData("", "not empty", false)]
        [InlineData("not empty", "", false)]
        [InlineData("", "", true)]
        public void MatchEmptyTest(string path1, string path2, bool match) => Assert.Equal(match, WildcardPath.Match(path1, path2));

        [Fact]
        public void MatchSamePathTest() => Assert.True(WildcardPath.Match(@"C:\ff8\data\eng\battle\scene.out", @"C:\ff8\data\eng\battle\scene.out"));

        [Fact]
        public void MatchSamePathDiffCaseTest() => Assert.True(WildcardPath.Match(@"c:\ff8\Data\eng\menu\mwepon.bin", @"C:\ff8\data\eng\menu\MWEPON.BIN"));

        [Fact]
        public void MatchDiffPathTest() => Assert.False(WildcardPath.Match(@"C:\ff8\data\eng\battle\scene.out", @"C:\ff8\data\eng\menu\mwepon.bin"));

        [Theory]
        [InlineData(@"C:\ff8\data\[x]\battle\scene.out", @"C:\ff8\data\eng\battle\scene.out", true)]
        [InlineData(@"C:\ff8\data\eng\kernel.bin", @"C:\ff8\data\[x]\kernel.bin", true)]
        [InlineData(@"C:\ff8\data\[x]\menu\mthomas.bin", @"C:\ff8\data\x\menu\mthomas.bin", true)]
        [InlineData(@"C:\ff8\data\fre\battle\scene.out", @"C:\ff8\data\[x]\battle\scene.out", false)]
        [InlineData(@"C:\ff8\data\[x]\menu\mngrp.bin", @"C:\ff8\data\[x]\menu\mngrp.bin", false)]
        public void MatchWildcardTest(string path1, string path2, bool match) => Assert.Equal(match, WildcardPath.Match(path1, path2));

        [Theory]
        [InlineData(@"C:\ff8\data\[x]\battle\scene.out", @"c:\ff8\data\eng\battle\scene.out", true)]
        [InlineData(@"c:\ff8\data\ENG\kernel.bin", @"C:\ff8\data\[x]\kernel.bin", true)]
        [InlineData(@"C:\ff8\data\[x]\menu\mthomas.bin", @"c:\ff8\data\x\menu\mthomas.bin", true)]
        [InlineData(@"c:\ff8\Data\eng\battle\scene.out", @"C:\ff8\data\[x]\battle\SCENE.OUT", true)]
        [InlineData(@"C:\FF8\data\x\Menu\mngrp.bin", @"c:\ff8\data\[x]\menu\mngrp.bin", true)]
        [InlineData(@"c:\ff8\Data\fre\battle\scene.out", @"C:\ff8\data\[x]\battle\SCENE.OUT", false)]
        [InlineData(@"C:\FF8\data\[x]\Menu\mngrp.bin", @"c:\ff8\data\[x]\menu\mngrp.bin", false)]
        public void MatchWildcardDiffCaseTest(string path1, string path2, bool match) => Assert.Equal(match, WildcardPath.Match(path1, path2));

        [Theory]
        [InlineData(@"/home/sleepey/ff8/ff8_en.exe", @"/home/sleepey/ff8/ff8_en.exe", true)]
        [InlineData(@"/home/sleepey/ff8/ff8_en.exe", @"/home/sleepey/ff8/ff8_fr.exe", false)]
        [InlineData(@"/data/[x]/init.out", @"/data/eng/init.out", true)]
        [InlineData(@"/data/x/init.out", @"/data/[x]/init.out", true)]
        [InlineData(@"/data/[x]/init.out", @"/data/fre/init.out", false)]
        [InlineData(@"/data/[x]/init.out", @"/data/[x]/init.out", false)]
        [InlineData(@"C:\ff8\data\[x]\init.out", @"C:/ff8/data/eng/init.out", true)]
        public void MatchForwardSlashTest(string path1, string path2, bool match) => Assert.Equal(match, WildcardPath.Match(path1, path2));
    }
}
