using System;
using System.Collections.Generic;
using System.IO;

namespace FF8Mod
{
    public partial class FieldScript
    {
        public List<Entity> Entities;

        public FieldScript(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var doorCount = reader.ReadByte();
                var lineCount = reader.ReadByte();
                var backgroundCount = reader.ReadByte();
                var otherCount = reader.ReadByte();
                var totalCount = doorCount + lineCount + backgroundCount + otherCount;

                var firstSectionOffset = reader.ReadUInt16();
                var scriptDataOffset = reader.ReadUInt16();

                var entityInfo = new List<EntityInfo>();
                for (var i = 0; i < totalCount; i++)
                {
                    var type = EntityType.Line;
                    if (i >= lineCount) type = EntityType.Door;
                    if (i >= lineCount + doorCount) type = EntityType.Background;
                    if (i >= lineCount + doorCount + backgroundCount) type = EntityType.Other;

                    var value = reader.ReadUInt16();
                    entityInfo.Add(new EntityInfo(type, value & 0x7f, value >> 7));
                }

                var scriptInfo = new List<ScriptInfo>();
                var scriptCount = (scriptDataOffset - firstSectionOffset) / 2;
                for (var i = 0; i < scriptCount; i++)
                {
                    var value = reader.ReadUInt16();
                    scriptInfo.Add(new ScriptInfo((value & 0x7fff) * 4, value >> 15));
                }

                var scripts = new List<List<Instruction>>();
                for (int i = 0; i < scriptCount - 1; i++)
                {
                    var script = new List<Instruction>();
                    while (stream.Position < scriptDataOffset + scriptInfo[i + 1].Position)
                    {
                        var value = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                        var hasParam = (value & 0xff000000) != 0;
                        if (hasParam)
                        {
                            var param = unchecked((short)(value & 0x00ffffff));
                            script.Add(new Instruction(value >> 24, param));
                        }
                        else
                        {
                            script.Add(new Instruction(value));
                        }
                    }
                    scripts.Add(script);
                }

                Entities = new List<Entity>();
                for (int i = 0; i < entityInfo.Count; i++)
                {
                    var entityScripts = new List<Script>();
                    for (int j = 0; j <= entityInfo[i].ScriptCount; j++)
                    {
                        var instructions = scripts[entityInfo[i].Label + j];
                        var flag = scriptInfo[entityInfo[i].Label + j].Flag == 1;
                        entityScripts.Add(new Script(instructions, flag));
                    }
                    Entities.Add(new Entity(entityInfo[i].Type, entityScripts));
                }
            }
        }

        public class Entity
        {
            public EntityType Type;
            public List<Script> Scripts;

            public Entity(EntityType type, List<Script> scripts)
            {
                Type = type;
                Scripts = scripts;
            }

            public Entity(EntityType type) : this(type, new List<Script>()) { }
        }

        public class Script
        {
            public List<Instruction> Instructions;
            public bool MysteryFlag;

            public Script(List<Instruction> instructions, bool flag)
            {
                Instructions = instructions;
                MysteryFlag = flag;
            }
        }

        public struct EntityInfo
        {
            public EntityType Type;
            public int ScriptCount;
            public int Label;

            public EntityInfo(EntityType type, int scriptCount, int label)
            {
                Type = type;
                ScriptCount = scriptCount;
                Label = label;
            }
        }

        public struct ScriptInfo
        {
            public int Position;
            public int Flag;

            public ScriptInfo(int position, int flag)
            {
                Position = position;
                Flag = flag;
            }
        }

        public struct Instruction
        {
            public int OpCode;
            public int Param;
            public bool HasParam;

            public Instruction(int opCode, int param)
            {
                OpCode = opCode;
                Param = param;
                HasParam = true;
            }

            public Instruction(int opCode)
            {
                OpCode = opCode;
                Param = 0;
                HasParam = false;
            }

            public override string ToString()
            {
                return string.Format("{0}{1}", OpCodes[OpCode], HasParam ? " " + Param : "");
            }
        }
    }
}
