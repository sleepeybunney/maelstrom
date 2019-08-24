using FF8Mod.Battle;
using Xunit;

namespace FF8ModTest
{
    public class BattleScriptTest
    {
        [Fact]
        public void EmptyScriptTest()
        {
            var script = new BattleScript();

            // empty sub-scripts should each contain exactly 1 instruction (return)
            Assert.NotNull(script.Init);
            Assert.NotEmpty(script.Init);
            Assert.Equal(0, script.Init[0].Op.Code);

            Assert.NotNull(script.Execute);
            Assert.NotEmpty(script.Execute);
            Assert.Equal(0, script.Execute[0].Op.Code);

            Assert.NotNull(script.Counter);
            Assert.NotEmpty(script.Counter);
            Assert.Equal(0, script.Counter[0].Op.Code);

            Assert.NotNull(script.Death);
            Assert.NotEmpty(script.Death);
            Assert.Equal(0, script.Death[0].Op.Code);

            Assert.NotNull(script.PreCounter);
            Assert.NotEmpty(script.PreCounter);
            Assert.Equal(0, script.PreCounter[0].Op.Code);
        }

        [Theory]
        [InlineData(0x08, new short[] { })]
        [InlineData(0x01, new short[] { 5 })]
        [InlineData(0x02, new short[] { 3, 200, 0, 76, 11 })]
        [InlineData(0x0b, new short[] { 0, 2, 3 })]
        [InlineData(0x23, new short[] { -285 })]
        public void EncodingTest(byte opcode, short[] args)
        {
            // construct a script with a populated init sub-script & run it through the encoder
            var script = new BattleScript();
            script.Init.Insert(0, new Instruction(Instruction.OpCodes[opcode], args));
            script = new BattleScript(script.Encode());

            // added instruction should still be intact
            Assert.NotNull(script.Init);
            Assert.NotEmpty(script.Init);
            Assert.Equal(opcode, script.Init[0].Op.Code);
            Assert.Equal(args.Length, script.Init[0].Args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                Assert.Equal(args[i], script.Init[0].Args[i]);
            }
            Assert.Equal(0, script.Init[1].Op.Code);

            // everything else should be the same as the empty script
            Assert.NotNull(script.Execute);
            Assert.NotEmpty(script.Execute);
            Assert.Equal(0, script.Execute[0].Op.Code);

            Assert.NotNull(script.Counter);
            Assert.NotEmpty(script.Counter);
            Assert.Equal(0, script.Counter[0].Op.Code);

            Assert.NotNull(script.Death);
            Assert.NotEmpty(script.Death);
            Assert.Equal(0, script.Death[0].Op.Code);

            Assert.NotNull(script.PreCounter);
            Assert.NotEmpty(script.PreCounter);
            Assert.Equal(0, script.PreCounter[0].Op.Code);
        }
    }
}
