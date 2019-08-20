using System;
using System.IO;
using System.Linq;

namespace FF8Mod
{
    public class BinaryPatch
    {
        public string TargetFile;
        public uint Offset;
        public byte[] OriginalData;
        public byte[] NewData;

        public BinaryPatch()
        {
            TargetFile = null;
            Offset = 0;
            OriginalData = new byte[0];
            NewData = new byte[0];
        }

        public BinaryPatch(uint offset, byte[] origData, byte[] newData) : this()
        {
            Offset = offset;
            OriginalData = origData;
            NewData = newData;
        }

        public BinaryPatch(string targetFile, uint offset, byte[] origData, byte[] newData) : this()
        {
            TargetFile = targetFile;
            Offset = offset;
            OriginalData = origData;
            NewData = newData;
        }

        public void Apply(string targetFile)
        {
            TargetFile = targetFile;
            Patch(false);
        }

        public void Apply() { Patch(false); }

        public void Remove(string targetFile)
        {
            TargetFile = targetFile;
            Patch(true);
        }

        public void Remove() { Patch(true); }

        private void Patch(bool remove)
        {
            using (var stream = new FileStream(TargetFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            using (var reader = new BinaryReader(stream))
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(Offset, SeekOrigin.Begin);
                var origCheck = reader.ReadBytes(OriginalData.Length);
                if (!origCheck.SequenceEqual(OriginalData))
                {
                    stream.Seek(Offset, SeekOrigin.Begin);
                    var newCheck = reader.ReadBytes(NewData.Length);
                    if (!newCheck.SequenceEqual(NewData))
                    {
                        // the file is either patched or unpatched - if it looks like neither version, something is wrong
                        throw new Exception("Binary file contents are not as expected - " + TargetFile);
                    }
                    else if (remove)
                    {
                        // remove patch
                        stream.Seek(Offset, SeekOrigin.Begin);
                        writer.Write(OriginalData);
                    }
                    else
                    {
                        // already applied
                        return;
                    }
                }
                else if (!remove)
                {
                    // apply patch
                    stream.Seek(Offset, SeekOrigin.Begin);
                    writer.Write(NewData);
                }
            }
        }
    }
}
