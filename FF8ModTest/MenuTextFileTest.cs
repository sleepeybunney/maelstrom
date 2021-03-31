using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Sleepey.FF8Mod.Menu;

namespace Sleepey.FF8ModTest
{
    public class MenuTextFileTest
    {
        [Fact]
        public void EncoderTest()
        {
            // files tend to have 16 pages
            var textFile = new TextFile();
            for (var i = 0; i < 16; i++) textFile.Pages.Add(new MenuTextPage(0));

            // add some strings (locations & offsets don't matter beyond whether or not they're 0)
            textFile.Pages[2].Location = 36;
            for (var i = 0; i < 14; i++) textFile.Pages[2].Offsets.Add(0);
            textFile.Pages[2].Offsets[6] = 1;
            textFile.Pages[2].Offsets[7] = 1;
            textFile.Pages[2].Strings.Add("Squalf");
            textFile.Pages[2].Strings.Add("Doritos");

            textFile.Pages[3].Location = 92;
            for (var i = 0; i < 3; i++) textFile.Pages[3].Offsets.Add(0);
            textFile.Pages[3].Offsets[0] = 1;
            textFile.Pages[3].Offsets[2] = 1;
            textFile.Pages[3].Strings.Add("Rinoa");
            textFile.Pages[3].Strings.Add("Quezacotl");

            // run through the encoder & make sure everything is the same
            textFile = TextFile.FromBytes(textFile.Encode(), false);
            Assert.Equal(16, textFile.Pages.Count);

            // offsets per page
            Assert.Empty(textFile.Pages[0].Offsets);
            Assert.Empty(textFile.Pages[1].Offsets);
            Assert.Equal(14, textFile.Pages[2].Offsets.Count);
            Assert.Equal(3, textFile.Pages[3].Offsets.Count);
            Assert.Empty(textFile.Pages[4].Offsets);
            Assert.Empty(textFile.Pages[5].Offsets);

            // page 2 strings
            Assert.Equal(2, textFile.Pages[2].Strings.Count);
            Assert.Equal("Squalf", textFile.Pages[2].Strings[0]);
            Assert.Equal("Doritos", textFile.Pages[2].Strings[1]);

            // page 2 offsets
            Assert.Equal(0, textFile.Pages[2].Offsets[4]);
            Assert.Equal(0, textFile.Pages[2].Offsets[5]);
            Assert.NotEqual(0, textFile.Pages[2].Offsets[6]);
            Assert.NotEqual(0, textFile.Pages[2].Offsets[7]);
            Assert.Equal(0, textFile.Pages[2].Offsets[8]);
            Assert.Equal(0, textFile.Pages[2].Offsets[9]);

            // page 3 strings
            Assert.Equal(2, textFile.Pages[3].Strings.Count);
            Assert.Equal("Rinoa", textFile.Pages[3].Strings[0]);
            Assert.Equal("Quezacotl", textFile.Pages[3].Strings[1]);

            // page 3 offsets
            Assert.NotEqual(0, textFile.Pages[3].Offsets[0]);
            Assert.Equal(0, textFile.Pages[3].Offsets[1]);
            Assert.NotEqual(0, textFile.Pages[3].Offsets[2]);
        }
    }
}
