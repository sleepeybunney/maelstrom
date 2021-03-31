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

        // drops
        public bool LootDrops { get; set; } = false;
        public bool LootDropsKeyItems { get; set; } = false;
        public bool LootDropsSummonItems { get; set; } = false;
        public bool LootDropsMagazines { get; set; } = false;
        public bool LootDropsChocoboWorld { get; set; } = false;

        // steals
        public bool LootSteals { get; set; } = false;
        public bool LootStealsKeyItems { get; set; } = false;
        public bool LootStealsSummonItems { get; set; } = false;
        public bool LootStealsMagazines { get; set; } = false;
        public bool LootStealsChocoboWorld { get; set; } = false;

        // draws
        public bool LootDraws { get; set; } = false;
        public int LootDrawsAmount { get; set; } = 4;
        public bool LootDrawsApoc { get; set; } = false;
        public bool LootDrawsSlot { get; set; } = false;
        public bool LootDrawsCut { get; set; } = false;

        // abilities
        public bool GfAbilitiesEnable { get; set; } = false;
        public bool GfAbilitiesBasics { get; set; } = false;
        public bool GfAbilitiesIncludeItemOnly { get; set; } = false;
        public bool GfAbilitiesSwapSets { get; set; } = false;
        public int GfAbilitiesLimit { get; set; } = 21;

        // shops
        public bool ShopEnable { get; set; } = false;
        public bool ShopKeyItems { get; set; } = false;
        public bool ShopMagazines { get; set; } = false;
        public bool ShopChocoboWorld { get; set; } = false;
        public bool ShopSummonItems { get; set; } = false;

        // upgrades
        public bool UpgradeEnable { get; set; } = false;

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

        // names
        public bool NameEnable { get; set; } = false;
        public string NameSquall { get; set; } = "Squall";
        public string NameRinoa { get; set; } = "Rinoa";
        public string NameAngelo { get; set; } = "Angelo";
        public string NameQuezacotl { get; set; } = "Quezacotl";
        public string NameShiva { get; set; } = "Shiva";
        public string NameIfrit { get; set; } = "Ifrit";
        public string NameSiren { get; set; } = "Siren";
        public string NameBrothers { get; set; } = "Brothers";
        public string NameDiablos { get; set; } = "Diablos";
        public string NameCarbuncle { get; set; } = "Carbuncle";
        public string NameLeviathan { get; set; } = "Leviathan";
        public string NamePandemona { get; set; } = "Pandemona";
        public string NameCerberus { get; set; } = "Cerberus";
        public string NameAlexander { get; set; } = "Alexander";
        public string NameDoomtrain { get; set; } = "Doomtrain";
        public string NameBahamut { get; set; } = "Bahamut";
        public string NameCactuar { get; set; } = "Cactuar";
        public string NameTonberry { get; set; } = "Tonberry";
        public string NameEden { get; set; } = "Eden";
        public string NameBoko { get; set; } = "Boko";
        public string NameGriever { get; set; } = "Griever";

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

        public State Clone()
        {
            return (State)MemberwiseClone();
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
                state.NameEnable = Current.NameEnable;
                state.NameSquall = Current.NameSquall;
                state.NameRinoa = Current.NameRinoa;
                state.NameAngelo = Current.NameAngelo;
                state.NameQuezacotl = Current.NameQuezacotl;
                state.NameShiva = Current.NameShiva;
                state.NameIfrit = Current.NameIfrit;
                state.NameSiren = Current.NameSiren;
                state.NameBrothers = Current.NameBrothers;
                state.NameDiablos = Current.NameDiablos;
                state.NameCarbuncle = Current.NameCarbuncle;
                state.NameLeviathan = Current.NameLeviathan;
                state.NamePandemona = Current.NamePandemona;
                state.NameCerberus = Current.NameCerberus;
                state.NameAlexander = Current.NameAlexander;
                state.NameDoomtrain = Current.NameDoomtrain;
                state.NameBahamut = Current.NameBahamut;
                state.NameCactuar = Current.NameCactuar;
                state.NameTonberry = Current.NameTonberry;
                state.NameEden = Current.NameEden;
                state.NameBoko = Current.NameBoko;
                state.NameGriever = Current.NameGriever;
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
                state.NameEnable = false;
                state.NameSquall = null;
                state.NameRinoa = null;
                state.NameAngelo = null;
                state.NameQuezacotl = null;
                state.NameShiva = null;
                state.NameIfrit = null;
                state.NameSiren = null;
                state.NameBrothers = null;
                state.NameDiablos = null;
                state.NameCarbuncle = null;
                state.NameLeviathan = null;
                state.NamePandemona = null;
                state.NameCerberus = null;
                state.NameAlexander = null;
                state.NameDoomtrain = null;
                state.NameBahamut = null;
                state.NameCactuar = null;
                state.NameTonberry = null;
                state.NameEden = null;
                state.NameBoko = null;
                state.NameGriever = null;
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
