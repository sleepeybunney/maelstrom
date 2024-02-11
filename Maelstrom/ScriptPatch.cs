using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Field;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sleepey.Maelstrom
{
    internal class ScriptPatch
    {
        public string Field { get; set; }
        public List<ScriptChangeOperation> UpdateScripts { get; set; } = new List<ScriptChangeOperation>();
        public List<EntityCopyOperation> CopyEntities { get; set; } = new List<EntityCopyOperation>();
        public List<ScriptCopyOperation> CopyScripts { get; set; } = new List<ScriptCopyOperation>();
        public List<ScriptDeleteOperation> DeleteScripts { get; set; } = new List<ScriptDeleteOperation>();
        public List<int> DeleteEntities { get; set; } = new List<int>();
        public string CopyParticlesFrom { get; set; } = null;

        public FieldScript ScriptFile;

        public ScriptPatch() { }

        public void Apply(FileSource fieldSource)
        {
            ScriptFile = FieldScript.FromSource(fieldSource, Field);
            if (CopyParticlesFrom != null) CopyParticles(fieldSource);
            foreach (var delete in DeleteScripts) DeleteScript(delete.Entity, delete.Script);
            foreach (var entity in DeleteEntities) DeleteEntity(entity);
            foreach (var copy in CopyEntities) CopyEntity(fieldSource, copy);
            foreach (var copy in CopyScripts) CopyScript(fieldSource, copy);
            foreach (var change in UpdateScripts) UpdateScript(change);
            ScriptFile.SaveToSource(fieldSource, Field);
        }

        // add new instructions to a script, using the surrounding instructions to determine where to write
        //  - before/after = insert before or after the given line(s)
        //  - give multiple lines for more specific placement, separate with "\n"
        //  - if more than one possible insertion point is found, the first will be used
        //  - before "" = insert at end of script (but before return)
        //  - after "" = insert at start of script (but after label)
        //  - before & after together = overwrite everything in between
        public void UpdateScript(ScriptChangeOperation change)
        {
            if (change.After == null && change.Before == null)
            {
                throw new ArgumentException($"No insertion point given ({Field}, entity {change.Entity}, script {change.Script})");
            }

            var script = ScriptFile.Entities[change.Entity].Scripts[change.Script];
            var startIndex = -1;
            var endIndex = -1;

            if (change.After != null)
            {
                if (change.After == "") startIndex = 1;
                else
                {
                    var searchInstructions = new Script(change.After).Instructions;
                    var startIndexCandidates = FindAllIndices(script.Instructions, searchInstructions);

                    if (startIndexCandidates.Count < 1)
                    {
                        throw new ArgumentException($"Insertion point not found: {change.After} ({Field}, entity {change.Entity}, script {change.Script})");
                    }

                    if (startIndexCandidates.Count > 1)
                    {
                        Debug.WriteLine($"Ambiguous insertion point, going with first match: {change.After} ({Field}, entity {change.Entity}, script {change.Script})");
                    }
                    
                    startIndex = startIndexCandidates[0] + searchInstructions.Count;
                }
            }

            if (change.Before != null)
            {
                if (change.Before == "") endIndex = script.Instructions.Count - 1;
                else
                {
                    var searchInstructions = new Script(change.Before).Instructions;
                    var endIndexCandidates = FindAllIndices(script.Instructions, searchInstructions);

                    // if both start & end points are provided, end must come after start
                    if (startIndex > -1) endIndexCandidates.RemoveAll(i => i <= startIndex);

                    if (endIndexCandidates.Count < 1)
                    {
                        throw new ArgumentException($"Insertion point not found: {change.Before} ({Field}, entity {change.Entity}, script {change.Script})");
                    }

                    if (endIndexCandidates.Count > 1)
                    {
                        Debug.WriteLine($"Ambiguous insertion point, going with first match: {change.Before} ({Field}, entity {change.Entity}, script {change.Script})");
                    }
                    
                    endIndex = endIndexCandidates[0];
                }
            }

            if (startIndex > -1 && endIndex > -1)
            {
                script.Instructions.RemoveRange(startIndex, endIndex - startIndex);
            }
            
            var newInstructions = new Script(string.Join("\n", change.Instructions)).Instructions;
            script.Instructions.InsertRange(startIndex > -1 ? startIndex : endIndex, newInstructions);
        }

        private List<int> FindAllIndices(List<FieldScriptInstruction> scriptInstructions, List<FieldScriptInstruction> searchInstructions)
        {
            if (searchInstructions.Count < 1) throw new ArgumentException("Invalid insertion point - no instructions");
            if (searchInstructions.Count > scriptInstructions.Count) throw new ArgumentException("Invalid insertion point - too many instructions");

            var indices = new List<int>();
            for (int i = 0; i <= scriptInstructions.Count - searchInstructions.Count; i++)
            {
                var match = true;
                for (int j = 0; j < searchInstructions.Count; j++)
                {
                    if (scriptInstructions[i + j] != searchInstructions[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) indices.Add(i);
            }

            return indices;
        }

        public void DeleteScript(int entity, int script)
        {
            if (ScriptFile.Entities.ElementAtOrDefault(entity) == null)
            {
                throw new ArgumentException($"Entity not found ({Field}, entity {entity})");
            }

            if (ScriptFile.Entities[entity].Scripts.ElementAtOrDefault(script) == null)
            {
                throw new ArgumentException($"Script not found ({Field}, entity {entity}, script {script})");
            }

            // overwrite with empty script (label & return statements only)
            var label = ScriptFile.Entities[entity].Scripts[script].Label;
            ScriptFile.Entities[entity].Scripts[script] = new Script($"lbl {label}\nret 8");
        }

        // copy a script from one entity to another
        //  - overwrites an existing script to maintain script count & order
        public void CopyScript(FileSource fieldSource, ScriptCopyOperation copy)
        {
            // assume source field is the same as the target field if none is given
            FieldScript sourceField;
            if (copy.SourceField == null) sourceField = ScriptFile;
            else sourceField = FieldScript.FromSource(fieldSource, copy.SourceField);

            if (sourceField.Entities.ElementAtOrDefault(copy.SourceEntity) == null)
            {
                throw new ArgumentException($"Entity not found ({copy.SourceField ?? Field}, entity {copy.SourceEntity})");
            }

            if (sourceField.Entities[copy.SourceEntity].Scripts.ElementAtOrDefault(copy.SourceScript) == null)
            {
                throw new ArgumentException($"Script not found ({copy.SourceField ?? Field}, entity {copy.SourceEntity}, script {copy.SourceScript})");
            }

            if (ScriptFile.Entities.ElementAtOrDefault(copy.TargetEntity) == null)
            {
                throw new ArgumentException($"Entity not found ({Field}, entity {copy.TargetEntity})");
            }

            if (ScriptFile.Entities[copy.TargetEntity].Scripts.ElementAtOrDefault(copy.OverwriteScript) == null)
            {
                throw new ArgumentException($"Script not found ({Field}, entity {copy.TargetEntity}, script {copy.OverwriteScript})");
            }

            var sourceScript = sourceField.Entities[copy.SourceEntity].Scripts[copy.SourceScript];
            var targetScripts = ScriptFile.Entities[copy.TargetEntity].Scripts;

            var label = targetScripts[copy.OverwriteScript].Label;
            targetScripts[copy.OverwriteScript] = new Script(sourceScript.ToString(), sourceScript.MysteryFlag);
            targetScripts[copy.OverwriteScript].Label = label;
        }

        public void DeleteEntity(int entity)
        {
            // delete the entity's init script to effectively remove it from the field
            DeleteScript(entity, 0);
        }

        // copy an entity
        //  - overwrites an existing entity to maintain script count & order
        //  - extra scripts on the source end aren't copied over
        //  - extra scripts on the target end are emptied out
        public void CopyEntity(FileSource fieldSource, EntityCopyOperation copy)
        {
            // assume source field is the same as the target field if none is given
            FieldScript sourceField;
            if (copy.SourceField == null) sourceField = ScriptFile;
            else sourceField = FieldScript.FromSource(fieldSource, copy.SourceField);

            if (sourceField.Entities.ElementAtOrDefault(copy.SourceEntity) == null)
            {
                throw new ArgumentException($"Entity not found ({copy.SourceField ?? Field}, entity {copy.SourceEntity})");
            }

            if (ScriptFile.Entities.ElementAtOrDefault(copy.OverwriteEntity) == null)
            {
                throw new ArgumentException($"Entity not found ({Field}, entity {copy.OverwriteEntity})");
            }

            var sourceEntity = sourceField.Entities[copy.SourceEntity];
            var sourceScripts = sourceEntity.Scripts;
            var targetEntity = ScriptFile.Entities[copy.OverwriteEntity];
            var targetScripts = targetEntity.Scripts;

            targetEntity.Type = sourceEntity.Type;
            for (int i = 0; i < targetScripts.Count; i++)
            {
                if (i >= sourceScripts.Count)
                {
                    DeleteScript(copy.OverwriteEntity, i);
                }
                else
                {
                    var label = targetScripts[i].Label;
                    targetScripts[i] = new Script(sourceScripts[i].ToString(), sourceScripts[i].MysteryFlag);
                    targetScripts[i].Label = label;
                }
            }
        }

        // copy particle effects from one field to another
        //  - if the target field has its own particles they will be overwritten
        public void CopyParticles(FileSource fieldSource)
        {
            var srcPath = FieldScript.GetFieldPath(CopyParticlesFrom);
            var srcParticlePath = Path.Combine(srcPath, CopyParticlesFrom);
            var src = new InnerFileSource(srcPath, fieldSource);

            var destPath = FieldScript.GetFieldPath(Field);
            var destParticlePath = Path.Combine(destPath, Field);
            var dest = new InnerFileSource(destPath, fieldSource);

            dest.ReplaceFile(destParticlePath + ".pmd", src.GetFile(srcParticlePath + ".pmd"));
            dest.ReplaceFile(destParticlePath + ".pmp", src.GetFile(srcParticlePath + ".pmp"));
        }
    }

    internal class ScriptChangeOperation
    {
        public int Entity { get; set; }
        public int Script { get; set; }
        public string Before { get; set; } = null;
        public string After { get; set; } = null;
        public List<string> Instructions { get; set; } = new List<string>();
    }

    internal class EntityCopyOperation
    {
        public string SourceField { get; set; } = null;
        public int SourceEntity { get; set; }
        public int OverwriteEntity { get; set; }
    }

    internal class ScriptCopyOperation
    {
        public string SourceField { get; set; } = null;
        public int SourceEntity { get; set; }
        public int SourceScript { get; set; }
        public int TargetEntity { get; set; }
        public int OverwriteScript { get; set; }
    }

    internal class ScriptDeleteOperation
    {
        public int Entity { get; set; }
        public int Script { get; set; }
    }
}
