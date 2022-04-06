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
    public class IndexEntryTest
    {
        [Fact]
        public void IndexEntryMinTest()
        {
            var entry = new IndexEntry(0, 0, 0);
            Assert.Equal(0U, entry.Length);
            Assert.Equal(0U, entry.Location);
            Assert.Equal(0U, entry.Compression);
        }

        [Fact]
        public void IndexEntryMaxTest()
        {
            var entry = new IndexEntry(uint.MaxValue, uint.MaxValue, 2);
            Assert.Equal(uint.MaxValue, entry.Length);
            Assert.Equal(uint.MaxValue, entry.Location);
            Assert.Equal(2U, entry.Compression);
        }

        [Fact]
        public void IndexEntryUnknownCompressionTest() => Assert.Throws<InvalidDataException>(() => new IndexEntry(0, 0, 3));

        [Fact]
        public void IndexEntryNullBytesTest() => Assert.Throws<ArgumentNullException>(() => new IndexEntry(null));

        [Fact]
        public void IndexEntryEmptyBytesTest() => Assert.Throws<InvalidDataException>(() => new IndexEntry(new byte[] { }));

        [Fact]
        public void IndexEntryShortBytesTest() => Assert.Throws<InvalidDataException>(() => new IndexEntry(new byte[11]));

        [Fact]
        public void IndexEntryLongBytesTest() => Assert.Throws<InvalidDataException>(() => new IndexEntry(new byte[13]));

        [Fact]
        public void IndexEntryMinBytesTest()
        {
            var entry = new IndexEntry(new byte[12]);
            Assert.Equal(0U, entry.Length);
            Assert.Equal(0U, entry.Location);
            Assert.Equal(0U, entry.Compression);
        }

        [Fact]
        public void IndexEntryMaxBytesTest()
        {
            var bytes = new byte[12];
            for (int i = 0; i < 8; i++) bytes[i] = byte.MaxValue;
            bytes[8] = 2;
            var entry = new IndexEntry(bytes);
            Assert.Equal(uint.MaxValue, entry.Length);
            Assert.Equal(uint.MaxValue, entry.Location);
            Assert.Equal(2U, entry.Compression);
        }

        [Fact]
        public void IndexEntryUnknownCompressionBytesTest()
        {
            var bytes = new byte[12];
            bytes[8] = 3;
            Assert.Throws<InvalidDataException>(() => new IndexEntry(bytes));
        }
    }
}
