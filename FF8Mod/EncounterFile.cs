using System.Collections.Generic;
using System.IO;

namespace FF8Mod
{
    public class EncounterFile
    {
        public List<Encounter> Encounters;

        public EncounterFile()
        {
            Encounters = new List<Encounter>();
        }

        public static EncounterFile FromBytes(byte[] data)
        {
            var result = new EncounterFile();

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position + 127 < stream.Length)
                {
                    result.Encounters.Add(new Encounter(reader.ReadBytes(128)));
                }
            }

            return result;
        }

        public static EncounterFile FromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Encounter file not found");
            }

            return FromBytes(File.ReadAllBytes(path));
        }

        public static EncounterFile FromSource(FileSource source, string path)
        {
            return FromBytes(source.GetFile(path));
        }

        public byte[] Encoded
        {
            get
            {
                var result = new List<byte>();
                foreach (var e in Encounters) result.AddRange(e.Encoded);
                return result.ToArray();
            }
        }
    }
}
