using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FF8Mod.Maelstrom
{
    class SpoilerFile
    {
        private Section Title, Options, Bosses, DrawPoints, Shops, Cards, Loot, Music;

        public SpoilerFile()
        {
            Title = new Section();
            Title.Heading("Maelstrom Spoiler File", '=');

            Options = new Section();
            Options.Heading("Options");

            var settings = Properties.Settings.Default;
            Options.Bullet("Mode", ModeString(settings.StorySkip));
            Options.Bullet("Seed", settings.SeedValue.ToString());
            Options.Bullet("Bosses", BossesString(settings.BossShuffle, settings.BossRebalance));
            Options.Bullet("Draw Points", GeneralString(settings.DrawPointShuffle));
            Options.Bullet("Shops", GeneralString(settings.ShopShuffle));
            Options.Bullet("Cards", GeneralString(settings.CardShuffle));
            Options.Bullet("Loot", GeneralString(settings.LootShuffle));
            Options.Bullet("Music", GeneralString(settings.MusicShuffle));
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
                    DrawPoints.Bullet(Enum.GetName(typeof(DrawPoint.Magic), spellMap[dp.Offset]), 1);
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

        public void AddLoot(List<MonsterInfo> monsters)
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
                Loot.Bullet(string.Format("Low-level drops: {0}", LootString(m.DropLow)), 1);
                Loot.Bullet(string.Format("Mid-level drops: {0}", LootString(m.DropMed)), 1);
                Loot.Bullet(string.Format("High-level drops: {0}", LootString(m.DropHigh)), 1);
                Loot.NewLine();
                Loot.Bullet(string.Format("Low-level steals: {0}", LootString(m.MugLow)), 1);
                Loot.Bullet(string.Format("Mid-level steals: {0}", LootString(m.MugMed)), 1);
                Loot.Bullet(string.Format("High-level steals: {0}", LootString(m.MugHigh)), 1);
                Loot.NewLine();
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

        private string ModeString(bool modeFlag)
        {
            return modeFlag ? "Free Roam" : "Normal Game";
        }

        private string BossesString(bool shuffleFlag, bool balanceFlag)
        {
            if (!shuffleFlag) return "Normal";
            if (shuffleFlag && balanceFlag) return "Shuffled, Rebalanced";
            return "Shuffled";
        }

        private string GeneralString(bool flag)
        {
            return flag ? "Random" : "Normal";
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
