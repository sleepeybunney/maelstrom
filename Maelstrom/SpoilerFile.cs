using System;
using System.Collections.Generic;
using System.Text;

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
                Bosses.Bullet("Replaces", origBoss.EncounterName, 1);
                Bosses.Bullet("Location", origBoss.FieldName, 1);
                Bosses.NewLine();
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
