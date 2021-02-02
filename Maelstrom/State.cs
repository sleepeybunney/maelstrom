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
        public string BossLocations { get; set; } = "Normal";
        public string LootDrops { get; set; } = "Normal";
        public string LootSteals { get; set; } = "Normal";
        public string GfAbilities { get; set; } = "Normal";
        public string ShopItems { get; set; } = "Normal";
        public string DrawPointSpells { get; set; } = "Normal";
        public string CardLocations { get; set; } = "Normal";
        public string Music { get; set; } = "Normal";
        public string PresetName { get; set; } = null;

        public override string ToString()
        {
            var result = new StringBuilder();
            if (FreeRoam) result.AppendLine(" - Free Roam");
            if (BossLocations != "Normal") result.AppendLine(" - " + BossLocations + " Bosses");
            if (LootDrops != "Normal") result.AppendLine(" - " + LootDrops + " Loot Drops");
            if (LootSteals != "Normal") result.AppendLine(" - " + LootSteals + " Loot Steals");
            if (GfAbilities != "Normal") result.AppendLine(" - " + GfAbilities + " GF Abilities");
            if (ShopItems != "Normal") result.AppendLine(" - " + ShopItems + " Shops");
            if (DrawPointSpells != "Normal") result.AppendLine(" - " + DrawPointSpells + " Draw Points");
            if (CardLocations != "Normal") result.AppendLine(" - " + CardLocations + " Cards");
            if (Music != "Normal") result.AppendLine(" - " + Music + " Music");
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
                state.SeedFixed = Current.SeedFixed;
                state.SeedValue = Current.SeedValue;
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
                state.SeedFixed = false;
                state.SeedValue = null;
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
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        };
    }
}
