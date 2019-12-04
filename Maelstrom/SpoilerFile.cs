using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8Mod.Maelstrom
{
    class SpoilerFile
    {
        private Section Title, Options, Bosses, DrawPoints, Shops;

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
                    Shops.Bullet(ShopShuffle.Items[i.ItemCode].Name + (i.Hidden ? " (Familiar)" : ""), 1);
                }
                Shops.NewLine();
            }
        }

        private string ModeString(bool modeFlag)
        {
            return modeFlag ? "Free Roam" : "Normal Game";
        }

        private string BossesString(bool shuffleFlag, bool balanceFlag)
        {
            var result = shuffleFlag ? "Shuffled" : "";
            if (shuffleFlag && balanceFlag) result += ", ";
            if (balanceFlag) result += "Rebalanced";
            return result;
        }

        private string GeneralString(bool flag)
        {
            return flag ? "Random" : "Normal";
        }

        private List<Section> Sections
        {
            get
            {
                var result = new List<Section>() { Title, Options };
                if (Bosses != null) result.Add(Bosses);
                if (DrawPoints != null) result.Add(DrawPoints);
                if (Shops != null) result.Add(Shops);
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
