using FF8Mod;
using Xunit;

namespace FF8ModTest
{
    public class MonsterTest
    {
        [Fact]
        public void FullTest()
        {
            // load a real monster dat file from the game
            var minotaur = Monster.FromFile(@"TestData\c0m062.dat");

            // check various known values
            Assert.Equal("Minotaur", minotaur.Info.Name);

            Assert.Equal(100, minotaur.Info.Hp[0]);
            Assert.Equal(75, minotaur.Info.Hp[1]);
            Assert.Equal(0, minotaur.Info.Hp[2]);
            Assert.Equal(0, minotaur.Info.Hp[3]);

            Assert.Equal(100, minotaur.Info.Str[0]);
            Assert.Equal(5, minotaur.Info.Str[1]);
            Assert.Equal(40, minotaur.Info.Str[2]);
            Assert.Equal(100, minotaur.Info.Str[3]);

            Assert.Equal(48, minotaur.Info.Mag[0]);
            Assert.Equal(2, minotaur.Info.Mag[1]);
            Assert.Equal(140, minotaur.Info.Mag[2]);
            Assert.Equal(160, minotaur.Info.Mag[3]);

            Assert.Equal(1, minotaur.Info.Vit[0]);
            Assert.Equal(8, minotaur.Info.Vit[1]);
            Assert.Equal(60, minotaur.Info.Vit[2]);
            Assert.Equal(1, minotaur.Info.Vit[3]);

            Assert.Equal(2, minotaur.Info.Spr[0]);
            Assert.Equal(20, minotaur.Info.Spr[1]);
            Assert.Equal(30, minotaur.Info.Spr[2]);
            Assert.Equal(1, minotaur.Info.Spr[3]);

            Assert.Equal(0, minotaur.Info.Spd[0]);
            Assert.Equal(2, minotaur.Info.Spd[1]);
            Assert.Equal(10, minotaur.Info.Spd[2]);
            Assert.Equal(16, minotaur.Info.Spd[3]);

            Assert.Equal(0, minotaur.Info.Eva[0]);
            Assert.Equal(8, minotaur.Info.Eva[1]);
            Assert.Equal(0, minotaur.Info.Eva[2]);
            Assert.Equal(24, minotaur.Info.Eva[3]);

            Assert.Equal(31455, minotaur.Info.HpAtLevel(69));
            Assert.Equal(92, minotaur.Info.StrAtLevel(33));
            Assert.Equal(42, minotaur.Info.MagAtLevel(6));
            Assert.Equal(65, minotaur.Info.VitAtLevel(40));
            Assert.Equal(75, minotaur.Info.SprAtLevel(43));
            Assert.Equal(50, minotaur.Info.SpdAtLevel(90));
            Assert.Equal(8, minotaur.Info.EvaAtLevel(100));

            Assert.Equal(2u, minotaur.Info.AbilitiesLow[0].AbilityId);
            Assert.Equal(213u, minotaur.Info.AbilitiesMed[2].AbilityId);
            Assert.Equal(220u, minotaur.Info.AbilitiesHigh[3].AbilityId);

            Assert.Equal(0x00, minotaur.AI.Scripts.Init[0].Op.Code);
            Assert.Equal(0x00, minotaur.AI.Scripts.PreCounter[0].Op.Code);
            Assert.Equal(25, minotaur.AI.Scripts.Execute.Count);
            Assert.Equal(0x0c, minotaur.AI.Scripts.Execute[20].Op.Code);
            Assert.Equal(0, minotaur.AI.Scripts.Execute[20].Args[0]);
            Assert.Equal(9, minotaur.AI.Scripts.Counter.Count);
            Assert.Equal(0x23, minotaur.AI.Scripts.Counter[1].Op.Code);
            Assert.Equal(3, minotaur.AI.Scripts.Counter[1].Args[0]);
            Assert.Equal(14, minotaur.AI.Scripts.Death.Count);

            Assert.Equal(4, minotaur.AI.Strings.Count);
            Assert.Equal("Minotaur “may we join you?”", minotaur.AI.Strings[3]);

            // run it through the encoder & check everything again
            minotaur = Monster.FromBytes(minotaur.Encode());
            Assert.Equal("Minotaur", minotaur.Info.Name);

            Assert.Equal(100, minotaur.Info.Hp[0]);
            Assert.Equal(75, minotaur.Info.Hp[1]);
            Assert.Equal(0, minotaur.Info.Hp[2]);
            Assert.Equal(0, minotaur.Info.Hp[3]);

            Assert.Equal(100, minotaur.Info.Str[0]);
            Assert.Equal(5, minotaur.Info.Str[1]);
            Assert.Equal(40, minotaur.Info.Str[2]);
            Assert.Equal(100, minotaur.Info.Str[3]);

            Assert.Equal(48, minotaur.Info.Mag[0]);
            Assert.Equal(2, minotaur.Info.Mag[1]);
            Assert.Equal(140, minotaur.Info.Mag[2]);
            Assert.Equal(160, minotaur.Info.Mag[3]);

            Assert.Equal(1, minotaur.Info.Vit[0]);
            Assert.Equal(8, minotaur.Info.Vit[1]);
            Assert.Equal(60, minotaur.Info.Vit[2]);
            Assert.Equal(1, minotaur.Info.Vit[3]);

            Assert.Equal(2, minotaur.Info.Spr[0]);
            Assert.Equal(20, minotaur.Info.Spr[1]);
            Assert.Equal(30, minotaur.Info.Spr[2]);
            Assert.Equal(1, minotaur.Info.Spr[3]);

            Assert.Equal(0, minotaur.Info.Spd[0]);
            Assert.Equal(2, minotaur.Info.Spd[1]);
            Assert.Equal(10, minotaur.Info.Spd[2]);
            Assert.Equal(16, minotaur.Info.Spd[3]);

            Assert.Equal(0, minotaur.Info.Eva[0]);
            Assert.Equal(8, minotaur.Info.Eva[1]);
            Assert.Equal(0, minotaur.Info.Eva[2]);
            Assert.Equal(24, minotaur.Info.Eva[3]);

            Assert.Equal(31455, minotaur.Info.HpAtLevel(69));
            Assert.Equal(92, minotaur.Info.StrAtLevel(33));
            Assert.Equal(42, minotaur.Info.MagAtLevel(6));
            Assert.Equal(65, minotaur.Info.VitAtLevel(40));
            Assert.Equal(75, minotaur.Info.SprAtLevel(43));
            Assert.Equal(50, minotaur.Info.SpdAtLevel(90));
            Assert.Equal(8, minotaur.Info.EvaAtLevel(100));

            Assert.Equal(2u, minotaur.Info.AbilitiesLow[0].AbilityId);
            Assert.Equal(213u, minotaur.Info.AbilitiesMed[2].AbilityId);
            Assert.Equal(220u, minotaur.Info.AbilitiesHigh[3].AbilityId);

            Assert.Equal(0x00, minotaur.AI.Scripts.Init[0].Op.Code);
            Assert.Equal(0x00, minotaur.AI.Scripts.PreCounter[0].Op.Code);
            Assert.Equal(25, minotaur.AI.Scripts.Execute.Count);
            Assert.Equal(0x0c, minotaur.AI.Scripts.Execute[20].Op.Code);
            Assert.Equal(0, minotaur.AI.Scripts.Execute[20].Args[0]);
            Assert.Equal(9, minotaur.AI.Scripts.Counter.Count);
            Assert.Equal(0x23, minotaur.AI.Scripts.Counter[1].Op.Code);
            Assert.Equal(3, minotaur.AI.Scripts.Counter[1].Args[0]);
            Assert.Equal(14, minotaur.AI.Scripts.Death.Count);

            Assert.Equal(4, minotaur.AI.Strings.Count);
            Assert.Equal("Minotaur “may we join you?”", minotaur.AI.Strings[3]);
        }
    }
}
