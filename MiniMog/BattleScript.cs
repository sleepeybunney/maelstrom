using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiniMog
{
    partial class BattleScript
    {
        public List<Instruction> Init;
        public List<Instruction> Execute;
        public List<Instruction> Counter;
        public List<Instruction> Death;
        public List<Instruction> PreCounter;

        public BattleScript()
        {
            this.Init = new List<Instruction>();
            this.Execute = new List<Instruction>();
            this.Counter = new List<Instruction>();
            this.Death = new List<Instruction>();
            this.PreCounter = new List<Instruction>();
        }

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

                Console.WriteLine("INIT");
                stream.Position = initOffset;
                Init = ReadScript(reader, executeOffset - initOffset);

                Console.WriteLine("EXECUTE");
                stream.Position = executeOffset;
                Execute = ReadScript(reader, counterOffset - executeOffset);

                Console.WriteLine("COUNTER");
                stream.Position = counterOffset;
                Counter = ReadScript(reader, deathOffset - counterOffset);

                Console.WriteLine("DEATH");
                stream.Position = deathOffset;
                Death = ReadScript(reader, preCounterOffset - deathOffset);

                Console.WriteLine("PRECOUNTER");
                stream.Position = preCounterOffset;
                PreCounter = ReadScript(reader, (uint)stream.Length - preCounterOffset);
            }
        }

        private static List<Instruction> ReadScript(BinaryReader reader, uint length)
        {
            var result = new List<Instruction>();
            byte code;
            byte prevCode = byte.MaxValue;
            var initialOffset = reader.BaseStream.Position;

            while (reader.BaseStream.Position < initialOffset + length)
            {
                code = reader.ReadByte();

                if (code == 0 && prevCode == 0) continue;

                if (!OpCodes.ContainsKey(code))
                {
                    Console.WriteLine("Unknown op: " + code);
                    break;
                }

                var op = OpCodes[code];
                var args = new List<short>();
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

                var log = new StringBuilder(op.Name);
                for (int i=0; i < args.Count; i++)
                {
                    log.Append(" ");
                    log.Append(op.Args[i].Name);
                    log.Append("=");
                    log.Append(args[i]);
                }
                Console.WriteLine(log.ToString());
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

        public struct Argument
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

        public struct Instruction
        {
            public OpCode Op;
            public short[] Args;

            public Instruction(OpCode op, short[] args)
            {
                Op = op;
                Args = args;
            }
        }
    }
}
