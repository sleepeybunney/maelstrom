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

        public static void Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                Current = JsonSerializer.Deserialize<State>(File.ReadAllText(path), options);
            }
            catch (Exception) { }
        }

        public static void Save(string path)
        {
            try
            {
                var folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(path, JsonSerializer.Serialize(Current, options));
            }
            catch (Exception) { }
        }

        private static JsonSerializerOptions options = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };
    }
}
