using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            byte prevCode = byte.MaxValue;
            var initialOffset = reader.BaseStream.Position;

            while (reader.BaseStream.Position < initialOffset + length)
            {
                code = reader.ReadByte();

                // skip over multiple returns in a row (padding)
                if (code == 0 && prevCode == 0) continue;

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
                prevCode = code;
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
                result.Add(instruction.Op.Code);
                for (var i = 0; i < instruction.Op.Args.Length; i++)
                {
                    if (instruction.Op.Args[i].Type == ArgType.Short)
                    {
                        result.AddRange(BitConverter.GetBytes(instruction.Args[i]));
                    }
                    else
                    {
                        result.Add((byte)instruction.Args[i]);
                    }
                }
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

            public override string ToString()
            {
                var result = Op.Name;

                if (Args.Length > 0 && Op.Code != 0x02) result += " (";

                switch (Op.Code)
                {
                    case 0x02:
                        var conditionType = (byte)Args[0];
                        result += " " + ConditionCodes[conditionType];

                        var left = (byte)Args[1];
                        if (ParameterisedConditions.Contains(conditionType))
                        {
                            result += "(";
                            if (new byte[] { 0x00, 0x04 }.Contains(conditionType))
                            {
                                if (Targets.Keys.Contains(left)) result += Targets[left];
                                else result += left;
                            }
                            else if (new byte[] { 0x01, 0x05, 0x06, 0x10, 0x14 }.Contains(conditionType))
                            {
                                if (Groups.Keys.Contains(left)) result += Groups[left];
                                else result += left;
                            }
                            else result += left;
                            result += ")";
                        }

                        if (conditionType == 0x0a)
                        {
                            result += ".";
                            switch (left)
                            {
                                case 0x00:
                                    result += "type";
                                    break;
                                case 0x01:
                                    result += "subject";
                                    break;
                                case 0x02:
                                    result += "unknown2";
                                    break;
                                case 0x03:
                                    result += "command";
                                    break;
                                case 0x04:
                                    result += "id";
                                    break;
                                case 0x05:
                                    result += "element";
                                    break;
                                default:
                                    result += "unknown";
                                    break;
                            }
                        }

                        var op = (byte)Args[2];
                        result += " " + Operators[op];

                        var right = Args[3];
                        result += " ";
                        switch (conditionType)
                        {
                            case 0x00:
                            case 0x01:
                                result += PercentageModifiers[(byte)right];
                                break;
                            case 0x04:
                            case 0x05:
                            case 0x14:
                                result += StatusCodes[(byte)right];
                                break;
                            case 0x09:
                                if (Targets.Keys.Contains((byte)right)) result += Targets[(byte)right];
                                else result += right;
                                break;
                            case 0x0a:
                                switch (left)
                                {
                                    case 0x00:
                                        result += ActionTypes[(byte)right];
                                        break;
                                    case 0x01:
                                        if (Targets.Keys.Contains((byte)right)) result += Targets[(byte)right];
                                        else result += right;
                                        break;
                                    case 0x03:
                                        result += MenuCommands[(byte)right];
                                        break;
                                    case 0x05:
                                        result += Elements[(byte)right];
                                        break;
                                    default:
                                        result += right;
                                        break;
                                }
                                break;
                            case 0x0e:
                                result += LevelBands[(byte)right];
                                break;
                            case 0x10:
                                result += Genders[(byte)right];
                                break;
                            default:
                                result += right;
                                break;
                        }

                        var offset = Args[4];
                        result += " [" + offset + "]";
                        break;
                    case 0x04:
                        var target = (byte)Args[0];
                        if (Targets.Keys.Contains(target)) result += Targets[target];
                        else result += target;
                        break;
                    case 0x1d:
                        var friend = (byte)Args[0];
                        if (Friends.Keys.Contains(friend)) result += Targets[friend];
                        else result += friend;
                        break;
                    case 0x26:
                        var group = (byte)Args[1];
                        var oper = (byte)Args[2];
                        var status = (byte)Args[3];

                        if (Groups.Keys.Contains(group)) result += Groups[group];
                        else result += group;

                        result += string.Format(" {0} {1}", Operators[oper], StatusCodes[status]);
                        break;
                    case 0x27:
                        result += string.Format("{0}, {1}", StatusCodes[(byte)Args[0]], Args[1]);
                        break;
                    case 0x2d:
                        result += string.Format("{0}, {1}", Elements[(byte)Args[0]], Args[1]);
                        break;
                    default:
                        result += string.Join(", ", Args);
                        break;
                }

                if (Args.Length > 0 && Op.Code != 0x02) result += ")";
                return result;
            }
        }
    }
}
