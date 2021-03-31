using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Menu
{
    public class TextFile
    {
        public List<MenuTextPage> Pages { get; set; } = new List<MenuTextPage>();
        public int FixedLength { get; set; } = -1;

        public TextFile() { }

        public static TextFile FromBytes(IEnumerable<byte> data, bool fixedLength)
        {
            var result = new TextFile();

            if (fixedLength) result.FixedLength = data.Count();

            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                // enumerate pages
                var pageCount = reader.ReadInt16();
                for (var i = 0; i < pageCount; i++)
                {
                    result.Pages.Add(new MenuTextPage(reader.ReadInt16()));
                }

                // read pages
                foreach (var page in result.Pages)
                {
                    // pages listed at location 0 aren't real
                    if (page.Location == 0) continue;

                    // enumerate string offsets
                    stream.Seek(page.Location, SeekOrigin.Begin);
                    var offsetCount = reader.ReadInt16();
                    for (var i = 0; i < offsetCount; i++)
                    {
                        page.Offsets.Add(reader.ReadInt16());
                    }

                    // read strings
                    foreach (var offset in page.Offsets)
                    {
                        // strings listed at offset 0 aren't real
                        if (offset == 0) continue;

                        stream.Seek(page.Location + offset, SeekOrigin.Begin);
                        var stringBytes = new List<byte>();
                        var nextByte = byte.MaxValue;
                        while (nextByte != 0)
                        {
                            nextByte = reader.ReadByte();
                            stringBytes.Add(nextByte);
                        }

                        page.Strings.Add(FF8String.Decode(stringBytes));
                    }
                }
            }

            return result;
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((short)Pages.Count));

            var headerSize = Pages.Count * 2 + 2;
            var encodedPages = new List<IEnumerable<byte>>();
            var firstLocation = 0;

            foreach (var p in Pages)
            {
                // fill in any fake pages
                if (p.Location == 0)
                {
                    result.Add(0);
                    result.Add(0);
                    continue;
                }

                // calculate location
                var newLocation = headerSize + encodedPages.Sum(ep => ep.Count());
                if (firstLocation == 0) firstLocation = newLocation;
                result.AddRange(BitConverter.GetBytes((short)newLocation));

                // put the actual data aside until the header is done
                encodedPages.Add(p.Encode());
            }

            // pad header as required (eg. fake pages after the last real one)
            while (result.Count < firstLocation) result.Add(0);

            // write page data
            foreach (var ep in encodedPages) result.AddRange(ep);

            // expand or truncate to the proper length, if one has been set
            if (FixedLength != -1)
            {
                while (result.Count < FixedLength) result.Add(0);
                result = result.GetRange(0, FixedLength);
            }

            return result;
        }
    }
}
