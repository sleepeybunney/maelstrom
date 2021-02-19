using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Field;

namespace Sleepey.Maelstrom
{
    class CardShuffle
    {
        public static List<Deck> Decks = JsonSerializer.Deserialize<List<Deck>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Decks.json"));
        public static List<Card> Cards = JsonSerializer.Deserialize<List<Card>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Cards.json"));

        public static Dictionary<int, int> Shuffle(int seed)
        {
            var random = new Random(seed + 3);

            // get a list of valid decks to put cards in (no npcs in debug rooms etc)
            var availableDecks = Decks.Where(d => d.Enabled).Select(d => d.DeckID).Distinct().ToList();

            // assign rare cards to random decks
            var result = new Dictionary<int, int>();
            foreach (var card in Cards.Select(c => c.CardID))
            {
                result.Add(card, availableDecks[random.Next(0, availableDecks.Count)]);
            }

            return result;
        }

        public static void Apply(FileSource fieldSource, Dictionary<int, int> shuffle)
        {
            // take a script from the start field
            var scriptField = "start0";
            var scriptEntity = 0;
            var scriptId = 0;
            var script = App.ReadEmbeddedFile(string.Format("Sleepey.Maelstrom.FieldScripts.{0}.{1}.{2}.txt", scriptField, scriptEntity, scriptId));

            // move cards to their assigned decks
            foreach (var card in shuffle.Keys)
            {
                // if you getcard before you setcard, it reveals the location in the card menu
                // script += Environment.NewLine + GetCard(i);

                script += Environment.NewLine + SetCard(shuffle[card], card);
            }

            // save the script
            var field = FieldScript.FromSource(fieldSource, scriptField);
            field.ReplaceScript(scriptEntity, scriptId, script);
            StorySkip.SaveToSource(fieldSource, scriptField, field.Encode()); // todo: put this somewhere more sensible
        }

        private static string GetCard(int card)
        {
            return string.Format("pshn_l {0}{1}getcard", card, Environment.NewLine);
        }

        private static string SetCard(int deck, int card)
        {
            return string.Format("pshn_l {0}{2}pshn_l {1}{2}setcard", deck, card, Environment.NewLine);
        }
    }
}
