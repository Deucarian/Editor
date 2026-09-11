using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Queues work outside asset import; source changes converge without repeated writes.</summary>
    [InitializeOnLoad]
    internal sealed class DeucarianDefinitionImport : AssetPostprocessor
    {
        private static readonly HashSet<string> pending = new HashSet<string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> errors = new Dictionary<string, string>(StringComparer.Ordinal);
        private static bool scheduled;
        internal static IReadOnlyDictionary<string, string> Errors => errors;
        internal static void ClearError(string path) => errors.Remove(path);
        internal static bool BelongsTo(string path, string schema)
        {
            var record = DeucarianDefinitionSync.Records.FirstOrDefault(x => AssetDatabase.GUIDToAssetPath(x.sourceGuid) == path || AssetDatabase.GUIDToAssetPath(x.assetGuid) == path);
            if (record != null) return record.schema == schema;
            if (!path.EndsWith(".definition.cs", StringComparison.Ordinal) || !File.Exists(path)) return false;
            try { return DeucarianDefinitionSource.SchemaId(File.ReadAllText(path)) == schema; }
            catch { return true; }
        }
        static DeucarianDefinitionImport()
        {
            EditorApplication.delayCall += Scan;
            Undo.postprocessModifications += Modified;
        }
        private static bool Disabled => SessionState.GetBool(DeucarianKeyGeneration.AutomaticRefreshDisabledSessionKey, false) || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer;
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] previous)
        {
            foreach (var path in deleted.Concat(previous)) errors.Remove(path);
            if (Disabled) return;
            foreach (var path in imported.Concat(moved)) Enqueue(path);
        }
        private static UndoPropertyModification[] Modified(UndoPropertyModification[] changes)
        {
            if (!Disabled) foreach (var change in changes) Enqueue(AssetDatabase.GetAssetPath(change.currentValue.target));
            return changes;
        }
        private static void Scan()
        {
            if (Disabled || !Directory.Exists("Assets")) return;
            foreach (var path in Directory.GetFiles("Assets", "*.definition.cs", SearchOption.AllDirectories)) Enqueue(path.Replace('\\', '/'));
        }
        internal static void Enqueue(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal)) return;
            pending.Add(path);
            if (scheduled) return;
            scheduled = true; EditorApplication.delayCall += Drain;
        }
        private static void Drain()
        {
            scheduled = false;
            if (Disabled) { pending.Clear(); return; }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { scheduled = true; EditorApplication.delayCall += Drain; return; }
            var paths = pending.ToArray(); pending.Clear();
            var schemas = DeucarianDefinitionSchema.Discover();
            foreach (string path in paths)
                try
                {
                    if (path.EndsWith(".definition.cs", StringComparison.Ordinal) && File.Exists(path))
                    {
                        string id = DeucarianDefinitionSource.SchemaId(File.ReadAllText(path));
                        var schema = schemas.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("Install the definition package for schema " + id);
                        DeucarianDefinitionSync.SynchronizeSource(schema, path);
                    }
                    else if (AssetDatabase.LoadMainAssetAtPath(path) is ScriptableObject asset)
                    {
                        var record = DeucarianDefinitionSync.FindAsset(asset);
                        if (record != null)
                        {
                            var schema = schemas.First(x => x.Id == record.schema);
                            DeucarianDefinitionSync.SynchronizeSource(schema, AssetDatabase.GUIDToAssetPath(record.sourceGuid));
                        }
                        foreach (var parent in DeucarianDefinitionSync.Records)
                        {
                            string parentPath = AssetDatabase.GUIDToAssetPath(parent.assetGuid);
                            if (parentPath == path || !AssetDatabase.GetDependencies(parentPath, true).Contains(path)) continue;
                            DeucarianDefinitionSync.SynchronizeSource(schemas.First(x => x.Id == parent.schema), AssetDatabase.GUIDToAssetPath(parent.sourceGuid));
                        }
                    }
                    errors.Remove(path);
                }
                catch (Exception error) { errors[path] = error.Message; }
        }
    }

    internal sealed class DeucarianDefinitionBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -950;
        public void OnPreprocessBuild(BuildReport report)
        {
            var problems = DeucarianDefinitionSync.ValidateAll().Concat(DeucarianDefinitionImport.Errors.Values).Distinct().ToArray();
            if (problems.Length > 0) throw new BuildFailedException("Resolve definition authoring errors before building:\n" + string.Join("\n", problems));
        }
    }
}
