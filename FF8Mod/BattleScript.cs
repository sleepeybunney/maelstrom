using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8Mod
{
    public partial class BattleScript
    {
        public List<Instruction> Init;          // executes when added to the battle (usually at the start)
        public List<Instruction> Execute;       // executes when ATB is full
        public List<Instruction> Counter;       // executes after receiving an action (eg. being attacked)
        public List<Instruction> Death;         // executes on death
        public List<Instruction> PreCounter;    // executes after receiving an action, before counter

        public BattleScript()
        {
            Init = new List<Instruction>();
            Execute = new List<Instruction>();
            Counter = new List<Instruction>();
            Death = new List<Instruction>();
            PreCounter = new List<Instruction>();

            // empty scripts still return
            Init.Add(new Instruction(OpCodes[0]));
            Execute.Add(new Instruction(OpCodes[0]));
            Counter.Add(new Instruction(OpCodes[0]));
            Death.Add(new Instruction(OpCodes[0]));
            PreCounter.Add(new Instruction(OpCodes[0]));
        }

        // construct from binary data (ie. game files)
        public BattleScript(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var initOffset = reader.ReadUInt32();
                var executeOffset = reader.ReadUInt32();
                var counterOffset = reader.ReadUInt32();
                var deathOffset = reader.ReadUInt32();
                var preCounterOffset = reader.ReadUInt32();

                stream.Position = initOffset;
                Init = ReadScript(reader, executeOffset - initOffset);

                stream.Position = executeOffset;
                Execute = ReadScript(reader, counterOffset - executeOffset);

                stream.Position = counterOffset;
                Counter = ReadScript(reader, deathOffset - counterOffset);

                stream.Position = deathOffset;
                Death = ReadScript(reader, preCounterOffset - deathOffset);

                stream.Position = preCounterOffset;
                PreCounter = ReadScript(reader, (uint)stream.Length - preCounterOffset);
            }
        }

        // decode an individual script from a binary stream
        private static List<Instruction> ReadScript(BinaryReader reader, uint length)
        {
            var result = new List<Instruction>();
            byte code;
            var initialOffset = reader.BaseStream.Position;

            while (reader.BaseStream.Position < initialOffset + length)
            {
                code = reader.ReadByte();

                // something is wrong, abort
                if (!OpCodes.ContainsKey(code))
                {
                    Console.WriteLine("Unknown op: " + code);
                    break;
                }

                var op = OpCodes[code];
                var args = new List<short>();

                // argument values are different lengths, depending on the op
                foreach (var a in op.Args)
                {
                    switch (a.Type)
                    {
                        case ArgType.Short:
                            args.Add(reader.ReadInt16());
                            break;
                        default:
                            args.Add(reader.ReadByte());
                            break;
                    }
                }
                result.Add(new Instruction(op, args.ToArray()));
            }

            return result;
        }

        public byte[] Encoded
        {
            get
            {
                var init = EncodeScript(Init);
                var exec = EncodeScript(Execute);
                var counter = EncodeScript(Counter);
                var death = EncodeScript(Death);
                var precounter = EncodeScript(PreCounter);

                var initOffset = 20;
                var execOffset = initOffset + init.Length;
                var counterOffset = execOffset + exec.Length;
                var deathOffset = counterOffset + counter.Length;
                var precounterOffset = deathOffset + death.Length;
                var totalLength = precounterOffset + precounter.Length;

                var result = new byte[totalLength];

                using (var stream = new MemoryStream(result))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((uint)initOffset);
                    writer.Write((uint)execOffset);
                    writer.Write((uint)counterOffset);
                    writer.Write((uint)deathOffset);
                    writer.Write((uint)precounterOffset);
                    writer.Write(init);
                    writer.Write(exec);
                    writer.Write(counter);
                    writer.Write(death);
                    writer.Write(precounter);
                }

                return result;
            }
        }

        private byte[] EncodeScript(List<Instruction> script)
        {
            var result = new List<byte>();
            foreach (var instruction in script)
            {
                result.AddRange(instruction.Encoded);
            }
            return result.ToArray();
        }

        public class OpCode
        {
            public string Name;
            public byte Code;
            public Argument[] Args;

            public OpCode(string name, byte code, params Argument[] args)
            {
                Name = name;
                Code = code;
                Args = args;
            }

            public int Length
            {
                get
                {
                    return Args.Sum(a => a.Type == ArgType.Short ? 2 : 1) + 1;
                }
            }
        }

        public class Argument
        {
            public string Name;
            public ArgType Type;

            public Argument(string name, ArgType type)
            {
                Name = name;
                Type = type;
            }

            public Argument(string name) : this(name, ArgType.Byte) { }
        }

        public class Instruction
        {
            public OpCode Op;
            public short[] Args;

            public Instruction(OpCode op, short[] args)
            {
                Op = op;
                Args = args;
            }

            public Instruction(OpCode op) : this(op, new short[0]) { }

            public byte[] Encoded
            {
                get
                {
                    var result = new List<byte>() { Op.Code };
                    for (var i = 0; i < Op.Args.Length; i++)
                    {
                        if (Op.Args[i].Type == ArgType.Short)
                        {
                            result.AddRange(BitConverter.GetBytes(Args[i]));
                        }
                        else
                        {
                            result.Add((byte)Args[i]);
                        }
                    }
                    return result.ToArray();
                }
            }

            public override string ToString()
            {
                var result = Op.Name;

                if (Args.Length > 0) result += " (";

                switch (Op.Code)
                {
                    // if statement
                    case 0x02:
                        
                        // first arg gives context to the others
                        // eg. whether we're looking at HP values or status effects etc.
                        var conditionType = (byte)Args[0];
                        result += ConditionCodes[conditionType];

                        // left side of the comparison may be omitted
                        // eg. "is-alive" only needs one value (the person whose aliveness we're checking)
                        // so that goes on the right & the left value is ignored
                        var left = (byte)Args[1];
                        if (ParameterisedConditions.Contains(conditionType))
                        {
                            result += "(";

                            // single target: target-hp and target-status
                            if (conditionType == 0x00 || conditionType == 0x04) result += GetConstant(Targets, left);

                            // numeric: random-number
                            else if (conditionType == 0x02) result += left;

                            // group-target: any-group-member-hp, number-in-group-alive, etc.
                            else result += GetConstant(Groups, left);

                            result += ")";
                        }

                        // last-action compares against a single property of the most recent action
                        // eg. the person who did it or the element of the attack
                        if (conditionType == 0x0a)
                        {
                            result += "." + GetConstant(ActionProperties, left, "unknown");
                        }

                        // comparison operator
                        var op = (byte)Args[2];
                        result += " " + Operators[op];

                        // right side of the comparison may be a byte or a short, depending on what it is
                        var right = Args[3];
                        result += " ";
                        switch (conditionType)
                        {
                            // HP is measured in increments of 10%, or 25%
                            case 0x00:
                            case 0x01:
                                result += GetConstant(PercentageModifiers, right);
                                break;

                            // status effects
                            case 0x04:
                            case 0x05:
                            case 0x14:
                                result += GetConstant(StatusCodes, right);
                                break;

                            // single target
                            case 0x09:
                                result += GetConstant(Targets, right);
                                break;

                            // last-action properties
                            case 0x0a:
                                switch (left)
                                {
                                    case 0x00:
                                        result += GetConstant(ActionTypes, right);
                                        break;
                                    case 0x01:
                                        result += GetConstant(Targets, right);
                                        break;
                                    case 0x03:
                                        result += GetConstant(MenuCommands, right);
                                        break;
                                    case 0x05:
                                        result += GetConstant(Elements, right);
                                        break;
                                    default:
                                        result += right;
                                        break;
                                }
                                break;

                            // level bands (low < med < high)
                            case 0x0e:
                                result += GetConstant(LevelBands, right);
                                break;

                            // genders
                            case 0x10:
                                result += GetConstant(Genders, right);
                                break;

                            // numeric or unknown values
                            default:
                                result += right;
                                break;
                        }

                        // offset to jump if the condition isn't met (+/-)
                        var offset = Args[4];
                        result += " [" + offset + "]";
                        break;

                    // target
                    case 0x04:
                        result += GetConstant(Targets, Args[0]);
                        break;

                    // friend-remove
                    case 0x1d:
                        result += GetConstant(Friends, Args[0]);
                        break;

                    // target-random
                    // narrowed down by group (eg. all enemies) then by status (eg. not asleep)
                    case 0x26:
                        var group = (byte)Args[1];
                        var oper = (byte)Args[2];
                        var status = (byte)Args[3];
                        result += string.Format("{0} {1} {2}", GetConstant(Groups, group), GetConstant(Operators, oper), GetConstant(StatusCodes, status));
                        break;

                    // permanent-status
                    case 0x27:
                        result += string.Format("{0}, {1}", GetConstant(StatusCodes, Args[0]), Args[1]);
                        break;

                    // elem-def-change
                    case 0x2d:
                        result += string.Format("{0}, {1}", GetConstant(Elements, Args[0]), Args[1]);
                        break;

                    // numeric or unknown values
                    default:
                        result += string.Join(", ", Args);
                        break;

                }

                if (Args.Length > 0) result += ")";
                return result;
            }
        }
    }
}
