using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Field;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Exe;

namespace Sleepey.Maelstrom
{
    public static class FreeRoam
    {
        public static BinaryPatch IntroPatch = new BinaryPatch(0x273fb, new byte[] { 0x33, 0x30 }, new byte[] { 0x30, 0x31 });
        private const int codenameLength = 16;

        public static void Apply(FileSource fieldSource, string seedString)
        {
            // something in the steam 2013 version tracks your progress by watching for certain movie files
            // getting played - not sure exactly what it's for but if we change the intro movie we need to
            // update this as well or it gets very unstable
            if (!Env.Remastered) IntroPatch.Apply(Env.Af3dnPath);

            // replace liberi fatali intro with quistis walking through a door
            ImportScript(fieldSource, "start0", 0, 1);

            // brief conversation in the infirmary, receive 2 GFs and a party member
            ImportScript(fieldSource, "bghoke_2", 12, 1);
            ImportScript(fieldSource, "bghoke_2", 6, 4);

            // show seed value, slightly different wording if numeric
            // (note: numbers above int_max count as numeric here, despite actually being treated as strings for rng purposes)
            var seedText = "ID #{0}...";
            if (!long.TryParse(seedString, out _)) seedText = "Codename: '{0}`...";
            var seedFinal = string.Format(seedText, seedString.Substring(0, Math.Min(codenameLength, seedString.Length)));
            SetText(fieldSource, "bghoke_2", 17, "Quistis{02}“Another random SeeD?{02} " + seedFinal);

            // activate garden directory
            CopyEntity(fieldSource, "bghall_1", 0, "bghall1b", 0);
            ImportScript(fieldSource, "bghall_7", 1, 1);

            // open garden gate
            ImportScript(fieldSource, "bggate_6", 3, 7);
            ImportScript(fieldSource, "bggate_6", 4, 7);

            // tutorial at the front gate
            DeleteEntity(fieldSource, "bggate_1", 0);

            // fire cavern tutorials & dialogue with quistis
            DeleteEntity(fieldSource, "bdview1", 0);

            // faculty fellas at the cave entrance
            DeleteEntity(fieldSource, "bdview1", 11);
            DeleteEntity(fieldSource, "bdview1", 12);
            DeleteEntity(fieldSource, "bdenter1", 9);
            DeleteEntity(fieldSource, "bdenter1", 10);

            // fire cavern dialogue & timers
            DeleteScript(fieldSource, "bdin1", 11, 1);
            DeleteScript(fieldSource, "bdin2", 11, 1);
            DeleteScript(fieldSource, "bdin3", 11, 1);
            DeleteScript(fieldSource, "bdin4", 12, 1);
            DeleteScript(fieldSource, "bdin5", 15, 1);
            DeleteEntity(fieldSource, "bdin5", 0);
            DeleteScript(fieldSource, "bdifrit1", 15, 1);
            ImportScript(fieldSource, "bdifrit1", 0, 0);
            ImportScript(fieldSource, "bdifrit1", 0, 5);
            ImportScript(fieldSource, "bdifrit1", 14, 0);
            ImportScript(fieldSource, "bdifrit1", 14, 4);

            // training centre boss
            CopyParticle(fieldSource, "fewor1", "bgmon_1");
            ImportScript(fieldSource, "bgmon_1", 16, 0);
            ImportScript(fieldSource, "bgmon_1", 0, 0);
            DeleteScript(fieldSource, "bgmon_1", 0, 1);
            ImportScript(fieldSource, "bgmon_1", 0, 7);

            // balamb hotel boss
            CopyParticle(fieldSource, "fewor1", "bchtl_1");
            ImportScript(fieldSource, "bchtl_1", 0, 0);
            ImportScript(fieldSource, "bchtl_1", 0, 5);
            ImportScript(fieldSource, "bchtl_1", 3, 0);

            // deling city station escalators
            ImportScript(fieldSource, "glsta1", 0, 7);
            ImportScript(fieldSource, "glsta1", 1, 7);
        }

        public static void Remove()
        {
            if (!Env.Remastered) IntroPatch.Remove(Env.Af3dnPath);
        }

        // overwrite a field script with one imported from an embedded text file
        public static void ImportScript(FileSource fieldSource, string fieldName, int entity, int script, string importPath)
        {
            var field = FieldScript.FromSource(fieldSource, fieldName);
            field.ReplaceScript(entity, script, App.ReadEmbeddedFile(importPath));
            field.SaveToSource(fieldSource, fieldName);
        }

        // slightly easier import with the filename convention "fieldName.entityID.scriptID.txt"
        public static void ImportScript(FileSource fieldSource, string fieldName, int entity, int script)
        {
            ImportScript(fieldSource, fieldName, entity, script, string.Format(@"Sleepey.Maelstrom.FieldScripts.{0}.{1}.{2}.txt", fieldName, entity, script));
        }

        // delete a single script
        public static void DeleteScript(FileSource fieldSource, string fieldName, int entity, int script)
        {
            var field = FieldScript.FromSource(fieldSource, fieldName);
            field.ReplaceScript(entity, script, "");
            field.SaveToSource(fieldSource, fieldName);
        }

        // overwrite an entity on one field with a copy of an entity from another
        public static void CopyEntity(FileSource fieldSource, string srcFieldName, int srcEntity, string destFieldName, int destEntity)
        {
            var srcField = FieldScript.FromSource(fieldSource, srcFieldName);
            var destField = FieldScript.FromSource(fieldSource, destFieldName);

            for (int i = 0; i < destField.Entities[destEntity].Scripts.Count; i++)
            {
                if (i < srcField.Entities[srcEntity].Scripts.Count)
                {
                    var src = srcField.Entities[srcEntity].Scripts[i].Instructions;
                    var dest = destField.Entities[destEntity].Scripts[i].Instructions;
                    src[0] = dest[0];
                    src[src.Count - 1] = dest[dest.Count - 1];
                    destField.Entities[destEntity].Scripts[i].Instructions = src;
                }
                else
                {
                    destField.Entities[destEntity].Scripts[i].Instructions = new List<FieldScriptInstruction>()
                    {
                        destField.Entities[destEntity].Scripts[i].Instructions[0],
                        destField.Entities[destEntity].Scripts[i].Instructions.Last()
                    };
                }
            }

            destField.Entities[destEntity].Type = srcField.Entities[srcEntity].Type;
            destField.SaveToSource(fieldSource, destFieldName);
        }

        // remove an entity from the field by deleting its initialisation script
        public static void DeleteEntity(FileSource fieldSource, string fieldName, int entity)
        {
            DeleteScript(fieldSource, fieldName, entity, 0);
        }

        // change the text of a field message
        public static void SetText(FileSource fieldSource, string fieldName, int messageId, string newText)
        {
            var fieldPath = FieldScript.GetFieldPath(fieldName);
            var msdPath = Path.Combine(fieldPath, fieldName + Env.MessageFileExtension);
            var innerSource = new FileSource(fieldPath, fieldSource);
            var fieldText = MessageFile.FromSource(innerSource, msdPath);
            fieldText.Messages[messageId] = newText;
            innerSource.ReplaceFile(msdPath, fieldText.Encode());
        }

        public static void CopyParticle(FileSource fieldSource, string srcField, string destField)
        {
            var srcPath = FieldScript.GetFieldPath(srcField);
            var destPath = FieldScript.GetFieldPath(destField);
            var srcSource = new FileSource(srcPath, fieldSource);
            var destSource = new FileSource(destPath, fieldSource);
            destSource.ReplaceFile(Path.Combine(destPath, destField + ".pmd"), srcSource.GetFile(Path.Combine(srcPath, srcField + ".pmd")));
            destSource.ReplaceFile(Path.Combine(destPath, destField + ".pmp"), srcSource.GetFile(Path.Combine(srcPath, srcField + ".pmp")));
            fieldSource.Encode();
        }
    }
}
