using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Field
{
    // field entity (eg. a person or an event trigger)
    public class Entity
    {
        public EntityType Type { get; set; }
        public List<Script> Scripts { get; set; }

        public Entity(EntityType type, IEnumerable<Script> scripts)
        {
            Type = type;
            Scripts = scripts.ToList();
        }

        public int Label { get => Scripts.First().Label; }
    }
}
