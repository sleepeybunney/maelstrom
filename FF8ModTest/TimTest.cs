using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Sleepey.FF8Mod;

namespace Sleepey.FF8ModTest
{
    public class TimTest
    {
        [Theory]
        [InlineData(14404)]
        [InlineData(25286)]
        [InlineData(42694)]
        [InlineData(50597)]
        public void ColourDecodeEncodeTest(ushort colour) => Assert.Equal(colour, new TimColour(colour).Encode());

        [Fact]
        public void ClutDecodeEncodeTest()
        {
            var clut = new TimClut(16, 32, 3, 3);
            clut.Data[0, 0] = new TimColour(6589);
            clut.Data[0, 1] = new TimColour(34521);
            clut.Data[0, 2] = new TimColour(12345);
            clut.Data[1, 0] = new TimColour(14834);
            clut.Data[1, 1] = new TimColour(62347);
            clut.Data[1, 2] = new TimColour(63232);
            clut.Data[2, 0] = new TimColour(21674);
            clut.Data[2, 1] = new TimColour(52402);
            clut.Data[2, 2] = new TimColour(13959);

            var encoded = clut.Encode();
            Assert.Equal(encoded, new TimClut(encoded).Encode());
        }

        [Fact]
        public void TimDecodeEncodeTest()
        {
            var clut = new TimClut(32, 64, 2, 2);
            clut.Data[0, 0] = new TimColour(38855);
            clut.Data[0, 1] = new TimColour(60455);
            clut.Data[1, 0] = new TimColour(8529);
            clut.Data[1, 1] = new TimColour(39274);

            var image = new byte[128];
            image[0] = 128;

            var tim = new Tim(1, clut, image);

            var encoded = tim.Encode();
            Assert.Equal(encoded, new Tim(encoded).Encode());
        }
    }
}
