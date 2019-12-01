using System;
using System.IO;
using System.Reflection;
using FF8Mod.Field;
using FF8Mod.Archive;

namespace FF8Mod.Maelstrom
{
    public static class StorySkip
    {
        public static BinaryPatch IntroPatch = new BinaryPatch(0x273fb, new byte[] { 0x33, 0x30 }, new byte[] { 0x30, 0x31 });

        public static void Apply(FileSource fieldSource, string af3dnPath, int seed)
        {
            // replace liberi fatali intro with quistis walking through a door
            ImportScript(fieldSource, "start0", 0, 1);
            IntroPatch.Apply(af3dnPath);

            // brief conversation in the infirmary, receive 2 GFs and a party member
            ImportScript(fieldSource, "bghoke_2", 12, 1);
            ImportScript(fieldSource, "bghoke_2", 6, 4);
            SetText(fieldSource, "bghoke_2", 17, "Quistis{02}“Another random SeeD?{02} ID #" + seed.ToString()  + "...”");

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

            // timber gate guards
            DeleteEntity(fieldSource, "tigate1", 0);            // lines
            DeleteEntity(fieldSource, "tigate1", 1);
            DeleteEntity(fieldSource, "tigate1", 2);
            DeleteScript(fieldSource, "tigate1", 19, 1);        // exit lock
            DeleteEntity(fieldSource, "tigate1", 17);           // g-soldiers
            DeleteEntity(fieldSource, "tigate1", 18);
        }

        public static void Remove(string af3dnPath)
        {
            // todo: reset field scripts
            IntroPatch.Remove(af3dnPath);
        }

        // overwrite a field script with one imported from an embedded text file
        public static void ImportScript(FileSource fieldSource, string fieldName, int entity, int script, string importPath)
        {
            var field = FieldScript.FromSource(fieldSource, fieldName);
            field.ReplaceScript(entity, script, App.ReadEmbeddedFile(importPath));
            SaveToSource(fieldSource, fieldName, field.Encode());
        }

        // slightly easier import with the filename convention "fieldName.entityID.scriptID.txt"
        public static void ImportScript(FileSource fieldSource, string fieldName, int entity, int script)
        {
            ImportScript(fieldSource, fieldName, entity, script, string.Format(@"FF8Mod.Maelstrom.FieldScripts.{0}.{1}.{2}.txt", fieldName, entity, script));
        }

        // delete a single script
        public static void DeleteScript(FileSource fieldSource, string fieldName, int entity, int script)
        {
            var field = FieldScript.FromSource(fieldSource, fieldName);
            field.ReplaceScript(entity, script, "");
            SaveToSource(fieldSource, fieldName, field.Encode());
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
            var msdPath = Path.Combine(fieldPath, fieldName + ".msd");
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

        private static void SaveToSource(FileSource fieldSource, string fieldName, byte[] fieldCode)
        {
            var innerSource = new FileSource(FieldScript.GetFieldPath(fieldName), fieldSource);
            innerSource.ReplaceFile(FieldScript.GetFieldPath(fieldName) + "\\" + fieldName + ".jsm", fieldCode);
        }
    }
}
