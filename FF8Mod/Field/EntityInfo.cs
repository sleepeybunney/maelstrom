using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Field
{
    // header data for entities in the jsm file
    public class EntityInfo
    {
        public EntityType Type { get; set; }
        public int ScriptCount { get; set; }
        public int Label { get; set; }

        public EntityInfo(EntityType type, int scriptCount, int label)
        {
            Type = type;
            ScriptCount = scriptCount;
            Label = label;
        }

        public EntityInfo(EntityType type, ushort data)
        {
            Type = type;
            ScriptCount = data & 0x7f;
            Label = data >> 7;
        }

        public EntityInfo(Entity entity)
        {
            Type = entity.Type;
            ScriptCount = entity.Scripts.Count - 1;     // entry script (the one referred to by Label) isn't counted
            Label = entity.Scripts[0].Instructions[0].Param;
        }

        public ushort Encode()
        {
            return (ushort)((Label << 7) + ScriptCount);
        }
    }
}
