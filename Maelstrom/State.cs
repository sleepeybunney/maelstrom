using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace FF8Mod.Maelstrom
{
    public class State
    {
        public static State Current = new State();

        public string GameLocation { get; set; } = "";
        public bool FreeRoam { get; set; } = false;
        public bool SeedFixed { get; set; } = false;
        public string SeedValue { get; set; } = "";
        public bool SpoilerFile { get; set; } = false;
        public string Language { get; set; } = "eng";
        public string PresetName { get; set; } = null;
        public List<string> History { get; set; } = new List<string>();

        // bosses
        public bool BossEnable { get; set; } = false;
        public bool BossRandom { get; set; } = false;

        // loot
        public bool LootDrops { get; set; } = false;
        public bool LootSteals { get; set; } = false;
        public bool LootDraws { get; set; } = false;

        // abilities
        public bool GfAbilitiesEnable { get; set; } = false;
        public bool GfAbilitiesBasics { get; set; } = true;
        public bool GfAbilitiesIncludeItemOnly { get; set; } = false;

        // shops
        public bool ShopEnable { get; set; } = false;

        // draw points
        public bool DrawPointEnable { get; set; } = false;
        public bool DrawPointIncludeApoc { get; set; } = false;
        public bool DrawPointIncludeSlot { get; set; } = false;
        public bool DrawPointIncludeCut { get; set; } = false;

        // cards
        public bool CardEnable { get; set; } = false;

        // music
        public bool MusicEnable { get; set; } = false;
        public bool MusicIncludeNonMusic { get; set; } = false;

        public override string ToString()
        {
            var result = new StringBuilder();
            if (FreeRoam) result.AppendLine(" - Free Roam");
            if (BossEnable) result.AppendLine(" - Random Bosses");
            if (LootDrops) result.AppendLine(" - Random Drops");
            if (LootSteals) result.AppendLine(" - Random Steals");
            if (LootDraws) result.AppendLine(" - Random Draws");
            if (GfAbilitiesEnable) result.AppendLine(" - Random GF Abilities");
            if (ShopEnable) result.AppendLine(" - Random Shops");
            if (DrawPointEnable) result.AppendLine(" - Random Draw Points");
            if (CardEnable) result.AppendLine(" - Random Rare Cards");
            if (MusicEnable) result.AppendLine(" - Random Music");
            if (result.Length == 0) result.AppendLine(" - Reset All to Normal");
            return result.ToString();
        }

        public static List<State> Presets { get; set; }

        public static State LoadFile(string path, bool preset = true)
        {
            try
            {
                if (!File.Exists(path)) return new State();
                return LoadState(JsonSerializer.Deserialize<State>(File.ReadAllText(path), options), preset);
                
            }
            catch (Exception) { }
            return new State();
        }

        public static State LoadState(State state, bool preset = true)
        {
            if (preset)
            {
                state.GameLocation = Current.GameLocation;
                state.Language = Current.Language;
                state.SpoilerFile = Current.SpoilerFile;
                state.SeedFixed = Current.SeedFixed;
                state.SeedValue = Current.SeedValue;
                state.History = Current.History;
            }
            return state;
        }

        public static void SaveFile(string path, bool preset = true)
        {
            try
            {
                var folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(path, JsonSerializer.Serialize(SaveState(Current, preset), options) + Environment.NewLine);
            }
            catch (Exception) { }
        }

        public static State SaveState(State state, bool preset = true)
        {
            state = (State)state.MemberwiseClone();

            if (preset)
            {
                state.GameLocation = null;
                state.Language = null;
                state.SpoilerFile = false;
                state.SeedFixed = false;
                state.SeedValue = null;
                state.History = null;
            }
            else
            {
                state.PresetName = null;
            }

            return state;
        }

        private static JsonSerializerOptions options = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
}
