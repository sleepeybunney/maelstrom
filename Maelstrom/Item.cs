using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace Sleepey.Maelstrom
{
    public class Item
    {
        public static Dictionary<int, Item> Lookup = JsonSerializer.Deserialize<List<Item>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.Items.json")).ToDictionary(i => i.ID);

        public int ID { get; set; }
        public string Name { get; set; }
        public bool KeyItem { get; set; } = false;
        public bool Magazine { get; set; } = false;
        public bool SummonItem { get; set; } = false;
        public bool ChocoboWorld { get; set; } = false;
        public bool Medicine { get; set; } = false;
        public string MedicineName { get; set; } = string.Empty;
        public bool MonsterItem { get; set; } = false;
        public string MonsterName { get; set; } = string.Empty;
        public bool FenceItem { get; set; } = false;
    }
}
