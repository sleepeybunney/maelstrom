using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Field;
using System.Diagnostics;

namespace Sleepey.Maelstrom
{
    public static class MusicShuffle
    {
        public static List<MusicLoad> MusicLoads = JsonSerializer.Deserialize<List<MusicLoad>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.MusicLoads.json"));
        public static List<MusicTrack> MusicTracks = JsonSerializer.Deserialize<List<MusicTrack>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.MusicTracks.json"));

        public static Dictionary<int, int> Randomise(int seed, State settings)
        {
            var random = new Random(seed + 10);
            var result = new Dictionary<int, int>();

            var trackIds = MusicTracks.Where(t => !t.NonMusic || settings.MusicIncludeNonMusic).Select(t => t.TrackID).ToList();
            var trackCount = trackIds.Count;

            foreach (var t in trackIds) result.Add(t, trackIds[random.Next(trackCount)]);

            // leave "julia" unshuffled to avoid problems in laguna scene
            result[22] = 22;

            // generate seed to randomise battle music later (spoiler file won't be accurate)
            if (settings.MusicBattleChange) result[-1] = random.Next();

            return result;
        }

        public static void Apply(FileSource fieldSource, Dictionary<int, int> shuffle)
        {
            var musicOps = new int[] { FieldScript.OpCodesReverse["setbattlemusic"], FieldScript.OpCodesReverse["musicload"] };

            // load list of scripts to search for music changes
            var scripts = MusicLoads.Select(m => new Tuple<string, int, int>(m.FieldName, m.Entity, m.Script)).ToList();

            // add any extra changes from free roam boss clouds
            scripts.AddRange(Boss.Bosses.Where(b => !string.IsNullOrEmpty(b.FieldID)).Select(b => new Tuple<string, int, int>(b.FieldID, b.FieldEntity, b.FieldScript)));

            // remove duplicates
            scripts = scripts.Distinct().ToList();

            var random = new Random(shuffle.ContainsKey(-1) ? shuffle[-1] : 0);
            var values = shuffle.ToList().OrderBy(x => x.Key).Select(x => x.Value).ToList();

            // search all these scripts & replace the music IDs with random ones
            foreach (var fieldName in scripts.Select(s => s.Item1).Distinct())
            {
                var field = FieldScript.FromSource(fieldSource, fieldName);

                foreach (var s in scripts.Where(s => s.Item1 == fieldName))
                {
                    var script = field.Entities[s.Item2].Scripts[s.Item3];
                    for (int i = 0; i < script.Instructions.Count; i++)
                    {
                        if (musicOps.Contains(script.Instructions[i].OpCode) && i > 0)
                        {
                            // update the previous instruction (where the track ID is pushed onto the stack)
                            var prevParam = script.Instructions[i - 1].Param;
                            if (shuffle.ContainsKey(prevParam))
                            {
                                var newParam = shuffle[prevParam];

                                if (shuffle.ContainsKey(-1) && script.Instructions[i].OpCode == FieldScript.OpCodesReverse["setbattlemusic"])
                                {
                                    // re-randomise battle music
                                    newParam = values[random.Next(values.Count)];
                                }

                                field.Entities[s.Item2].Scripts[s.Item3].Instructions[i - 1].Param = newParam;
                            }
                        }
                    }
                }

                field.SaveToSource(fieldSource, fieldName);
            }
        }
    }

    public class MusicLoad
    {
        public string OpCode { get; set; }
        public string FieldName { get; set; }
        public int Entity { get; set; }
        public int Script { get; set; }
        public int Line { get; set; }
        public int Arg { get; set; }
    }

    public class MusicTrack
    {
        public int TrackID { get; set; }
        public string TrackName { get; set; }
        public bool NonMusic { get; set; } = false;
    }
}
