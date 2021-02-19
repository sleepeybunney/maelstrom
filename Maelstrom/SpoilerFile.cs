using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Main;
using Sleepey.FF8Mod.Menu;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.Maelstrom
{
    class SpoilerFile
    {
        private Section Title, Options, Bosses, DrawPoints, Shops, Cards, Loot, Music, Abilities, Weapons;

        public SpoilerFile()
        {
            Title = new Section();
            Title.Heading("Maelstrom Spoiler File", '=');

            Options = new Section();
            Options.Heading("Options");

            var settings = State.Current;
            Options.Bullet("Mode", ModeString(settings.FreeRoam));
            Options.Bullet("Seed", settings.SeedValue.ToString());
            Options.Bullet("Bosses", GeneralString(settings.BossEnable));
            Options.Bullet("Draw Points", DrawPointsString(settings.DrawPointEnable, settings.DrawPointIncludeApoc, settings.DrawPointIncludeSlot, settings.DrawPointIncludeCut));
            Options.Bullet("Shops", ShopString(settings.ShopEnable, settings.ShopKeyItems, settings.ShopSummonItems, settings.ShopMagazines, settings.ShopChocoboWorld));
            Options.Bullet("Cards", GeneralString(settings.CardEnable));
            Options.Bullet("Loot", LootString(settings.LootDrops, settings.LootSteals, settings.LootDraws));
            Options.Bullet("Music", GeneralString(settings.MusicEnable));
            Options.Bullet("Abilities", GeneralString(settings.GfAbilitiesEnable));
            Options.Bullet("Weapons", GeneralString(settings.UpgradeEnable));
        }

        public void AddBosses(Dictionary<int, int> encounterMap)
        {
            Bosses = new Section();
            Bosses.Heading("Bosses");
            foreach (var origId in encounterMap.Keys)
            {
                if (!Boss.Encounters.ContainsKey(origId)) continue;
                var origBoss = Boss.Encounters[origId];
                var newBoss = Boss.Encounters[encounterMap[origId]];
                Bosses.Bullet(newBoss.EncounterName, "");
                Bosses.Bullet("Location", origBoss.FieldName, 1);
                Bosses.Bullet("Replacing", origBoss.EncounterName, 1);
                Bosses.NewLine();
            }
        }

        public void AddDrawPoints(Dictionary<int, int> spellMap)
        {
            DrawPoints = new Section();
            DrawPoints.Heading("Draw Points");

            string last = "";
            foreach (var dp in DrawPointShuffle.DrawPoints.OrderBy(x => x.Location))
            {
                if (spellMap.ContainsKey(dp.Offset))
                {
                    if (dp.Location != last)
                    {
                        if (last != "") DrawPoints.NewLine();
                        DrawPoints.Bullet(dp.Location, "");
                    }
                    DrawPoints.Bullet(DrawPointShuffle.Spells.Find(s => s.SpellID == spellMap[dp.Offset]).SpellName, 1);
                    last = dp.Location;
                }
            }
        }

        public void AddShops(List<Shop> shops)
        {
            Shops = new Section();
            Shops.Heading("Shops");

            foreach (var s in shops)
            {
                Shops.Bullet(s.Name, "");
                foreach (var i in s.Items)
                {
                    Shops.Bullet(Item.Lookup[i.ItemCode].Name + (i.Hidden ? " (Familiar)" : ""), 1);
                }
                Shops.NewLine();
            }
        }

        public void AddCards(Dictionary<int, int> cardShuffle)
        {
            Cards = new Section();
            Cards.Heading("Cards");

            foreach (var cardId in cardShuffle.Keys)
            {
                var card = CardShuffle.Cards.Where(c => c.CardID == cardId).First();
                var deck = CardShuffle.Decks.Where(d => d.DeckID == cardShuffle[cardId]).First();
                Cards.Bullet(card.CardName, deck.LocationString);
            }
            Cards.NewLine();
        }

        public void AddLoot(List<MonsterInfo> monsters, bool drops, bool steals, bool draws)
        {
            Loot = new Section();
            Loot.Heading("Loot");
            Loot.Bullet("Format: Common / Uncommon / Rare / Very Rare");
            Loot.NewLine();

            foreach (var m in monsters)
            {
                var name = m.Name;
                if (name == "{03}*") name = "Rinoa";
                if (name == "{03}L") name = "Griever";
                if (name == " ") name = "(Unknown)";

                Loot.Bullet(name + ":");

                if (drops)
                {
                    Loot.Bullet(string.Format("Low-level drops: {0}", LootString(m.DropLow)), 1);
                    Loot.Bullet(string.Format("Mid-level drops: {0}", LootString(m.DropMed)), 1);
                    Loot.Bullet(string.Format("High-level drops: {0}", LootString(m.DropHigh)), 1);
                    Loot.NewLine();
                }

                if (steals)
                {
                    Loot.Bullet(string.Format("Low-level steals: {0}", LootString(m.MugLow)), 1);
                    Loot.Bullet(string.Format("Mid-level steals: {0}", LootString(m.MugMed)), 1);
                    Loot.Bullet(string.Format("High-level steals: {0}", LootString(m.MugHigh)), 1);
                    Loot.NewLine();
                }

                if (draws)
                {
                    Loot.Bullet(string.Format("Low-level draws: {0}", DrawString(m.DrawLow)), 1);
                    Loot.Bullet(string.Format("Mid-level draws: {0}", DrawString(m.DrawMed)), 1);
                    Loot.Bullet(string.Format("High-level draws: {0}", DrawString(m.DrawHigh)), 1);
                    Loot.NewLine();
                }
            }
        }

        public void AddMusic(Dictionary<int, int> tracks)
        {
            Music = new Section();
            Music.Heading("Music");

            var replacements = new List<string>();
            foreach (var t in tracks.Keys)
            {
                var orig = MusicShuffle.MusicTracks.Find(m => m.TrackID == t).TrackName;
                var repl = MusicShuffle.MusicTracks.Find(m => m.TrackID == tracks[t]).TrackName;
                replacements.Add(string.Format("{0} -> {1}", orig, repl));
            }

            // sort alphabetically
            foreach (var r in replacements.OrderBy(r => (r.StartsWith("[") ? "z" : "") + r)) Music.Bullet(r);
        }

        public void AddAbilities(List<JunctionableGF> gfs)
        {
            Abilities = new Section();
            Abilities.Heading("Abilities");

            for (var i = 0; i < gfs.Count; i++)
            {
                Abilities.Bullet(AbilityShuffle.GFNames.Find(gfn => gfn.GFID == i).GFName);
                for (var j = 0; j < gfs[i].Abilities.Length; j++)
                {
                    var abilityID = gfs[i].Abilities[j].Ability;
                    if (abilityID != 0) Abilities.Bullet(AbilityShuffle.Abilities.Find(an => an.AbilityID == abilityID).AbilityName, 1);
                }
                Abilities.NewLine();
            }
        }

        public void AddWeapons(FileSource mainSource, List<WeaponUpgrade> upgrades)
        {
            Weapons = new Section();
            Weapons.Heading("Weapons");

            var kernel = new Kernel(mainSource.GetFile(Globals.KernelPath));

            for (var i = 0; i < upgrades.Count; i++)
            {
                Weapons.Bullet(string.Format("{0} - {1}0g", kernel.Weapons[i].Name, upgrades[i].Price));
                Weapons.Bullet(string.Format("{0} x{1}", Item.Lookup[upgrades[i].Item1].Name, upgrades[i].Item1Quantity), 1);
                if (upgrades[i].Item2 != 0) Weapons.Bullet(string.Format("{0} x{1}", Item.Lookup[upgrades[i].Item2].Name, upgrades[i].Item2Quantity), 1);
                if (upgrades[i].Item3 != 0) Weapons.Bullet(string.Format("{0} x{1}", Item.Lookup[upgrades[i].Item3].Name, upgrades[i].Item3Quantity), 1);
                if (upgrades[i].Item4 != 0) Weapons.Bullet(string.Format("{0} x{1}", Item.Lookup[upgrades[i].Item4].Name, upgrades[i].Item4Quantity), 1);
                Weapons.NewLine();
            }
        }

        private string ModeString(bool modeFlag)
        {
            return modeFlag ? "Free Roam" : "Normal Game";
        }

        private string GeneralString(bool flag)
        {
            if (flag) return "Random";
            return "Normal";
        }

        private string DrawPointsString(bool enabled, bool apocFlag, bool slotFlag, bool cutFlag)
        {
            if (!enabled) return "Normal";
            var flags = new Dictionary<string, bool>()
            {
                { "Apoc", apocFlag },
                { "Slot", slotFlag },
                { "Cut", cutFlag }
            };
            return FlagString(flags);
        }

        private string LootString(bool dropsFlag, bool stealsFlag, bool drawsFlag)
        {
            if (!(dropsFlag || stealsFlag || drawsFlag)) return "Normal";
            var flags = new Dictionary<string, bool>()
            {
                { "Drops", dropsFlag },
                { "Steals", stealsFlag },
                { "Draws", drawsFlag }
            };
            return FlagString(flags);
        }

        private string ShopString(bool enabled, bool keyItems, bool summonItems, bool magazines, bool chocoboWorld)
        {
            if (!enabled) return "Normal";
            var flags = new Dictionary<string, bool>()
            {
                { "Key", keyItems },
                { "Summon", summonItems },
                { "Mag", magazines },
                { "Pocket", chocoboWorld }
            };
            return FlagString(flags);
        }

        private string FlagString(Dictionary<string, bool> flags)
        {
            return string.Format("Random ({0})", string.Join(", ", flags.Keys.Where(k => flags[k]).ToList()));
        }

        private string LootString(HeldItem[] items)
        {
            var result = new StringBuilder();
            result.Append(items[0].ItemId == 0 ? "Nothing" : Item.Lookup[items[0].ItemId].Name);
            result.Append(string.Format(" x{0}", items[0].Quantity));
            result.Append(" / ");
            result.Append(items[1].ItemId == 0 ? "Nothing" : Item.Lookup[items[1].ItemId].Name);
            result.Append(string.Format(" x{0}", items[1].Quantity));
            result.Append(" / ");
            result.Append(items[2].ItemId == 0 ? "Nothing" : Item.Lookup[items[2].ItemId].Name);
            result.Append(string.Format(" x{0}", items[2].Quantity));
            result.Append(" / ");
            result.Append(items[3].ItemId == 0 ? "Nothing" : Item.Lookup[items[3].ItemId].Name);
            result.Append(string.Format(" x{0}", items[3].Quantity));
            return result.ToString();
        }

        private string DrawString(byte[] spells)
        {
            var result = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                result.Append(SpellName(spells[i]));
                if (i < 3) result.Append(" / ");
            }
            return result.ToString();
        }

        private string SpellName(byte spell)
        {
            if (spell == 0) return "Nothing";
            if (spell >= 64) return "GF";
            return DrawPointShuffle.Spells.First(s => (byte)s.SpellID == spell).SpellName;
        }

        private List<Section> Sections
        {
            get
            {
                var result = new List<Section>() { Title, Options };
                if (Bosses != null) result.Add(Bosses);
                if (DrawPoints != null) result.Add(DrawPoints);
                if (Shops != null) result.Add(Shops);
                if (Cards != null) result.Add(Cards);
                if (Loot != null) result.Add(Loot);
                if (Music != null) result.Add(Music);
                if (Abilities != null) result.Add(Abilities);
                if (Weapons != null) result.Add(Weapons);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine + Environment.NewLine, Sections);
        }

        private class Section
        {
            public List<string> Lines;

            public Section()
            {
                Lines = new List<string>();
            }

            // underlined text
            public void Heading(string text, char underline = '-')
            {
                Lines.Add(text + Environment.NewLine + new string(underline, text.Length));
            }

            // indented bullet points
            public void Bullet(string title, string text, int indent = 0)
            {
                Lines.Add(string.Format("{0}* {1}: {2}", new string(' ', indent * 4), title, text));
            }

            public void Bullet(string text, int indent = 0)
            {
                Lines.Add(string.Format("{0}* {1}", new string(' ', indent * 4), text));
            }

            // extra spacing
            public void NewLine()
            {
                Lines.Add("");
            }

            public override string ToString()
            {
                return String.Join(Environment.NewLine, Lines);
            }
        }
    }
}
