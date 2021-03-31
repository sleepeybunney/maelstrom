using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod
{
    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, IEnumerable<byte> buffer)
        {
            writer.Write(buffer.ToArray());
        }
    }
}
