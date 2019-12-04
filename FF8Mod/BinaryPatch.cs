using System;
using System.IO;
using System.Linq;

namespace FF8Mod
{
    public class BinaryPatch
    {
        public uint Offset;
        public byte[] OriginalData;
        public byte[] NewData;

        public BinaryPatch()
        {
            Offset = 0;
            OriginalData = Array.Empty<byte>();
            NewData = Array.Empty<byte>();
        }

        public BinaryPatch(uint offset, byte[] origData, byte[] newData)
        {
            Offset = offset;
            OriginalData = origData;
            NewData = newData;
        }

        public void Apply(string targetFile)
        {
            Patch(targetFile, false);
        }

        public void Remove(string targetFile)
        {
            Patch(targetFile, true);
        }

        private void Patch(string targetFile, bool remove)
        {
            using (var stream = new FileStream(targetFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            using (var reader = new BinaryReader(stream))
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(Offset, SeekOrigin.Begin);

                if (remove)
                {
                    // remove patch
                    writer.Write(OriginalData);
                }
                else
                {
                    // apply patch
                    writer.Write(NewData);
                }
            }
        }
    }
}
