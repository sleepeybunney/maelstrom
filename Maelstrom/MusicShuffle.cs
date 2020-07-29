using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using FF8Mod.Archive;
using FF8Mod.Field;
using System.Diagnostics;

namespace FF8Mod.Maelstrom
{
    public static class MusicShuffle
    {
        public static List<MusicLoad> MusicLoads = JsonSerializer.Deserialize<List<MusicLoad>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.MusicLoads.json"));
        public static List<MusicTrack> MusicTracks = JsonSerializer.Deserialize<List<MusicTrack>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.MusicTracks.json"));

        public static Dictionary<int, int> Shuffle(int seed)
        {
            var random = new Random(seed);
            var result = new Dictionary<int, int>();

            var trackIds = MusicTracks.Select(t => t.TrackID).ToList();
            var trackCount = MusicTracks.Count;

            foreach (var t in trackIds) result.Add(t, trackIds[random.Next(0, trackCount)]);

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
                                field.Entities[s.Item2].Scripts[s.Item3].Instructions[i - 1].Param = shuffle[prevParam];
                            }
                        }
                    }
                }
                StorySkip.SaveToSource(fieldSource, fieldName, field.Encode()); // todo: this still needs moving
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
    }
}
