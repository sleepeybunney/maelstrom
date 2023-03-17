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
        public void ColourDecodeEncodeTest(ushort colour) => Assert.Equal(colour, new Colour(colour).Encode());

        [Fact]
        public void ClutDecodeEncodeTest()
        {
            var clut = new Clut(16, 32, 3, 3);
            clut.Data[0, 0] = new Colour(6589);
            clut.Data[0, 1] = new Colour(34521);
            clut.Data[0, 2] = new Colour(12345);
            clut.Data[1, 0] = new Colour(14834);
            clut.Data[1, 1] = new Colour(62347);
            clut.Data[1, 2] = new Colour(63232);
            clut.Data[2, 0] = new Colour(21674);
            clut.Data[2, 1] = new Colour(52402);
            clut.Data[2, 2] = new Colour(13959);

            var encoded = clut.Encode();
            Assert.Equal(encoded, new Clut(encoded).Encode());
        }

        [Fact]
        public void TimDecodeEncodeTest()
        {
            var clut = new Clut(32, 64, 2, 2);
            clut.Data[0, 0] = new Colour(38855);
            clut.Data[0, 1] = new Colour(60455);
            clut.Data[1, 0] = new Colour(8529);
            clut.Data[1, 1] = new Colour(39274);

            var image = new byte[128];
            image[0] = 128;

            var tim = new Tim(1, clut, image);

            var encoded = tim.Encode();
            Assert.Equal(encoded, new Tim(encoded).Encode());
        }
    }
}
