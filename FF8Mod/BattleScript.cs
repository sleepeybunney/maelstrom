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
            this.Init = new List<Instruction>();
            this.Execute = new List<Instruction>();
            this.Counter = new List<Instruction>();
            this.Death = new List<Instruction>();
            this.PreCounter = new List<Instruction>();
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

            public override string ToString()
            {
                var result = Op.Name;
                foreach (var a in Args) result += " " + a;
                return result;
            }
        }
    }
}
