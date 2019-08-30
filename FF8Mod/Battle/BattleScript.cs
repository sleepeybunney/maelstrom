using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8Mod.Battle
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

            // empty scripts aren't empty, they still return
            Init.Add(new Instruction());
            Execute.Add(new Instruction());
            Counter.Add(new Instruction());
            Death.Add(new Instruction());
            PreCounter.Add(new Instruction());
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
                if (!Instruction.OpCodes.ContainsKey(code))
                {
                    Console.WriteLine("Unknown op: " + code);
                    break;
                }

                var op = Instruction.OpCodes[code];
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

        public byte[] Encode()
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

        private byte[] EncodeScript(List<Instruction> script)
        {
            var result = new List<byte>();
            foreach (var instruction in script)
            {
                result.AddRange(instruction.Encode());
            }
            return result.ToArray();
        }
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

    public enum ArgType
    {
        Byte,
        Short,
        Bool
    }
}
