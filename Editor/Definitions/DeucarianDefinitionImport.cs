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
        private static readonly DeucarianDefinitionWorkQueue work = new DeucarianDefinitionWorkQueue();
        private static readonly Dictionary<string, string> errors = new Dictionary<string, string>(StringComparer.Ordinal);
        internal static IReadOnlyDictionary<string, string> Errors => errors;
        internal static void ClearError(string path) => errors.Remove(path);
        internal static bool BelongsTo(string path, string schema)
        {
            var record = DeucarianDefinitionSync.Records.FirstOrDefault(x => AssetDatabase.GUIDToAssetPath(x.sourceGuid) == path || AssetDatabase.GUIDToAssetPath(x.assetGuid) == path);
            if (record != null) return record.schema == schema;
            if (!DeucarianDefinitionSource.IsSourcePath(path) || !File.Exists(path)) return false;
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
            foreach (var path in Directory.EnumerateFiles("Assets", "*.cs", SearchOption.AllDirectories).Where(DeucarianDefinitionSource.IsSourcePath)) Enqueue(path.Replace('\\', '/'));
        }
        internal static void Enqueue(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal) ||
                !(DeucarianDefinitionSource.IsSourcePath(path) || path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))) return;
            if (work.Enqueue(path, EditorApplication.timeSinceStartup)) EditorApplication.update += Drain;
        }

        internal static IDisposable BeginEditing(DeucarianDefinitionSchema schema, ScriptableObject asset) =>
            work.BeginEditing(AssetDatabase.GetAssetPath(asset), Enqueue, () => DeucarianDefinitionSync.CaptureHash(schema, asset));
        internal static void Acknowledge(ScriptableObject asset) => work.Acknowledge(AssetDatabase.GetAssetPath(asset));

        private static void Drain()
        {
            var paths = work.TakeReady(EditorApplication.timeSinceStartup,
                EditorApplication.isCompiling || EditorApplication.isUpdating || EditorGUIUtility.editingTextField);
            if (paths == null) return;
            EditorApplication.update -= Drain;
            if (Disabled) return;
            var schemas = DeucarianDefinitionSchema.Discover();
            var synchronized = new HashSet<string>(StringComparer.Ordinal);
            foreach (string path in paths)
                try
                {
                    if (DeucarianDefinitionSource.IsSourcePath(path) && File.Exists(path))
                    {
                        string id = DeucarianDefinitionSource.SchemaId(File.ReadAllText(path));
                        var schema = schemas.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("Install the definition package for schema " + id);
                        Synchronize(schema, path, synchronized);
                    }
                    else if (AssetDatabase.LoadMainAssetAtPath(path) is ScriptableObject asset)
                    {
                        var record = DeucarianDefinitionSync.FindAsset(asset);
                        if (record != null)
                        {
                            var schema = schemas.First(x => x.Id == record.schema);
                            Synchronize(schema, AssetDatabase.GUIDToAssetPath(record.sourceGuid), synchronized);
                        }
                        foreach (var parent in DeucarianDefinitionSync.Records)
                        {
                            string parentPath = AssetDatabase.GUIDToAssetPath(parent.assetGuid);
                            if (parentPath == path || !AssetDatabase.GetDependencies(parentPath, true).Contains(path)) continue;
                            Synchronize(schemas.First(x => x.Id == parent.schema), AssetDatabase.GUIDToAssetPath(parent.sourceGuid), synchronized);
                        }
                    }
                }
                catch (Exception error) { errors[path] = error.Message; }
        }

        private static void Synchronize(DeucarianDefinitionSchema schema, string path, HashSet<string> synchronized)
        {
            if (!synchronized.Add(path)) return;
            var record = DeucarianDefinitionSync.Records.FirstOrDefault(x => AssetDatabase.GUIDToAssetPath(x.sourceGuid) == path);
            if (record != null && work.ShouldDefer(AssetDatabase.GUIDToAssetPath(record.assetGuid))) return;
            try
            {
                DeucarianDefinitionSync.SynchronizeSource(schema, path);
                errors.Remove(path);
                if (record != null) errors.Remove(AssetDatabase.GUIDToAssetPath(record.assetGuid));
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
