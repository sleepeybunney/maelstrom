using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Field;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Exe;
using System.Text.Json;

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

            var scriptPatches = JsonSerializer.Deserialize<List<ScriptPatch>>(App.ReadEmbeddedFile("Sleepey.Maelstrom.Data.FreeRoam.json"));
            foreach (var patch in scriptPatches) patch.Apply(fieldSource);

            // show seed value during intro, slightly different wording if numeric
            // (note: numbers above int_max count as numeric here, despite actually being treated as strings for rng purposes)
            var seedText = "ID #{0}...";
            if (!long.TryParse(seedString, out _)) seedText = "Codename: '{0}`...";
            var seedFinal = string.Format(seedText, seedString.Substring(0, Math.Min(codenameLength, seedString.Length)));
            SetText(fieldSource, "bghoke_2", 17, "Quistis{02}“Another random SeeD?{02} " + seedFinal);
        }

        public static void Remove()
        {
            if (!Env.Remastered) IntroPatch.Remove(Env.Af3dnPath);
        }

        // change the text of a field message
        public static void SetText(FileSource fieldSource, string fieldName, int messageId, string newText)
        {
            var fieldPath = FieldScript.GetFieldPath(fieldName);
            var msdPath = Path.Combine(fieldPath, fieldName + Env.MessageFileExtension);
            var innerSource = new InnerFileSource(fieldPath, fieldSource);
            var fieldText = MessageFile.FromSource(innerSource, msdPath);
            fieldText.Messages[messageId] = newText;
            innerSource.ReplaceFile(msdPath, fieldText.Encode());
        }
    }
}
