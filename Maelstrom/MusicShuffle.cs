using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Exe;
using Sleepey.FF8Mod.Field;

namespace Sleepey.Maelstrom
{
    public static class MusicShuffle
    {
        public static List<MusicLoad> MusicLoads = JsonSerializer.Deserialize<List<MusicLoad>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.MusicLoads.json"));
        public static List<MusicTrack> MusicTracks = JsonSerializer.Deserialize<List<MusicTrack>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.MusicTracks.json"));

        /*
         * jae $+15
         * call $+0x6fc0e7
         * ...
         */

        public static byte[] Caller = new byte[] { 0x73, 0x0D, 0xE8, 0xE2, 0xC0, 0x6F, 0x00, 0x8B, 0x04, 0x85, 0x58, 0xF7, 0xB7, 0x00, 0xC3, 0x33, 0xC0, 0xC3 };
        public static byte[] CallerOrig = new byte[] { 0x73, 0x08, 0x8B, 0x04, 0x85, 0x58, 0xF7, 0xB7, 0x00, 0xC3, 0x33, 0xC0, 0xC3, 0x90, 0x90, 0x90, 0x90, 0x90 };
        public const int CallerLocation = 0x06c857;

        /*
         * push ebx
         * call $+5
         * pop ebx
         * add ebx, 0x1a
         * movzx eax, byte ptr [ebx+eax]
         * pop ebx
         * ret
         */

        public static byte[] Mapper = new byte[] { 0x53, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x5B, 0x83, 0xC3, 0x1A, 0x0F, 0xB6, 0x04, 0x03, 0x5B, 0xC3 };
        public const int MapperLocation = 0x768940;

        public static Dictionary<int, int> Randomise(int seed, State settings)
        {
            var random = new Random(seed + 10);
            var result = new Dictionary<int, int>();

            // hard-coded songs can't be matched with non-music tracks
            var matchableTracks = MusicTracks.Where(t => !t.NonMusic).ToList();
            foreach (var t in MusicTracks.Where(t => t.HardCoded))
            {
                result.Add(t.TrackID, matchableTracks[random.Next(matchableTracks.Count)].TrackID);
            }

            // everything else depends on settings
            matchableTracks = MusicTracks.Where(t => !t.NonMusic || settings.MusicIncludeNonMusic).ToList();
            foreach (var t in matchableTracks.Where(t => !t.HardCoded))
            {
                result.Add(t.TrackID, matchableTracks[random.Next(matchableTracks.Count)].TrackID);
            }

            // leave "julia" unshuffled to avoid problems in laguna scene
            result[22] = 22;

            // generate seed to randomise battle music later (spoiler file won't be accurate)
            if (settings.MusicBattleChange) result[-1] = random.Next();

            return result;
        }

        public static void ApplyPatch(Dictionary<int, int> shuffle)
        {
            var shuffler = new byte[0x95];
            Array.Copy(Mapper, shuffler, Mapper.Length);
            foreach (var k in shuffle.Keys) shuffler[k + 0x20] = (byte)shuffle[k];

            var shufflerOrig = new byte[0x95];

            var callerPatch = new BinaryPatch(CallerLocation, CallerOrig, Caller);
            var shufflerPatch = new BinaryPatch(MapperLocation, shufflerOrig, shuffler);

            callerPatch.Apply(Env.ExePath);
            shufflerPatch.Apply(Env.ExePath);
        }

        public static void RemovePatch()
        {
            var shufflerOrig = new byte[0x95];

            var callerPatch = new BinaryPatch(CallerLocation, CallerOrig, CallerOrig);
            var shufflerPatch = new BinaryPatch(MapperLocation, shufflerOrig, shufflerOrig);

            callerPatch.Remove(Env.ExePath);
            shufflerPatch.Remove(Env.ExePath);
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
        public bool HardCoded { get; set; } = false;
    }
}
