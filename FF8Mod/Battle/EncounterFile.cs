using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod.Battle
{
    public class EncounterFile
    {
        public List<Encounter> Encounters { get; set; } = new List<Encounter>();

        public EncounterFile() { }

        public static EncounterFile FromBytes(IEnumerable<byte> data)
        {
            var result = new EncounterFile();

            using (var stream = new MemoryStream(data.ToArray()))
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
            return FromSource(source, Env.EncounterFilePath);
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            foreach (var e in Encounters) result.AddRange(e.Encode());
            return result;
        }
    }
}
