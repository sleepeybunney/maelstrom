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
        public struct OpCode
        {
            public string Name;
            public byte Code;
            public uint Length;

            public OpCode(string name, byte code, uint length)
            {
                Name = name;
                Code = code;
                Length = length;
            }
        }

        public struct Instruction
        {
            public OpCode Op;
            public byte[] Args;

            public Instruction(OpCode op, byte[] args)
            {
                Op = op;
                Args = args;
            }
        }

        public static Dictionary<byte, OpCode> OpCodes = new Dictionary<byte, OpCode>()
        {
            { 0x00, new OpCode("return", 0x00, 1) },
            { 0x01, new OpCode("text", 0x01, 2) },
            { 0x02, new OpCode("if", 0x02, 8) },
            { 0x04, new OpCode("target", 0x04, 2) },
            { 0x05, new OpCode("unknown-05", 0x05, 2) },
            { 0x08, new OpCode("die", 0x08, 1) },
            { 0x09, new OpCode("transform", 0x09, 2) },
            { 0x0b, new OpCode("use-random", 0x0b, 4) },
            { 0x0c, new OpCode("use", 0x0c, 2) },
            { 0x0e, new OpCode("set", 0x0e, 3) },
            { 0x0f, new OpCode("set-shared", 0x0f, 3) },
            { 0x11, new OpCode("set-global", 0x11, 3) },
            { 0x12, new OpCode("add", 0x12, 3) },
            { 0x13, new OpCode("add-shared", 0x13, 3) },
            { 0x15, new OpCode("add-global", 0x15, 3) },
            { 0x16, new OpCode("hp-fill", 0x16, 1) },
            { 0x17, new OpCode("cannot-escape", 0x17, 2) },
            { 0x18, new OpCode("text-like-ability", 0x18, 2) },
            { 0x19, new OpCode("unknown-19", 0x19, 2) },
            { 0x1a, new OpCode("talk", 0x1a, 2) },
            { 0x1b, new OpCode("unknown-1b", 0x1b, 3) },
            { 0x1c, new OpCode("wait-all", 0x1c, 2) },
            { 0x1d, new OpCode("friend-remove", 0x1d, 2) },
            { 0x1e, new OpCode("effect", 0x1e, 2) },
            { 0x1f, new OpCode("friend-add", 0x1f, 2) },
            { 0x23, new OpCode("jmp", 0x23, 3) },
            { 0x24, new OpCode("atb-fill", 0x24, 1) },
            { 0x25, new OpCode("scan-text", 0x25, 2) },
            { 0x26, new OpCode("target-random", 0x26, 5) },
            { 0x27, new OpCode("permanent-status", 0x27, 3) },
            { 0x28, new OpCode("stat-change", 0x28, 3) },
            { 0x29, new OpCode("draw", 0x29, 1) },
            { 0x2a, new OpCode("draw-cast", 0x2a, 1) },
            { 0x2b, new OpCode("target-at", 0x2b, 2) },
            { 0x2c, new OpCode("unknown-2c", 0x2c, 1) },
            { 0x2d, new OpCode("elem-def-change", 0x2d, 4) },
            { 0x2e, new OpCode("blow-away", 0x2e, 1) },
            { 0x30, new OpCode("untargetable", 0x30, 1) },
            { 0x31, new OpCode("award-gf", 0x31, 2) },
            { 0x32, new OpCode("unknown-32", 0x32, 1) },
            { 0x33, new OpCode("unknown-33", 0x33, 1) },
            { 0x34, new OpCode("unknown-34", 0x34, 2) },
            { 0x35, new OpCode("unknown-35", 0x35, 2) },
            { 0x36, new OpCode("award-gilgamesh", 0x36, 1) },
            { 0x37, new OpCode("award-card", 0x37, 2) },
            { 0x38, new OpCode("award-item", 0x38, 2) },
            { 0x39, new OpCode("game-over", 0x39, 1) },
            { 0x3a, new OpCode("targetable-at", 0x3a, 2) },
            { 0x3b, new OpCode("friend-add-at", 0x3b, 3) },
            { 0x3c, new OpCode("hp-add", 0x3c, 2) },
            { 0x3d, new OpCode("award-omega", 0x3d, 1) }
        };

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

        public static BattleScript Load(byte[] data)
        {
            var result = new BattleScript();
            using (var stream = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var initOffset = reader.ReadUInt32();
                    var executeOffset = reader.ReadUInt32();
                    var counterOffset = reader.ReadUInt32();
                    var deathOffset = reader.ReadUInt32();
                    var preCounterOffset = reader.ReadUInt32();

                    Console.WriteLine("INIT");
                    stream.Seek(initOffset, SeekOrigin.Begin);
                    result.Init = ReadScript(reader);

                    Console.WriteLine("EXECUTE");
                    stream.Seek(executeOffset, SeekOrigin.Begin);
                    result.Execute = ReadScript(reader);

                    Console.WriteLine("COUNTER");
                    stream.Seek(counterOffset, SeekOrigin.Begin);
                    result.Counter = ReadScript(reader);

                    Console.WriteLine("DEATH");
                    stream.Seek(deathOffset, SeekOrigin.Begin);
                    result.Death = ReadScript(reader);

                    Console.WriteLine("PRECOUNTER");
                    stream.Seek(preCounterOffset, SeekOrigin.Begin);
                    result.PreCounter = ReadScript(reader);
                }
            }
            return result;
        }

        private static List<Instruction> ReadScript(BinaryReader reader)
        {
            var result = new List<Instruction>();
            byte code = 3;

            while (code != 0 && reader.BaseStream.Position < reader.BaseStream.Length)
            {
                code = reader.ReadByte();

                if (!OpCodes.ContainsKey(code))
                {
                    Console.WriteLine("Unknown op");
                    break;
                }

                var op = OpCodes[code];
                var args = reader.ReadBytes((int)op.Length - 1);
                result.Add(new Instruction(op, args));

                var log = new StringBuilder(op.Name);
                for (int i = 0; i < args.Length; i++)
                {
                    log.Append(" ");
                    log.Append(args[i]);
                }
                Console.WriteLine(log.ToString());
            }

            return result;
        }
    }
}
