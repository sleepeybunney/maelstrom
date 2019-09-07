using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8Mod.Battle
{
    public class Instruction
    {
        public OpCode Op;
        public short[] Args;

        public Instruction(OpCode op, short[] args)
        {
            Op = op;
            Args = args;
        }

        public Instruction(OpCode op) : this(op, Array.Empty<short>()) { }

        public Instruction() : this(OpCodes[0]) { }

        public byte[] Encode()
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

        private static Dictionary<byte, OpCode> BuildOpCodeDictionary()
        {
            var opcodes = new OpCode[]
            {
                new OpCode("return", 0x00),
                new OpCode("text", 0x01, new Argument("textIndex")),
                new OpCode("if", 0x02, new Argument("conditionType"), new Argument("left"), new Argument("op"), new Argument("right", ArgType.Short), new Argument("offset", ArgType.Short)),
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
                new OpCode("no-escape", 0x17, new Argument("switch", ArgType.Bool)),
                new OpCode("text-like-ability", 0x18, new Argument("textIndex")),
                new OpCode("unknown-19", 0x19, new Argument("arg")),
                new OpCode("talk", 0x1a, new Argument("textIndex")),
                new OpCode("unknown-1b", 0x1b, new Argument("arg1"), new Argument("arg2")),
                new OpCode("wait-all", 0x1c, new Argument("count")),
                new OpCode("friend-remove", 0x1d, new Argument("battler")),
                new OpCode("effect", 0x1e, new Argument("effectId")),
                new OpCode("friend-add", 0x1f, new Argument("encounterIndex")),
                new OpCode("jmp", 0x23, new Argument("offset", ArgType.Short)),
                new OpCode("atb-fill", 0x24),
                new OpCode("scan-text", 0x25, new Argument("textIndex")),
                new OpCode("target-random", 0x26, new Argument("unused"), new Argument("battlerGroup"), new Argument("unaryOp"), new Argument("status")),
                new OpCode("permanent-status", 0x27, new Argument("status"), new Argument("switch", ArgType.Bool)),
                new OpCode("stat-change", 0x28, new Argument("stat"), new Argument("rate")),
                new OpCode("draw", 0x29),
                new OpCode("draw-cast", 0x2a),
                new OpCode("target-at", 0x2b, new Argument("battlerIndex")),
                new OpCode("unknown-2c", 0x2c),
                new OpCode("elem-def-change", 0x2d, new Argument("element"), new Argument("value", ArgType.Short)),
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
                new OpCode("make-targetable", 0x3a, new Argument("battlerIndex")),
                new OpCode("friend-add-at", 0x3b, new Argument("encounterIndex"), new Argument("battlerIndex")),
                new OpCode("hp-add", 0x3c, new Argument("value")),
                new OpCode("award-omega", 0x3d)
            };

            var result = new Dictionary<byte, OpCode>();
            foreach (var o in opcodes)
            {
                result.Add(o.Code, o);
            }

            return result;
        }

        public static Dictionary<byte, OpCode> OpCodes = BuildOpCodeDictionary();
        public static Dictionary<string, OpCode> OpCodesReverse = OpCodes.ToDictionary(o => o.Value.Name, o => o.Value);

        public static Dictionary<byte, string> ConditionCodes = new Dictionary<byte, string>()
        {
            { 0x00, "target-hp" },
            { 0x01, "any-group-member-hp" },
            { 0x02, "random-number" },
            { 0x03, "encounter-id" },
            { 0x04, "target-status" },
            { 0x05, "any-group-member-status" },
            { 0x06, "number-in-group-alive" },
            { 0x09, "target-alive" },
            { 0x0a, "last-action" },
            { 0x0e, "level-band" },
            { 0x0f, "alive-at" },
            { 0x10, "any-group-member-gender" },
            { 0x11, "has-gf" },
            { 0x12, "odin-active" },
            { 0x13, "unknown-13" },
            { 0x14, "all-group-members-status" },
            { 0x50, "global-var-0" },
            { 0x51, "global-var-1" },
            { 0x52, "global-var-2" },
            { 0x53, "global-var-3" },
            { 0x54, "global-var-4" },
            { 0x55, "global-var-5" },
            { 0x56, "global-var-6" },
            { 0x57, "global-var-7" },
            { 0x60, "shared-var-0" },
            { 0x61, "shared-var-1" },
            { 0x62, "shared-var-2" },
            { 0x63, "shared-var-3" },
            { 0x64, "shared-var-4" },
            { 0x65, "shared-var-5" },
            { 0x66, "shared-var-6" },
            { 0x67, "shared-var-7" },
            { 0xdc, "var-0" },
            { 0xdd, "var-1" },
            { 0xde, "var-2" },
            { 0xdf, "var-3" },
            { 0xe0, "var-4" },
            { 0xe1, "var-5" },
            { 0xe2, "var-6" },
            { 0xe3, "var-7" }
        };

        public static byte[] ParameterisedConditions = new byte[]
        {
            0x00, 0x01, 0x02, 0x04, 0x05, 0x06, 0x10, 0x14
        };

        public static Dictionary<byte, string> ActionProperties = new Dictionary<byte, string>()
        {
            { 0x00, "type" },
            { 0x01, "subject" },
            { 0x02, "unknown-02" },
            { 0x03, "command" },
            { 0x04, "id" },
            { 0x05, "element" },
            { 0xcb, "unknown-cb" },
        };

        public static Dictionary<byte, string> Operators = new Dictionary<byte, string>()
        {
            { 0x00, "==" },
            { 0x01, "<" },
            { 0x02, ">" },
            { 0x03, "!=" },
            { 0x04, "<=" },
            { 0x05, ">=" },
        };

        public static Dictionary<byte, string> UnaryOperators = new Dictionary<byte, string>()
        {
            { 0x00, "==" },
            { 0x03, "!=" }
        };

        public static Dictionary<byte, string> Elements = new Dictionary<byte, string>()
        {
            { 0x00, "fire" },
            { 0x01, "ice" },
            { 0x02, "lightning" },
            { 0x03, "earth" },
            { 0x04, "poison" },
            { 0x05, "wind" },
            { 0x06, "water" },
            { 0x07, "holy" },
        };

        public static Dictionary<byte, string> StatusCodes = new Dictionary<byte, string>()
        {
            { 0x00, "ko" },
            { 0x01, "poisoned" },
            { 0x02, "petrify" },
            { 0x03, "darkness" },
            { 0x04, "silence" },
            { 0x05, "berserk" },
            { 0x06, "zombie" },
            { 0x07, "unknown-07" },
            { 0x08, "hp-critical" },
            { 0x09, "hp-half" },
            { 0x10, "sleep" },
            { 0x11, "haste" },
            { 0x12, "slow" },
            { 0x13, "stop" },
            { 0x14, "regen" },
            { 0x15, "protect" },
            { 0x16, "shell" },
            { 0x17, "reflect" },
            { 0x18, "aura" },
            { 0x19, "curse" },
            { 0x1a, "doom" },
            { 0x1b, "invincible" },
            { 0x1c, "slow-petrify" },
            { 0x1d, "float" },
            { 0x1e, "confuse" },
            { 0x1f, "drain" },
            { 0x20, "eject" },
            { 0x21, "double" },
            { 0x22, "triple" },
            { 0x23, "defend" },
            { 0x26, "charge" },
            { 0x28, "vit-0" },
            { 0x2e, "angel-wing" },
            { 0x2f, "summoning" },
            { 0xc8, "male" },
            { 0xc9, "female" },
            { 0xcb, "hp-max" },
            { 0xcc, "hp-min" },
            { 0xd1, "speed-max" },
            { 0xd5, "vit-max" },
            { 0xd6, "vit-min" },
            { 0xe5, "fire-def-min" },
            { 0xe6, "ice-def-min" },
            { 0xe7, "lightning-def-min" },
            { 0xe8, "earth-def-min" },
            { 0xe9, "poison-def-min" },
            { 0xea, "wind-def-min" },
            { 0xeb, "water-def-min" },
            { 0xec, "holy-def-min" },
        };

        public static Dictionary<byte, string> Targets = new Dictionary<byte, string>()
        {
            { 0x00, "squall" },
            { 0x01, "zell" },
            { 0x02, "irvine" },
            { 0x03, "quistis" },
            { 0x04, "rinoa" },
            { 0x05, "selphie" },
            { 0x06, "seifer" },
            { 0x07, "edea" },
            { 0x08, "laguna" },
            { 0x09, "kiros" },
            { 0x0a, "ward" },
            // 0x10-0x9e = enemy-id + 0x10
            { 0xc8, "self" },
            { 0xc9, "random-enemy" },
            { 0xca, "random-friend" },
            { 0xcb, "last-action-subject" },
            { 0xcc, "all-enemies" },
            { 0xcd, "all-friends" },
            { 0xcf, "random-other-friend" },
            { 0xd0, "random-enemies" },
            { 0xd1, "newest-battler" }
            // 0xdc-0xe3 = var + 0xdc
        };

        public static Dictionary<byte, string> Groups = new Dictionary<byte, string>()
        {
            // 0x10-0x9e = enemy-id + 0x10
            { 0xc8, "enemies" },
            { 0xc9, "friends" }
        };

        public static Dictionary<byte, string> Friends = new Dictionary<byte, string>()
        {
            // 0x00-0x07 = friend-index
            { 0xc8, "self" }
        };

        public static Dictionary<byte, string> ActionTypes = new Dictionary<byte, string>()
        {
            { 0x00, "physical" },
            { 0x01, "magic" }
        };

        public static Dictionary<byte, string> MenuCommands = new Dictionary<byte, string>()
        {
            { 0x01, "attack" },
            { 0x02, "magic" },
            { 0x04, "item" },
            { 0x06, "draw" },
            { 0xfe, "gf" }
        };

        public static Dictionary<byte, string> LevelBands = new Dictionary<byte, string>()
        {
            { 0x00, "low" },
            { 0x01, "medium" },
            { 0x02, "high" }
        };

        public static Dictionary<byte, string> Genders = new Dictionary<byte, string>()
        {
            { 0xca, "male" },
            { 0xcb, "female" }
        };

        public static Dictionary<byte, string> PercentageModifiers = new Dictionary<byte, string>()
        {
            { 0x00, "0%" },
            { 0x01, "10%" },
            { 0x02, "20%" },
            { 0x03, "30%" },
            { 0x04, "40%" },
            { 0x05, "50%" },
            { 0x06, "60%" },
            { 0x07, "70%" },
            { 0x08, "80%" },
            { 0x09, "90%" },
            { 0x0a, "25%" }
        };

        public static string GetConstant(Dictionary<byte, string> map, byte code, object defaultOutput)
        {
            if (map.Keys.Contains(code)) return map[code];
            else return defaultOutput.ToString();
        }

        public static string GetConstant(Dictionary<byte, string> map, short code, object defaultOutput)
        {
            return GetConstant(map, (byte)code, defaultOutput);
        }

        public static string GetConstant(Dictionary<byte, string> map, byte code)
        {
            return GetConstant(map, code, code);
        }

        public static string GetConstant(Dictionary<byte, string> map, short code)
        {
            return GetConstant(map, (byte)code, code);
        }
    }
}
