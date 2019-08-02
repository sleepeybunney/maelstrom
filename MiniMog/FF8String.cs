using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMog
{
    class FF8String
    {
        private static readonly char[] chars = new char[]
        {
            ' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '%', '/', ':', '!', '?',
            '…', '+', '-', '=', '*', '&', '"', '"', '(', ')', '\'', '.', ',', '~', '"', '"',
            '\'', '#', '$', '`', '_', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K',
            'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a',
            'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q',
            'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public static string Decode(byte[] bytes)
        {
            var result = "";
            foreach (var b in bytes)
            {
                var code = (int)b;
                if (code == 0) break;

                var index = code - 0x20;
                if (index < 0 || index >= chars.Length)
                {
                    result += "{" + code.ToString("x2") + "}";
                }
                else
                {
                    result += chars[index];
                }
            }
            return result;
        }
    }
}
