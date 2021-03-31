using System;
using System.Collections.Generic;
using System.Linq;

namespace Sleepey.FF8Mod.Field
{
    // code attached to an entity
    public class Script
    {
        public List<FieldScriptInstruction> Instructions { get; set; } = new List<FieldScriptInstruction>();
        public bool MysteryFlag { get; set; } = false;

        public Script() { }

        public Script(IEnumerable<FieldScriptInstruction> instructions, bool flag)
        {
            Instructions = instructions.ToList();
            MysteryFlag = flag;
        }

        public Script(string instructions)
        {
            var lines = instructions.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var tokens = line.Split(null, 2).Select(t => t.Trim().ToLower()).Where(t => !string.IsNullOrEmpty(t)).ToList();
                if (!FieldScript.OpCodesReverse.Keys.Contains(tokens[0]))
                {
                    throw new Exception("unrecognised operation in fieldscript file (" + instructions + "): " + line);
                }
                var instruction = new FieldScriptInstruction(FieldScript.OpCodesReverse[tokens[0]]);

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
}
