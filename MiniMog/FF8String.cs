using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiniMog
{
    class FF8String
    {
        private static readonly char[] readableChars = new char[]
        {
            ' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '%', '/', ':', '!', '?',
            '…', '+', '-', '=', '*', '&', '"', '"', '(', ')', '\'', '.', ',', '~', '”', '“',
            '\'', '#', '$', '`', '_', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K',
            'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a',
            'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q',
            'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public static string Decode(byte[] bytes)
        {
            var result = "";

            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length)
                {
                    var code = (int)reader.ReadByte();
                    if (code == 0) break;

                    switch (code)
                    {
                        case 0x02:
                            result += @"\n";
                            continue;
                        case 0x03:
                            result += "{char " + reader.ReadByte().ToString("x2") + "}";
                            continue;
                        case 0x05:
                            result += "{icon " + reader.ReadByte().ToString("x2") + "}";
                            continue;
                        case 0x06:
                            result += "{col " + reader.ReadByte().ToString("x2") + "}";
                            continue;
                        case 0x0a:
                            result += "{spec " + reader.ReadByte().ToString("x2") + "}";
                            continue;
                        case 0x0b:
                            result += "{cursor " + reader.ReadByte().ToString("x2") + "}";
                            continue;
                        case 0x0c:
                            result += "{spell " + reader.ReadByte().ToString("x2") + "}";
                            continue;
                    }

                    var index = code - 0x20;
                    if (index < 0 || index >= readableChars.Length)
                    {
                        result += "{" + code.ToString("x2") + "}";
                    }
                    else
                    {
                        result += readableChars[index];
                    }
                }
            }
            return result;
        }
    }
}
