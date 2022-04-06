using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Sleepey.FF8Mod;
using System.Collections;
using System.IO;

namespace Sleepey.FF8ModTest
{
    public class FF8StringTest
    {
        public static TheoryData<string, byte[]> Names => new TheoryData<string, byte[]>
        {
            { "Squall", new byte[] { 87, 111, 115, 95, 106, 106, 0 } },
            { "Rinoa", new byte[] { 86, 103, 108, 109, 95, 0 } },
            { "Quezacotl", new byte[] { 85, 115, 99, 120, 95, 97, 109, 114, 106, 0 } },
            { "Brothers", new byte[] { 70, 112, 109, 114, 102, 99, 112, 113, 0 } },
            { "Griever", new byte[] { 75, 112, 103, 99, 116, 99, 112, 0 } },
            { "X-ATM092", new byte[] { 92, 50, 69, 88, 81, 33, 42, 35, 0 } },
            { "Ultima Weapon", new byte[] { 89, 106, 114, 103, 107, 95, 32, 91, 99, 95, 110, 109, 108, 0 } },
            { "“Sorceress”", new byte[] { 63, 87, 109, 112, 97, 99, 112, 99, 113, 113, 62, 0 } },
        };

        public static TheoryData<string> MissingBraceStrings => new TheoryData<string>
        {
            "{08",
            "01}",
            "Squall{0e",
            "Shiva74}",
            "{99Tonberry",
            "60}NORG",
            "{{8a}",
            "{ee}}"
        };

        public static TheoryData<string> InvalidCodeStrings => new TheoryData<string>
        {
            "{00f}",
            "{Ifrit}",
            "{{20}}"
        };

        [Fact]
        public void DecodeNullTest() => Assert.Throws<ArgumentNullException>(() => FF8String.Decode(null));

        [Fact]
        public void DecodeEmptyTest() => Assert.Empty(FF8String.Decode(new byte[] { }));

        [Theory]
        [MemberData(nameof(Names))]
        public void DecodeNamesTest(string name, IEnumerable<byte> bytes) => Assert.Equal(name, FF8String.Decode(bytes));

        [Fact]
        public void DecodeUnterminatedTest() => Assert.Equal("Ultima Weapon", FF8String.Decode(new byte[] { 89, 106, 114, 103, 107, 95, 32, 91, 99, 95, 110, 109, 108 }));

        [Fact]
        public void DecodeEarlyTerminatedTest() => Assert.Equal("Ultima Weapon", FF8String.Decode(new byte[] { 89, 106, 114, 103, 107, 95, 32, 91, 99, 95, 110, 109, 108, 0, 75, 112, 103, 99, 116, 99, 112, 0 }));

        [Fact]
        public void DecodeUnprintableTest() => Assert.Equal("{07} {13}{7e}", FF8String.Decode(new byte[] { 7, 32, 19, 126, 0 }));

        [Fact]
        public void EncodeNullTest() => Assert.Throws<ArgumentNullException>(() => FF8String.Encode(null));

        [Fact]
        public void EncodeEmptyTest() => Assert.Equal(new byte[] { 0 }, FF8String.Encode(string.Empty));

        [Theory]
        [MemberData(nameof(Names))]
        public void EncodeNamesTest(string name, IEnumerable<byte> bytes) => Assert.Equal(bytes, FF8String.Encode(name));

        [Fact]
        public void EncodeUnprintableTest() => Assert.Equal(new byte[] { 8, 73, 118, 201, 0 }, FF8String.Encode("{08}{49}x{c9}"));

        [Theory]
        [MemberData(nameof(MissingBraceStrings))]
        public void EncodeMissingClosingBraceTest(string str) => Assert.Throws<InvalidDataException>(() => FF8String.Encode(str));

        [Theory]
        [MemberData(nameof(InvalidCodeStrings))]
        public void EncodeInvalidCodeTest(string str) => Assert.Throws<InvalidDataException>(() => FF8String.Encode(str));

        [Fact]
        public void EncodeSingleDigitCodeTest() => Assert.Equal(new byte[] { 10, 0 }, FF8String.Encode("{a}"));

        [Fact]
        public void EncodeUnknownCharacterTest() => Assert.Throws<InvalidDataException>(() => FF8String.Encode("¬"));

        [Fact]
        public void EncodeDecodeInverseTest() => Assert.Equal("asdf'&.{0d}x  ~", FF8String.Decode(FF8String.Encode("asdf'&.{0d}x  ~")));
    }
}
