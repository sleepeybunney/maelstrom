using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Menu
{
    public class MenuTextPage
    {
        public short Location { get; set; }
        public List<short> Offsets { get; set; } = new List<short>();
        public List<string> Strings { get; set; } = new List<string>();

        public MenuTextPage(short location) { Location = location; }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((short)Offsets.Count));

            var headerSize = Offsets.Count * 2 + 2;
            var encodedStrings = new List<IEnumerable<byte>>();
            var firstOffset = 0;
            var offsetEnum = Offsets.GetEnumerator();
            offsetEnum.MoveNext();

            foreach (var s in Strings)
            {
                // fake offsets
                while (offsetEnum.Current == 0)
                {
                    result.Add(0);
                    result.Add(0);
                    offsetEnum.MoveNext();
                }

                // calculate offset
                var newOffset = headerSize + encodedStrings.Sum(es => es.Count());
                if (firstOffset == 0) firstOffset = newOffset;
                result.AddRange(BitConverter.GetBytes((short)newOffset));
                offsetEnum.MoveNext();

                // put the actual data aside until the header is done
                encodedStrings.Add(FF8String.Encode(s));
            }

            // pad header as required (eg. fake offsets after the last real one)
            while (result.Count < firstOffset) result.Add(0);

            // write text data
            foreach (var es in encodedStrings) result.AddRange(es);

            return result;
        }
    }
}
