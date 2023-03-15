using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.Maelstrom
{
    public static class MagicSortFix
    {
        public static void Apply(FileSource menuSource)
        {
            menuSource.ReplaceFile(Env.MagsortPath, GenerateSort());
        }

        public static byte[] GenerateSort()
        {
            var attack = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x36, 0x11, 0x12, 0x0E, 0x0F, 0x10, 0x13, 0x14, 0x37 };
            var restore = new byte[] { 0x15, 0x16, 0x17, 0x33, 0x18, 0x19, 0x1A, 0x1B };
            var indirect = new byte[] { 0x32, 0x28, 0x26, 0x29, 0x27, 0x2E, 0x2A, 0x30, 0x2B, 0x21, 0x22, 0x1C, 0x1D, 0x1E, 0x34, 0x1F, 0x2F, 0x2C, 0x23, 0x24, 0x25, 0x31, 0x2D, 0x20, 0x35, 0x38 };

            var blockSize = 64;
            var spellCount = attack.Length + restore.Length + indirect.Length;

            var padding = new List<byte>();
            for (int i = spellCount; i < blockSize; i++) padding.Add(0);

            var manual = new List<byte>();
            for (int i = 0; i < blockSize; i++) manual.Add(0);

            var result = new List<byte>();
            result.AddRange(manual);

            result.AddRange(attack);
            result.AddRange(restore);
            result.AddRange(indirect);
            result.AddRange(padding);

            result.AddRange(attack);
            result.AddRange(indirect);
            result.AddRange(restore);
            result.AddRange(padding);

            result.AddRange(restore);
            result.AddRange(attack);
            result.AddRange(indirect);
            result.AddRange(padding);

            result.AddRange(restore);
            result.AddRange(indirect);
            result.AddRange(attack);
            result.AddRange(padding);

            result.AddRange(indirect);
            result.AddRange(attack);
            result.AddRange(restore);
            result.AddRange(padding);

            result.AddRange(indirect);
            result.AddRange(restore);
            result.AddRange(attack);
            result.AddRange(padding);

            return result.ToArray();
        }
    }
}
