using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace FF8Mod.Maelstrom
{
    class Item
    {
        public static Dictionary<int, Item> Lookup = JsonSerializer.Deserialize<List<Item>>(App.ReadEmbeddedFile("FF8Mod.Maelstrom.Data.Items.json")).ToDictionary(i => i.ID);

        public int ID { get; set; }
        public string Name { get; set; }
        public bool KeyItem { get; set; } = false;
        public bool Magazine { get; set; } = false;
        public bool SummonItem { get; set; } = false;
        public bool ChocoboWorld { get; set; } = false;
    }
}
