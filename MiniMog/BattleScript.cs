using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiniMog
{
    class BattleScript
    {
        public static Dictionary<byte, OpCode> OpCodes;

        public List<Instruction> Init;
        public List<Instruction> Execute;
        public List<Instruction> Counter;
        public List<Instruction> Death;
        public List<Instruction> PreCounter;

        static BattleScript()
        {
            var opcodes = new OpCode[]
            {
                new OpCode("return", 0x00),
                new OpCode("text", 0x01, new Argument("textIndex")),
                new OpCode("if", 0x02, new Argument("conditionType"), new Argument("left"), new Argument("op"), new Argument("right", 2), new Argument("offset", 2)),
                new OpCode("target", 0x04, new Argument("battler")),
                new OpCode("unknown-05", 0x05, new Argument("arg")),
                new OpCode("die", 0x08),
                new OpCode("transform", 0x09, new Argument("form")),
                new OpCode("use-random", 0x0b, new Argument("abilityIndex1"), new Argument("abilityIndex2"), new Argument("abilityIndex3")),
                new OpCode("use", 0x0c, new Argument("abilityIndex")),
                new OpCode("set", 0x0e, new Argument("offset"), new Argument("value")),
                new OpCode("set-shared", 0x0f, new Argument("offset"), new Argument("value")),
                new OpCode("set-global", 0x11, new Argument("offset"), new Argument("value")),
                new OpCode("add", 0x12, new Argument("offset"), new Argument("value")),
                new OpCode("add-shared", 0x13, new Argument("offset"), new Argument("value")),
                new OpCode("add-global", 0x15, new Argument("offset"), new Argument("value")),
                new OpCode("hp-fill", 0x16),
                new OpCode("cannot-escape", 0x17, new Argument("switch")),
                new OpCode("text-like-ability", 0x18, new Argument("textIndex")),
                new OpCode("unknown-19", 0x19, new Argument("arg")),
                new OpCode("talk", 0x1a, new Argument("textIndex")),
                new OpCode("unknown-1b", 0x1b, new Argument("arg1"), new Argument("arg2")),
                new OpCode("wait-all", 0x1c, new Argument("count")),
                new OpCode("friend-remove", 0x1d, new Argument("battler")),
                new OpCode("effect", 0x1e, new Argument("effectId")),
                new OpCode("friend-add", 0x1f, new Argument("encounterIndex")),
                new OpCode("jmp", 0x23, new Argument("offset", 2)),
                new OpCode("atb-fill", 0x24),
                new OpCode("scan-text", 0x25, new Argument("textIndex")),
                new OpCode("target-random", 0x26, new Argument("unused"), new Argument("battlerGroup"), new Argument("unaryOp"), new Argument("status")),
                new OpCode("permanent-status", 0x27, new Argument("status"), new Argument("switch")),
                new OpCode("stat-change", 0x28, new Argument("stat"), new Argument("rate")),
                new OpCode("draw", 0x29),
                new OpCode("draw-cast", 0x2a),
                new OpCode("target-at", 0x2b, new Argument("battlerIndex")),
                new OpCode("unknown-2c", 0x2c),
                new OpCode("elem-def-change", 0x2d, new Argument("element"), new Argument("value", 2)),
                new OpCode("blow-away", 0x2e),
                new OpCode("untargetable", 0x30),
                new OpCode("award-gf", 0x31, new Argument("gfId")),
                new OpCode("unknown-32", 0x32),
                new OpCode("unknown-33", 0x33),
                new OpCode("unknown-34", 0x34, new Argument("arg")),
                new OpCode("unknown-35", 0x35, new Argument("arg")),
                new OpCode("award-gilgamesh", 0x36),
                new OpCode("award-card", 0x37, new Argument("cardId")),
                new OpCode("award-item", 0x38, new Argument("itemId")),
                new OpCode("game-over", 0x39),
                new OpCode("targetable-at", 0x3a, new Argument("battlerIndex")),
                new OpCode("friend-add-at", 0x3b, new Argument("encounterIndex"), new Argument("battlerIndex")),
                new OpCode("hp-add", 0x3c, new Argument("value")),
                new OpCode("award-omega", 0x3d)
            };

            OpCodes = new Dictionary<byte, OpCode>();
            foreach (var o in opcodes)
            {
                OpCodes.Add(o.Code, o);
            }
        }

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
                    if (a.Length == 1) args.Add(reader.ReadByte());
                    else args.Add(reader.ReadInt16());
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
                    return Args.Sum(a => a.Length) + 1;
                }
            }
        }

        public struct Argument
        {
            public string Name;
            public int Length;

            public Argument(string name, int length)
            {
                Name = name;
                Length = length;
            }

            public Argument(string name) : this(name, 1) { }
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
