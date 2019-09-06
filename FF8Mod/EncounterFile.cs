using System.Collections.Generic;
using System.IO;
using FF8Mod.Archive;

namespace FF8Mod
{
    public class EncounterFile
    {
        public static string Path = @"c:\ff8\data\eng\battle\scene.out";

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

        public static EncounterFile FromSource(FileSource source)
        {
            return FromSource(source, Path);
        }

        public byte[] Encode()
        {
            var result = new List<byte>();
            foreach (var e in Encounters) result.AddRange(e.Encode());
            return result.ToArray();
        }
    }
}
