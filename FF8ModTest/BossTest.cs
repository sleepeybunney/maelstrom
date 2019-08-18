using FF8Mod;
using FF8Mod.Maelstrom;
using Xunit;
using Xunit.Abstractions;

namespace FF8ModTest
{
    public class BossTest
    {
        public static MonsterInfo ifrit = new MonsterInfo("Ifrit")
        {
            Hp = new byte[] { 60, 60, 0, 0 },
            Str = new byte[] { 28, 2, 130, 100 },
            Vit = new byte[] { 1, 5, 40, 2 }
        };

        public static MonsterInfo oilboyle = new MonsterInfo("Oilboyle")
        {
            Hp = new byte[] { 6, 3, 1, 2 },
            Str = new byte[] { 80, 6, 140, 200 },
            Vit = new byte[] { 2, 20, 45, 2 }
        };

        public static MonsterInfo omega = new MonsterInfo("Omega Weapon")
        {
            Hp = new byte[] { 100, 100, 100, 100 },
            Str = new byte[] { 200, 5, 250, 130 },
            Vit = new byte[] { 1, 30, 60, 2 }
        };

        public static MonsterInfo norg = new MonsterInfo("NORG")
        {
            Hp = new byte[] { 0, 10, 3, 4 },
            Str = new byte[] { 100, 4, 60, 150 },
            Vit = new byte[] { 1, 6, 4, 1 }
        };

        public static MonsterInfo norgPod = new MonsterInfo("NORG Pod")
        {
            Hp = new byte[] { 0, 0, 0, 2 },
            Str = new byte[] { 8, 200, 4, 200 },
            Vit = new byte[] { 1, 4, 150, 1 }
        };

        private readonly ITestOutputHelper Output;
        
        public BossTest(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void ScalingTest()
        {
            // single monster to bigger single monster
            var ifritToOmega = Boss.ScaleMonster(ifrit, ifrit, omega);
            Assert.Equal(omega.Hp, ifritToOmega.Hp);
            Assert.Equal(omega.Str, ifritToOmega.Str);
            Assert.Equal(omega.Vit, ifritToOmega.Vit);

            Assert.True(ifritToOmega.HpAtLevel(10) < ifritToOmega.HpAtLevel(50));
            Assert.True(ifritToOmega.HpAtLevel(50) < ifritToOmega.HpAtLevel(100));
            Assert.True(ifritToOmega.HpAtLevel(50) > ifrit.HpAtLevel(50));

            // (effectively-) single monster to smaller single monster
            var oilboyleToIfrit = Boss.ScaleMonster(oilboyle, oilboyle, ifrit);
            Assert.Equal(ifrit.Hp, oilboyleToIfrit.Hp);
            Assert.Equal(ifrit.Str, oilboyleToIfrit.Str);
            Assert.Equal(ifrit.Vit, oilboyleToIfrit.Vit);

            Assert.True(oilboyleToIfrit.HpAtLevel(10) < oilboyleToIfrit.HpAtLevel(50));
            Assert.True(oilboyleToIfrit.HpAtLevel(50) < oilboyleToIfrit.HpAtLevel(100));
            Assert.True(oilboyleToIfrit.HpAtLevel(10) < oilboyle.HpAtLevel(10));
            Assert.True(oilboyleToIfrit.HpAtLevel(50) > oilboyle.HpAtLevel(50));    // ifrit's hp starts low but curves up fast
            
            // all monsters compare as you would expect of their scaling targets
            Assert.True(ifritToOmega.HpAtLevel(50) > oilboyle.HpAtLevel(50));
            Assert.True(ifritToOmega.HpAtLevel(50) > oilboyleToIfrit.HpAtLevel(50));
            Assert.Equal(ifrit.HpAtLevel(50), oilboyleToIfrit.HpAtLevel(50));

            // multi-monster to single monster that is smaller than the main but bigger than the limbs
            var norgToIfrit = Boss.ScaleMonster(norg, norg, ifrit);
            var norgPodToIfrit = Boss.ScaleMonster(norgPod, norg, ifrit);
            Assert.Equal(ifrit.Hp, norgToIfrit.Hp);
            Assert.Equal(ifrit.Str, norgToIfrit.Str);
            Assert.Equal(ifrit.Vit, norgToIfrit.Vit);
            Assert.NotEqual(ifrit.Hp, norgPodToIfrit.Hp);
            Assert.NotEqual(ifrit.Str, norgPodToIfrit.Str);
            Assert.NotEqual(ifrit.Vit, norgPodToIfrit.Vit);

            Assert.True(norgToIfrit.HpAtLevel(10) < norgToIfrit.HpAtLevel(50));
            Assert.True(norgToIfrit.HpAtLevel(50) < norgToIfrit.HpAtLevel(100));
            Assert.True(norgToIfrit.HpAtLevel(50) < norg.HpAtLevel(50));
            Assert.True(norgPodToIfrit.HpAtLevel(50) < norgPod.HpAtLevel(50));
            Assert.True(norgPodToIfrit.HpAtLevel(50) < norgToIfrit.HpAtLevel(50));

            // multi-monster to bigger single monster
            var norgToOmega = Boss.ScaleMonster(norg, norg, omega);
            var norgPodToOmega = Boss.ScaleMonster(norgPod, norg, omega);
            Assert.Equal(omega.Hp, norgToOmega.Hp);
            Assert.Equal(omega.Str, norgToOmega.Str);
            Assert.Equal(omega.Vit, norgToOmega.Vit);
            Assert.NotEqual(omega.Hp, norgPodToOmega.Hp);
            Assert.NotEqual(omega.Str, norgPodToOmega.Str);
            Assert.NotEqual(omega.Vit, norgPodToOmega.Vit);

            Output.WriteLine(norgPodToOmega.HpAtLevel(20).ToString());
            Output.WriteLine(norgPod.HpAtLevel(20).ToString());

            Output.WriteLine(norgPodToOmega.HpAtLevel(50).ToString());
            Output.WriteLine(norgPod.HpAtLevel(50).ToString());

            Output.WriteLine(norgPodToOmega.HpAtLevel(100).ToString());
            Output.WriteLine(norgPod.HpAtLevel(100).ToString());

            Assert.True(norgToOmega.HpAtLevel(10) < norgToOmega.HpAtLevel(50));
            Assert.True(norgToOmega.HpAtLevel(50) < norgToOmega.HpAtLevel(100));
            Assert.True(norgToOmega.HpAtLevel(50) > norg.HpAtLevel(50));
            Assert.True(norgPodToOmega.HpAtLevel(50) < norgToOmega.HpAtLevel(50));
        }
    }
}
