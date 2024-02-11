using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Field
{
    public class FieldScriptInstruction : IEquatable<FieldScriptInstruction>
    {
        public int OpCode { get; set; } = 0;
        public int Param { get; set; } = 0;
        public bool HasParam { get; set; } = false;

        public FieldScriptInstruction(int opCode) { OpCode = opCode; }

        public FieldScriptInstruction(int opCode, int param) : this(opCode)
        {
            Param = param;
            HasParam = true;
        }

        public FieldScriptInstruction(IEnumerable<byte> instruction)
        {
            var value = BitConverter.ToInt32(instruction.ToArray(), 0);
            HasParam = (value & 0xff000000) != 0;

            if (HasParam)
            {
                OpCode = value >> 24;
                Param = unchecked((short)(value & 0x00ffffff));
            }
            else
            {
                OpCode = value;
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

        public override bool Equals(object obj)
        {
            return Equals(obj as FieldScriptInstruction);
        }

        public bool Equals(FieldScriptInstruction other)
        {
            return !(other is null) &&
                OpCode == other.OpCode &&
                (!HasParam || Param == other.Param);
        }

        public override int GetHashCode()
        {
            int hashCode = 823597393;
            hashCode = hashCode * -1521134295 + OpCode.GetHashCode();
            hashCode = hashCode * -1521134295 + (HasParam ? Param : -1).GetHashCode();
            return hashCode;
        }

        public static bool operator ==(FieldScriptInstruction lhs, FieldScriptInstruction rhs) { return lhs.Equals(rhs); }
        public static bool operator !=(FieldScriptInstruction lhs, FieldScriptInstruction rhs) {  return !lhs.Equals(rhs); }
    }
}
