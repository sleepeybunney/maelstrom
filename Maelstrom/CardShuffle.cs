using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using FF8Mod.Archive;
using FF8Mod.Field;

namespace FF8Mod.Maelstrom
{
    class CardShuffle
    {
        public static List<Deck> Decks = JsonSerializer.Deserialize<List<Deck>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Decks.json"));

        public static void Shuffle(FileSource fieldSource, int seed)
        {
            var random = new Random(seed);

            // take a script from the start field
            var scriptField = "start0";
            var scriptEntity = 0;
            var scriptId = 0;
            var script = App.ReadEmbeddedFile(string.Format("FF8Mod.Maelstrom.FieldScripts.{0}.{1}.{2}.txt", scriptField, scriptEntity, scriptId));

            // get a list of valid decks to put cards in (no npcs in debug rooms etc)
            var availableDecks = Decks.Where(d => d.Enabled).Select(d => d.DeckID).Distinct().ToList();

            // assign rare cards to random decks
            for (var i = 77; i <= 109; i++)
            {
                // if you getcard before you setcard, it tells you the location in the card menu
                // script += Environment.NewLine + GetCard(i);

                script += Environment.NewLine + SetCard(availableDecks[random.Next(0, availableDecks.Count)], i);
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
