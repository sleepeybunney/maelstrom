using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FF8Mod;
using Xunit;

namespace FF8ModTest
{
    public class EncounterTest
    {
        [Fact]
        public void EncoderTest()
        {
            // construct an encounter
            var enc = new Encounter()
            {
                Scene = 31,
                NoEscape = true,
                ScriptedBattle = true,
                NoExp = true,
                MainCamera = 1,
                SecondaryCamera = 2,
                SecondaryCameraAnimation = 3
            };

            enc.Slots[0] = new EncounterSlot()
            {
                MonsterID = 77,
                Level = 6,
                Position = new EncounterSlot.Coords(200, 0, -2100),
                Enabled = true,
                Unknown2 = 209
            };

            // run it through the encoder & make sure everything's the same
            enc = new Encounter(enc.Encoded);

            Assert.Equal(31, enc.Scene);
            Assert.True(enc.NoEscape);
            Assert.True(enc.ScriptedBattle);
            Assert.True(enc.NoExp);
            Assert.False(enc.BackAttack);
            Assert.False(enc.NoResults);
            Assert.False(enc.NoVictorySequence);
            Assert.False(enc.ShowTimer);
            Assert.False(enc.StruckFirst);
            Assert.Equal(1, enc.MainCamera);
            Assert.Equal(0, enc.MainCameraAnimation);
            Assert.Equal(2, enc.SecondaryCamera);
            Assert.Equal(3, enc.SecondaryCameraAnimation);
            Assert.Equal(8, enc.Slots.Length);

            // first slot
            Assert.Equal(77, enc.Slots[0].MonsterID);
            Assert.Equal(6, enc.Slots[0].Level);
            Assert.Equal(200, enc.Slots[0].Position.X);
            Assert.Equal(0, enc.Slots[0].Position.Y);
            Assert.Equal(-2100, enc.Slots[0].Position.Z);
            Assert.True(enc.Slots[0].Enabled);
            Assert.False(enc.Slots[0].Hidden);
            Assert.False(enc.Slots[0].Unloaded);
            Assert.False(enc.Slots[0].Untargetable);
            Assert.Equal(0, enc.Slots[0].Unknown1);
            Assert.Equal(209, enc.Slots[0].Unknown2);
            Assert.Equal(0, enc.Slots[0].Unknown3);
            Assert.Equal(0, enc.Slots[0].Unknown4);

            // second slot
            Assert.Equal(0, enc.Slots[1].MonsterID);
            Assert.Equal(255, enc.Slots[1].Level);
            Assert.Equal(0, enc.Slots[1].Position.X);
            Assert.Equal(0, enc.Slots[1].Position.Y);
            Assert.Equal(-5700, enc.Slots[1].Position.Z);
            Assert.False(enc.Slots[1].Enabled);
            Assert.False(enc.Slots[1].Hidden);
            Assert.False(enc.Slots[1].Unloaded);
            Assert.False(enc.Slots[1].Untargetable);
            Assert.Equal(0, enc.Slots[1].Unknown1);
            Assert.Equal(0, enc.Slots[1].Unknown2);
            Assert.Equal(0, enc.Slots[1].Unknown3);
            Assert.Equal(0, enc.Slots[1].Unknown4);

            // third slot
            Assert.Equal(0, enc.Slots[2].MonsterID);
            Assert.Equal(255, enc.Slots[2].Level);
            Assert.Equal(0, enc.Slots[2].Position.X);
            Assert.Equal(0, enc.Slots[2].Position.Y);
            Assert.Equal(-5700, enc.Slots[2].Position.Z);
            Assert.False(enc.Slots[2].Enabled);
            Assert.False(enc.Slots[2].Hidden);
            Assert.False(enc.Slots[2].Unloaded);
            Assert.False(enc.Slots[2].Untargetable);
            Assert.Equal(0, enc.Slots[2].Unknown1);
            Assert.Equal(0, enc.Slots[2].Unknown2);
            Assert.Equal(0, enc.Slots[2].Unknown3);
            Assert.Equal(0, enc.Slots[2].Unknown4);

            // fourth slot
            Assert.Equal(0, enc.Slots[3].MonsterID);
            Assert.Equal(255, enc.Slots[3].Level);
            Assert.Equal(0, enc.Slots[3].Position.X);
            Assert.Equal(0, enc.Slots[3].Position.Y);
            Assert.Equal(-5700, enc.Slots[3].Position.Z);
            Assert.False(enc.Slots[3].Enabled);
            Assert.False(enc.Slots[3].Hidden);
            Assert.False(enc.Slots[3].Unloaded);
            Assert.False(enc.Slots[3].Untargetable);
            Assert.Equal(0, enc.Slots[3].Unknown1);
            Assert.Equal(0, enc.Slots[3].Unknown2);
            Assert.Equal(0, enc.Slots[3].Unknown3);
            Assert.Equal(0, enc.Slots[3].Unknown4);
        }
    }
}
