using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sleepey.FF8Mod.Archive;

namespace Sleepey.FF8Mod.Field
{
    public class FieldScript
    {
        public List<Entity> Entities { get; set; } = new List<Entity>();

        public FieldScript() { }

        // construct from binary data (ie. game files)
        public static FieldScript FromBytes(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                var doorCount = reader.ReadByte();
                var lineCount = reader.ReadByte();
                var backgroundCount = reader.ReadByte();
                var otherCount = reader.ReadByte();
                var totalCount = doorCount + lineCount + backgroundCount + otherCount;

                var scriptInfoOffset = reader.ReadUInt16();
                var scriptDataOffset = reader.ReadUInt16();

                // script counts & labels for each entity
                var entityInfo = new List<EntityInfo>();
                for (var i = 0; i < totalCount; i++)
                {
                    // entity types are inferred from their order in the file
                    var type = EntityType.Line;
                    if (i >= lineCount) type = EntityType.Door;
                    if (i >= lineCount + doorCount) type = EntityType.Background;
                    if (i >= lineCount + doorCount + backgroundCount) type = EntityType.Other;

                    entityInfo.Add(new EntityInfo(type, reader.ReadUInt16()));
                }

                // script offsets within the data block
                var scriptInfo = new List<ScriptInfo>();
                var scriptCount = (scriptDataOffset - scriptInfoOffset) / 2;
                for (var i = 0; i < scriptCount; i++)
                {
                    scriptInfo.Add(new ScriptInfo(reader.ReadUInt16()));
                }

                // script data
                var scripts = new List<List<FieldScriptInstruction>>();
                for (int i = 0; i < scriptCount - 1; i++)
                {
                    var script = new List<FieldScriptInstruction>();
                    while (stream.Position < scriptDataOffset + scriptInfo[i + 1].Position)
                    {
                        script.Add(new FieldScriptInstruction(reader.ReadBytes(4)));
                    }
                    scripts.Add(script);
                }

                // put it all together
                var result = new FieldScript();
                for (int i = 0; i < entityInfo.Count; i++)
                {
                    var entityScripts = new List<Script>();
                    for (int j = 0; j <= entityInfo[i].ScriptCount; j++)
                    {
                        var instructions = scripts[entityInfo[i].Label + j];
                        var flag = scriptInfo[entityInfo[i].Label + j].Flag == 1;
                        entityScripts.Add(new Script(instructions, flag));
                    }
                    result.Entities.Add(new Entity(entityInfo[i].Type, entityScripts));
                }
                return result;
            }
        }

        // extract from field archive & construct
        public static FieldScript FromSource(FileSource fieldSource, string fieldName)
        {
            var fieldPath = GetFieldPath(fieldName);
            var innerSource = new FileSource(fieldPath, fieldSource);
            return FromBytes(innerSource.GetFile(fieldPath + "\\" + fieldName + Env.ScriptFileExtension));
        }

        public static string GetFieldPath(string fieldName)
        {
            var abbrev = fieldName.Substring(0, 2);
            return string.Format(Env.DataPath + @"\field\mapdata\{0}\{1}", abbrev, fieldName);
        }

        // jsm output
        public IEnumerable<byte> Encode()
        {
            var doorCount = 0;
            var lineCount = 0;
            var backgroundCount = 0;
            var otherCount = 0;
            var allScripts = new List<Script>();

            foreach (var e in Entities)
            {
                switch (e.Type)
                {
                    case EntityType.Door:
                        doorCount++;
                        break;
                    case EntityType.Line:
                        lineCount++;
                        break;
                    case EntityType.Background:
                        backgroundCount++;
                        break;
                    default:
                        otherCount++;
                        break;
                }

                allScripts.AddRange(e.Scripts);
            }

            allScripts = allScripts.OrderBy(s => s.Instructions[0].Param).ToList();

            var entityInfoLength = Entities.Count * 2;                              // 2 bytes per entity
            var scriptInfoLength = allScripts.Count * 2 + 2;                        // 2 bytes per script + an extra entry to denote EOF
            var scriptDataLength = allScripts.Sum(s => s.Instructions.Count * 4);   // 4 bytes per instruction
            var totalLength = 8 + entityInfoLength + scriptInfoLength + scriptDataLength;

            var scriptInfoOffset = entityInfoLength + 8;                            // entity counts above + these two offsets = 8 bytes
            var scriptDataOffset = scriptInfoLength + scriptInfoOffset;

            var result = new byte[totalLength];

            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte)doorCount);
                writer.Write((byte)lineCount);
                writer.Write((byte)backgroundCount);
                writer.Write((byte)otherCount);
                writer.Write((short)scriptInfoOffset);
                writer.Write((short)scriptDataOffset);

                // entity info block
                // todo: make sure everything's still in the right order (entity types -> script labels)
                foreach (var e in Entities)
                {
                    writer.Write(new EntityInfo(e).Encode());
                }

                // script info block (& gathering script data while we're here)
                var scriptData = new List<int>();
                foreach (var s in allScripts)
                {
                    var info = new ScriptInfo(scriptData.Count * 4, s.MysteryFlag ? 1 : 0);
                    writer.Write(info.Encode());

                    foreach (var i in s.Instructions)
                    {
                        scriptData.Add(i.Encode());
                    }
                }

                // extra eof entry
                writer.Write(new ScriptInfo(scriptData.Count * 4, 0).Encode());

                // script data block
                foreach (var s in scriptData)
                {
                    writer.Write(s);
                }
            }

            return result;
        }

        // overwrite a script with interpreted instructions
        public void ReplaceScript(int entity, int script, string scriptText)
        {
            var label = Entities[entity].Scripts[script].Instructions[0].Param;
            var newScript = new Script(scriptText);
            newScript.Instructions.Insert(0, new FieldScriptInstruction(FieldScript.OpCodesReverse["lbl"], label));
            newScript.Instructions.Add(new FieldScriptInstruction(FieldScript.OpCodesReverse["ret"], 8));
            Entities[entity].Scripts[script].Instructions = newScript.Instructions;
        }

        public void SaveToSource(FileSource fieldSource, string fieldName)
        {
            var fieldPath = GetFieldPath(fieldName);
            var innerSource = new FileSource(fieldPath, fieldSource);
            innerSource.ReplaceFile(fieldPath + "\\" + fieldName + Env.ScriptFileExtension, Encode());
        }

        public static Dictionary<int, string> OpCodes { get; } = new Dictionary<int, string>()
        {
            { 0x00, "nop" },
            { 0x01, "cal" },
            { 0x02, "jmp" },
            { 0x03, "jpf" },
            { 0x04, "gjmp" },
            { 0x05, "lbl" },
            { 0x06, "ret" },
            { 0x07, "pshn_l" },
            { 0x08, "pshi_l" },
            { 0x09, "popi_l" },
            { 0x0a, "pshm_b" },
            { 0x0b, "popm_b" },
            { 0x0c, "pshm_w" },
            { 0x0d, "popm_w" },
            { 0x0e, "pshm_l" },
            { 0x0f, "popm_l" },
            { 0x10, "pshsm_b" },
            { 0x11, "pshsm_w" },
            { 0x12, "pshsm_l" },
            { 0x13, "pshac" },
            { 0x14, "req" },
            { 0x15, "reqsw" },
            { 0x16, "reqew" },
            { 0x17, "preq" },
            { 0x18, "preqsw" },
            { 0x19, "preqew" },
            { 0x1a, "unuse" },
            { 0x1b, "debug" },
            { 0x1c, "halt" },
            { 0x1d, "set" },
            { 0x1e, "set3" },
            { 0x1f, "idlock" },
            { 0x20, "idunlock" },
            { 0x21, "effectplay2" },
            { 0x22, "footstep" },
            { 0x23, "jump" },
            { 0x24, "jump3" },
            { 0x25, "ladderup" },
            { 0x26, "ladderdown" },
            { 0x27, "ladderup2" },
            { 0x28, "ladderdown2" },
            { 0x29, "mapjump" },
            { 0x2a, "mapjump3" },
            { 0x2b, "setmodel" },
            { 0x2c, "baseanime" },
            { 0x2d, "anime" },
            { 0x2e, "animekeep" },
            { 0x2f, "canime" },
            { 0x30, "canimekeep" },
            { 0x31, "ranime" },
            { 0x32, "ranimekeep" },
            { 0x33, "rcanime" },
            { 0x34, "rcanimekeep" },
            { 0x35, "ranimeloop" },
            { 0x36, "rcanimeloop" },
            { 0x37, "ladderanime" },
            { 0x38, "discjump" },
            { 0x39, "setline" },
            { 0x3a, "lineon" },
            { 0x3b, "lineoff" },
            { 0x3c, "wait" },
            { 0x3d, "mspeed" },
            { 0x3e, "move" },
            { 0x3f, "movea" },
            { 0x40, "pmovea" },
            { 0x41, "cmove" },
            { 0x42, "fmove" },
            { 0x43, "pjumpa" },
            { 0x44, "animesync" },
            { 0x45, "animestop" },
            { 0x46, "mesw" },
            { 0x47, "mes" },
            { 0x48, "messync" },
            { 0x49, "mesvar" },
            { 0x4a, "ask" },
            { 0x4b, "winsize" },
            { 0x4c, "winclose" },
            { 0x4d, "ucon" },
            { 0x4e, "ucoff" },
            { 0x4f, "movie" },
            { 0x50, "moviesync" },
            { 0x51, "setpc" },
            { 0x52, "dir" },
            { 0x53, "dirp" },
            { 0x54, "dira" },
            { 0x55, "pdira" },
            { 0x56, "spuready" },
            { 0x57, "talkon" },
            { 0x58, "talkoff" },
            { 0x59, "pushon" },
            { 0x5a, "pushoff" },
            { 0x5b, "istouch" },
            { 0x5c, "mapjumpo" },
            { 0x5d, "mapjumpon" },
            { 0x5e, "mapjumpoff" },
            { 0x5f, "setmesspeed" },
            { 0x60, "show" },
            { 0x61, "hide" },
            { 0x62, "talkradius" },
            { 0x63, "pushradius" },
            { 0x64, "amesw" },
            { 0x65, "ames" },
            { 0x66, "getinfo" },
            { 0x67, "throughon" },
            { 0x68, "throughoff" },
            { 0x69, "battle" },
            { 0x6a, "battleresult" },
            { 0x6b, "battleon" },
            { 0x6c, "battleoff" },
            { 0x6d, "keyscan" },
            { 0x6e, "keyon" },
            { 0x6f, "aask" },
            { 0x70, "pgetinfo" },
            { 0x71, "dscroll" },
            { 0x72, "lscroll" },
            { 0x73, "cscroll" },
            { 0x74, "dscrolla" },
            { 0x75, "lscrolla" },
            { 0x76, "cscrolla" },
            { 0x77, "scrollsync" },
            { 0x78, "rmove" },
            { 0x79, "rmovea" },
            { 0x7a, "rpmovea" },
            { 0x7b, "rcmove" },
            { 0x7c, "rfmove" },
            { 0x7d, "movesync" },
            { 0x7e, "clear" },
            { 0x7f, "dscrollp" },
            { 0x80, "lscrollp" },
            { 0x81, "cscrollp" },
            { 0x82, "lturnr" },
            { 0x83, "lturnl" },
            { 0x84, "cturnr" },
            { 0x85, "cturnl" },
            { 0x86, "addparty" },
            { 0x87, "subparty" },
            { 0x88, "changeparty" },
            { 0x89, "refreshparty" },
            { 0x8a, "setparty" },
            { 0x8b, "isparty" },
            { 0x8c, "addmember" },
            { 0x8d, "submember" },
            { 0x8e, "ismember" },
            { 0x8f, "lturn" },
            { 0x90, "cturn" },
            { 0x91, "plturn" },
            { 0x92, "pcturn" },
            { 0x93, "join" },
            { 0x94, "mesfocus" },
            { 0x95, "bganime" },
            { 0x96, "rbganime" },
            { 0x97, "rbganimeloop" },
            { 0x98, "bganimesync" },
            { 0x99, "bgdraw" },
            { 0x9a, "bgoff" },
            { 0x9b, "bganimespeed" },
            { 0x9c, "settimer" },
            { 0x9d, "disptimer" },
            { 0x9e, "shadetimer" },
            { 0x9f, "setgeta" },
            { 0xa0, "setroottrans" },
            { 0xa1, "setvibrate" },
            { 0xa2, "stopvibrate" },
            { 0xa3, "movieready" },
            { 0xa4, "gettimer" },
            { 0xa5, "fadein" },
            { 0xa6, "fadeout" },
            { 0xa7, "fadesync" },
            { 0xa8, "shake" },
            { 0xa9, "shakeoff" },
            { 0xaa, "fadeblack" },
            { 0xab, "followoff" },
            { 0xac, "followon" },
            { 0xad, "gameover" },
            { 0xae, "ending" },
            { 0xaf, "shadelevel" },
            { 0xb0, "shadeform" },
            { 0xb1, "fmovea" },
            { 0xb2, "fmovep" },
            { 0xb3, "shadeset" },
            { 0xb4, "musicchange" },
            { 0xb5, "musicload" },
            { 0xb6, "fadenone" },
            { 0xb7, "polycolor" },
            { 0xb8, "polycolorall" },
            { 0xb9, "killtimer" },
            { 0xba, "crossmusic" },
            { 0xbb, "dualmusic" },
            { 0xbc, "effectplay" },
            { 0xbd, "effectload" },
            { 0xbe, "loadsync" },
            { 0xbf, "musicstop" },
            { 0xc0, "musicvol" },
            { 0xc1, "musicvoltrans" },
            { 0xc2, "musicvolfade" },
            { 0xc3, "allsevol" },
            { 0xc4, "allsevoltrans" },
            { 0xc5, "allsepos" },
            { 0xc6, "allsepostrans" },
            { 0xc7, "sevol" },
            { 0xc8, "sevoltrans" },
            { 0xc9, "sepos" },
            { 0xca, "sepostrans" },
            { 0xcb, "setbattlemusic" },
            { 0xcc, "battlemode" },
            { 0xcd, "sestop" },
            { 0xce, "bganimeflag" },
            { 0xcf, "initsound" },
            { 0xd0, "bgshade" },
            { 0xd1, "bgshadestop" },
            { 0xd2, "rbgshadeloop" },
            { 0xd3, "dscroll2" },
            { 0xd4, "lscroll2" },
            { 0xd5, "cscroll2" },
            { 0xd6, "dscrolla2" },
            { 0xd7, "lscrolla2" },
            { 0xd8, "cscrolla2" },
            { 0xd9, "dscrollp2" },
            { 0xda, "lscrollp2" },
            { 0xdb, "cscrollp2" },
            { 0xdc, "scrollsync2" },
            { 0xdd, "scrollmode2" },
            { 0xde, "menuenable" },
            { 0xdf, "menudisable" },
            { 0xe0, "footstepon" },
            { 0xe1, "footstepoff" },
            { 0xe2, "footstepoffall" },
            { 0xe3, "footstepcut" },
            { 0xe4, "premapjump" },
            { 0xe5, "use" },
            { 0xe6, "split" },
            { 0xe7, "animespeed" },
            { 0xe8, "rnd" },
            { 0xe9, "dcoladd" },
            { 0xea, "dcolsub" },
            { 0xeb, "tcoladd" },
            { 0xec, "tcolsub" },
            { 0xed, "fcoladd" },
            { 0xee, "fcolsub" },
            { 0xef, "colsync" },
            { 0xf0, "doffset" },
            { 0xf1, "loffsets" },
            { 0xf2, "coffsets" },
            { 0xf3, "loffset" },
            { 0xf4, "coffset" },
            { 0xf5, "offsetsync" },
            { 0xf6, "runenable" },
            { 0xf7, "rundisable" },
            { 0xf8, "mapfadeoff" },
            { 0xf9, "mapfadeon" },
            { 0xfa, "inittrace" },
            { 0xfb, "setdress" },
            { 0xfc, "getdress" },
            { 0xfd, "facedir" },
            { 0xfe, "facedira" },
            { 0xff, "facedirp" },
            { 0x100, "facedirlimit" },
            { 0x101, "facediroff" },
            { 0x102, "salaryoff" },
            { 0x103, "salaryon" },
            { 0x104, "salarydispoff" },
            { 0x105, "salarydispon" },
            { 0x106, "mesmode" },
            { 0x107, "facedirinit" },
            { 0x108, "facediri" },
            { 0x109, "junction" },
            { 0x10a, "setcamera" },
            { 0x10b, "battlecut" },
            { 0x10c, "footstepcopy" },
            { 0x10d, "worldmapjump" },
            { 0x10e, "rfacediri" },
            { 0x10f, "rfacedir" },
            { 0x110, "rfacedira" },
            { 0x111, "rfacedirp" },
            { 0x112, "rfacediroff" },
            { 0x113, "rfacedirsync" },
            { 0x114, "copyinfo" },
            { 0x115, "pcopyinfo" },
            { 0x116, "ramesw" },
            { 0x117, "bgshadeoff" },
            { 0x118, "axis" },
            { 0x119, "axissync" },
            { 0x11a, "menunormal" },
            { 0x11b, "menuphs" },
            { 0x11c, "bgclear" },
            { 0x11d, "getparty" },
            { 0x11e, "menushop" },
            { 0x11f, "disc" },
            { 0x120, "dscroll3" },
            { 0x121, "lscroll3" },
            { 0x122, "cscroll3" },
            { 0x123, "maccel" },
            { 0x124, "mlimit" },
            { 0x125, "additem" },
            { 0x126, "setwitch" },
            { 0x127, "setodin" },
            { 0x128, "resetgf" },
            { 0x129, "menuname" },
            { 0x12a, "rest" },
            { 0x12b, "movecancel" },
            { 0x12c, "pmovecancel" },
            { 0x12d, "actormode" },
            { 0x12e, "menusave" },
            { 0x12f, "saveenable" },
            { 0x130, "phsenable" },
            { 0x131, "hold" },
            { 0x132, "moviecut" },
            { 0x133, "setplace" },
            { 0x134, "setdcamera" },
            { 0x135, "choicemusic" },
            { 0x136, "getcard" },
            { 0x137, "drawpoint" },
            { 0x138, "phspower" },
            { 0x139, "key" },
            { 0x13a, "cardgame" },
            { 0x13b, "setbar" },
            { 0x13c, "dispbar" },
            { 0x13d, "killbar" },
            { 0x13e, "scrollratio2" },
            { 0x13f, "whoami" },
            { 0x140, "musicstatus" },
            { 0x141, "musicreplay" },
            { 0x142, "doorlineoff" },
            { 0x143, "doorlineon" },
            { 0x144, "musicskip" },
            { 0x145, "dying" },
            { 0x146, "sethp" },
            { 0x147, "gethp" },
            { 0x148, "moveflush" },
            { 0x149, "musicvolsync" },
            { 0x14a, "pushanime" },
            { 0x14b, "popanime" },
            { 0x14c, "keyscan2" },
            { 0x14d, "keyon2" },
            { 0x14e, "particleon" },
            { 0x14f, "particleoff" },
            { 0x150, "keysignchange" },
            { 0x151, "addgil" },
            { 0x152, "addpastgil" },
            { 0x153, "addseedlevel" },
            { 0x154, "particleset" },
            { 0x155, "setdrawpoint" },
            { 0x156, "menutips" },
            { 0x157, "lastin" },
            { 0x158, "lastout" },
            { 0x159, "sealedoff" },
            { 0x15a, "menututo" },
            { 0x15b, "openeyes" },
            { 0x15c, "closeeyes" },
            { 0x15d, "blinkeyes" },
            { 0x15e, "setcard" },
            { 0x15f, "howmanycard" },
            { 0x160, "wherecard" },
            { 0x161, "addmagic" },
            { 0x162, "swap" },
            { 0x163, "setparty2" },
            { 0x164, "spusync" },
            { 0x165, "broken" },
            { 0x166, "unknown166" },
            { 0x167, "unknown167" },
            { 0x168, "unknown168" },
            { 0x169, "unknown169" },
            { 0x16a, "unknown16a" },
            { 0x16b, "unknown16b" },
            { 0x16c, "unknown16c" },
            { 0x16d, "unknown16d" },
            { 0x16e, "unknown16e" },
            { 0x16f, "unknown16f" },
            { 0x170, "unknown170" },
            { 0x171, "unknown171" },
            { 0x172, "unknown172" },
            { 0x173, "unknown173" },
            { 0x174, "unknown174" },
            { 0x175, "unknown175" },
            { 0x176, "premapjump2" },
            { 0x177, "tuto" },
            { 0x178, "unknown178" },
            { 0x179, "unknown179" },
            { 0x17a, "unknown17a" },
            { 0x17b, "unknown17b" },
            { 0x17c, "unknown17c" },
            { 0x17d, "unknown17d" },
            { 0x17e, "unknown17e" },
            { 0x17f, "unknown17f" },
            { 0x180, "unknown180" },
            { 0x181, "unknown181" },
            { 0x182, "unknown182" },
            { 0x183, "unknown183" },
        };

        public static Dictionary<string, int> OpCodesReverse { get; } = OpCodes.ToDictionary(o => o.Value, o => o.Key);
    }
}
