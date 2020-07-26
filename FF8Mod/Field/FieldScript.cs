using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FF8Mod;
using FF8Mod.Archive;

namespace FF8Mod.Field
{
    public partial class FieldScript
    {
        public List<Entity> Entities;

        public FieldScript()
        {
            Entities = new List<Entity>();
        }

        // construct from binary data (ie. game files)
        public static FieldScript FromBytes(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var doorCount = reader.ReadByte();
                var lineCount = reader.ReadByte();
                var backgroundCount = reader.ReadByte();
                var otherCount = reader.ReadByte();
                var totalCount = doorCount + lineCount + backgroundCount + otherCount;

                var scriptInfoOffset = reader.ReadUInt16();
                var scriptDataOffset = reader.ReadUInt16();

                // script counts & labels for each entity
                var entityInfo = new List<EntityInfo>();
                for (var i = 0; i < totalCount; i++)
                {
                    // entity types are inferred from their order in the file
                    var type = EntityType.Line;
                    if (i >= lineCount) type = EntityType.Door;
                    if (i >= lineCount + doorCount) type = EntityType.Background;
                    if (i >= lineCount + doorCount + backgroundCount) type = EntityType.Other;

                    entityInfo.Add(new EntityInfo(type, reader.ReadUInt16()));
                }

                // script offsets within the data block
                var scriptInfo = new List<ScriptInfo>();
                var scriptCount = (scriptDataOffset - scriptInfoOffset) / 2;
                for (var i = 0; i < scriptCount; i++)
                {
                    scriptInfo.Add(new ScriptInfo(reader.ReadUInt16()));
                }

                // script data
                var scripts = new List<List<Instruction>>();
                for (int i = 0; i < scriptCount - 1; i++)
                {
                    var script = new List<Instruction>();
                    while (stream.Position < scriptDataOffset + scriptInfo[i + 1].Position)
                    {
                        script.Add(new Instruction(reader.ReadBytes(4)));
                    }
                    scripts.Add(script);
                }

                // put it all together
                var result = new FieldScript();
                for (int i = 0; i < entityInfo.Count; i++)
                {
                    var entityScripts = new List<Script>();
                    for (int j = 0; j <= entityInfo[i].ScriptCount; j++)
                    {
                        var instructions = scripts[entityInfo[i].Label + j];
                        var flag = scriptInfo[entityInfo[i].Label + j].Flag == 1;
                        entityScripts.Add(new Script(instructions, flag));
                    }
                    result.Entities.Add(new Entity(entityInfo[i].Type, entityScripts));
                }
                return result;
            }
        }

        // extract from field archive & construct
        public static FieldScript FromSource(FileSource fieldSource, string fieldName)
        {
            var fieldPath = GetFieldPath(fieldName);
            var innerSource = new FileSource(fieldPath, fieldSource);
            return FromBytes(innerSource.GetFile(fieldPath + "\\" + fieldName + Globals.ScriptFileExtension));
        }

        public static string GetFieldPath(string fieldName)
        {
            var abbrev = fieldName.Substring(0, 2);
            return string.Format(Globals.DataPath + @"\field\mapdata\{0}\{1}", abbrev, fieldName);
        }

        // jsm output
        public byte[] Encode()
        {
            var doorCount = 0;
            var lineCount = 0;
            var backgroundCount = 0;
            var otherCount = 0;
            var allScripts = new List<Script>();

            foreach (var e in Entities)
            {
                switch (e.Type)
                {
                    case EntityType.Door:
                        doorCount++;
                        break;
                    case EntityType.Line:
                        lineCount++;
                        break;
                    case EntityType.Background:
                        backgroundCount++;
                        break;
                    default:
                        otherCount++;
                        break;
                }

                allScripts.AddRange(e.Scripts);
            }

            allScripts = allScripts.OrderBy(s => s.Instructions[0].Param).ToList();

            var entityInfoLength = Entities.Count * 2;                              // 2 bytes per entity
            var scriptInfoLength = allScripts.Count * 2 + 2;                        // 2 bytes per script + an extra entry to denote EOF
            var scriptDataLength = allScripts.Sum(s => s.Instructions.Count * 4);   // 4 bytes per instruction
            var totalLength = 8 + entityInfoLength + scriptInfoLength + scriptDataLength;

            var scriptInfoOffset = entityInfoLength + 8;                            // entity counts above + these two offsets = 8 bytes
            var scriptDataOffset = scriptInfoLength + scriptInfoOffset;

            var result = new byte[totalLength];

            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte)doorCount);
                writer.Write((byte)lineCount);
                writer.Write((byte)backgroundCount);
                writer.Write((byte)otherCount);
                writer.Write((short)scriptInfoOffset);
                writer.Write((short)scriptDataOffset);

                // entity info block
                // todo: make sure everything's still in the right order (entity types -> script labels)
                foreach (var e in Entities)
                {
                    writer.Write(new EntityInfo(e).Encode());
                }

                // script info block (& gathering script data while we're here)
                var scriptData = new List<int>();
                foreach (var s in allScripts)
                {
                    var info = new ScriptInfo(scriptData.Count * 4, s.MysteryFlag ? 1 : 0);
                    writer.Write(info.Encode());

                    foreach (var i in s.Instructions)
                    {
                        scriptData.Add(i.Encode());
                    }
                }

                // extra eof entry
                writer.Write(new ScriptInfo(scriptData.Count * 4, 0).Encode());

                // script data block
                foreach (var s in scriptData)
                {
                    writer.Write(s);
                }
            }

            return result;

        }

        // overwrite a script with interpreted instructions
        public void ReplaceScript(int entity, int script, string scriptText)
        {
            var label = Entities[entity].Scripts[script].Instructions[0].Param;
            var newScript = new Script(scriptText);
            newScript.Instructions.Insert(0, new Instruction(FieldScript.OpCodesReverse["lbl"], label));
            newScript.Instructions.Add(new Instruction(FieldScript.OpCodesReverse["ret"], 8));
            Entities[entity].Scripts[script].Instructions = newScript.Instructions;
        }
    }

    // field entity (eg. a person or an event trigger)
    public class Entity
    {
        public EntityType Type;
        public List<Script> Scripts;

        public Entity(EntityType type, List<Script> scripts)
        {
            Type = type;
            Scripts = scripts;
        }
    }

    public enum EntityType
    {
        Line,
        Door,
        Background,
        Other
    }

    // code attached to an entity
    public class Script
    {
        public List<Instruction> Instructions;
        public bool MysteryFlag;

        public Script()
        {
            Instructions = new List<Instruction>();
            MysteryFlag = false;
        }

        public Script(List<Instruction> instructions, bool flag)
        {
            Instructions = instructions;
            MysteryFlag = flag;
        }

        public Script(string instructions) : this()
        {
            var lines = instructions.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var tokens = line.Split(null, 2).Select(t => t.Trim().ToLower()).Where(t => !string.IsNullOrEmpty(t)).ToList();
                if (!FieldScript.OpCodesReverse.Keys.Contains(tokens[0]))
                {
                    throw new Exception("unrecognised operation in fieldscript file (" + instructions + "): " + line);
                }
                var instruction = new Instruction(FieldScript.OpCodesReverse[tokens[0]]);

                if (tokens.Count > 1)
                {
                    if (!int.TryParse(tokens[1], out int param))
                    {
                        throw new Exception("invalid parameter in fieldscript file (" + instructions + "): " + line);
                    }
                    instruction.Param = param;
                    instruction.HasParam = true;
                }

                Instructions.Add(instruction);
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Instructions);
        }
    }

    // header data for entities in the jsm file
    public class EntityInfo
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

    // header data for scripts in the jsm file
    public class ScriptInfo
    {
        public int Position;
        public int Flag;

        public ScriptInfo(int position, int flag)
        {
            Position = position;
            Flag = flag;
        }

        public ScriptInfo(ushort data)
        {
            Position = (data & 0x7fff) * 4;
            Flag = data >> 15;
        }

        public ushort Encode()
        {
            return (ushort)((Flag << 15) + Position / 4);
        }
    }

    public class Instruction
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

        public Instruction(byte[] instruction)
        {
            var value = BitConverter.ToInt32(instruction, 0);
            HasParam = (value & 0xff000000) != 0;

            if (HasParam)
            {
                OpCode = value >> 24;
                Param = unchecked((short)(value & 0x00ffffff));
            }
            else
            {
                OpCode = value;
                Param = 0;
            }
        }

        public int Encode()
        {
            if (HasParam) return (OpCode << 24) | (Param & 0x00ffffff);
            else return OpCode;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", FieldScript.OpCodes[OpCode], HasParam ? " " + Param : "");
        }
    }
}
