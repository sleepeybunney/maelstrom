using FF8Mod.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FF8Mod.Maelstrom
{
    class SpoilerFile
    {
        private Section Title, Options, Bosses, DrawPoints, Shops, Cards, Loot, Music, Abilities;

        public SpoilerFile()
        {
            Title = new Section();
            Title.Heading("Maelstrom Spoiler File", '=');

            Options = new Section();
            Options.Heading("Options");

            var settings = State.Current;
            Options.Bullet("Mode", ModeString(settings.FreeRoam));
            Options.Bullet("Seed", settings.SeedValue.ToString());
            Options.Bullet("Bosses", BossesString(settings.BossLocations, false));
            Options.Bullet("Draw Points", DrawPointsString(settings.DrawPointSpells, settings.DrawPointIncludeApoc, settings.DrawPointIncludeSlot, settings.DrawPointIncludeCut));
            Options.Bullet("Shops", settings.ShopItems);
            Options.Bullet("Cards", settings.CardLocations);
            Options.Bullet("Loot", LootString(settings.LootDrops, settings.LootSteals));
            Options.Bullet("Music", settings.Music);
            Options.Bullet("Abilities", settings.GfAbilities);
        }

        public void AddBosses(Dictionary<int, int> encounterMap)
        {
            Bosses = new Section();
            Bosses.Heading("Bosses");
            foreach (var origId in encounterMap.Keys)
            {
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

        public void AddLoot(List<MonsterInfo> monsters, bool drops, bool steals)
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
                    Abilities.Bullet(AbilityShuffle.AbilityNames.Find(an => an.AbilityID == gfs[i].Abilities[j].Ability).AbilityName, 1);
                }
                Abilities.NewLine();
            }
        }

        private string ModeString(bool modeFlag)
        {
            return modeFlag ? "Free Roam" : "Normal Game";
        }

        private string BossesString(string shuffleFlag, bool balanceFlag)
        {
            if (shuffleFlag == "Normal" && balanceFlag) return "Shuffled, Rebalanced";
            return shuffleFlag;
        }

        private string DrawPointsString(string dpFlag, bool apocFlag, bool slotFlag, bool cutFlag)
        {
            if (dpFlag == "Normal" || (!apocFlag && !slotFlag && !cutFlag)) return dpFlag;

            var flagStrings = new List<string>();
            if (apocFlag) flagStrings.Add("+apoc");
            if (slotFlag) flagStrings.Add("+slot");
            if (cutFlag) flagStrings.Add("+cut");
            return string.Format("{0} ({1})", dpFlag, string.Join(" ", flagStrings));
        }

        private string LootString(string dropsFlag, string stealsFlag)
        {
            var drops = (dropsFlag != "Normal");
            var steals = (stealsFlag != "Normal");

            if (drops && steals) return "Random (All)";
            if (drops) return "Random (Drops)";
            if (steals) return "Random (Steals)";
            return "Normal";
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
