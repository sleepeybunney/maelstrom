using System;
using System.Collections.Generic;
using System.Text;

namespace Sleepey.FF8Mod
{
    public class Deck
    {
        public int DeckID { get; set; }
        public string FieldID { get; set; }
        public string FieldName { get; set; }
        public string EntityName { get; set; }
        public string EntityDescription { get; set; }
        public bool Enabled { get; set; }

        public string LocationString
        {
            get
            {
                var result = FieldName;
                if (!string.IsNullOrWhiteSpace(EntityDescription)) result += string.Format(" ({0})", EntityDescription);
                return result;
            }
        }
    }
}
