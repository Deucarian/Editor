using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Explicit synchronization commands; persistent state records the last agreed definition.</summary>
    public static class DeucarianDefinitionSync
    {
        public static IReadOnlyList<DeucarianDefinitionRecord> Records => DeucarianDefinitionIndex.Read().records.AsReadOnly();

        public static ScriptableObject Create(DeucarianDefinitionSchema schema, string name)
        {
            var names = AssetDatabase.FindAssets("t:" + schema.AssetType.Name, new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(x), schema.AssetType) as ScriptableObject)
                .Where(x => x != null).Select(x => DeucarianKeySourceText.Identifier(schema.Read(x).Name)).ToHashSet(StringComparer.Ordinal);
            string original = name;
            for (int suffix = 2; names.Contains(DeucarianKeySourceText.Identifier(name)); suffix++) name = original + suffix;
            var spec = schema.Create(name);
            schema.Validate(spec);
            string directory = "Assets/DeucarianDefinitions/" + schema.Id;
            Directory.CreateDirectory(directory + "/Editor");
            string sourcePath = AssetDatabase.GenerateUniqueAssetPath(directory + "/Editor/" + DeucarianDefinitionSource.Identifier(name) + ".definition.cs");
            EnsureAuthoringAssembly(schema, directory + "/Editor");
            File.WriteAllText(sourcePath, DeucarianDefinitionSource.Write(schema, spec));
            AssetDatabase.ImportAsset(sourcePath);
            return SynchronizeSource(schema, sourcePath);
        }

        public static ScriptableObject Adopt(DeucarianDefinitionSchema schema, ScriptableObject asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            RequireProjectPath(assetPath);
            var existing = FindAsset(asset);
            if (existing != null) return asset;
            var spec = schema.Read(asset);
            schema.Validate(spec);
            ValidateName(schema, spec);
            if (Records.Any(x => x.schema == schema.Id && x.identity == spec.Id)) throw new InvalidOperationException("Another managed definition uses '" + spec.Id + "'. Duplicate through Definitions to assign a new identity.");
            string directory = "Assets/DeucarianDefinitions/" + schema.Id + "/Editor";
            Directory.CreateDirectory(directory);
            EnsureAuthoringAssembly(schema, directory);
            string sourcePath = AssetDatabase.GenerateUniqueAssetPath(directory + "/" + DeucarianDefinitionSource.Identifier(spec.Name) + ".definition.cs");
            string source = DeucarianDefinitionSource.Write(schema, spec);
            File.WriteAllText(sourcePath, source);
            AssetDatabase.ImportAsset(sourcePath);
            var index = DeucarianDefinitionIndex.Read();
            index.records.Add(new DeucarianDefinitionRecord
            {
                schema = schema.Id, sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath),
                assetGuid = AssetDatabase.AssetPathToGUID(assetPath), identity = spec.Id, lastHash = Hash(schema, spec)
            });
            index.Save();
            schema.RefreshCatalog();
            foreach (var provider in DeucarianKeyGeneration.Sources()) DeucarianKeyGeneration.Refresh(provider);
            return asset;
        }

        public static DeucarianDefinitionRecord FindAsset(ScriptableObject asset)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            return DeucarianDefinitionIndex.Read().records.Find(x => x.assetGuid == guid);
        }

        public static ScriptableObject SynchronizeSource(DeucarianDefinitionSchema schema, string sourcePath)
        {
            RequireProjectPath(sourcePath);
            if (!sourcePath.Contains("/Editor/")) throw new InvalidOperationException("Authoring declarations use editor-only types. Move " + sourcePath + " into an Editor folder, then reimport it.");
            var spec = DeucarianDefinitionSource.Read(schema, File.ReadAllText(sourcePath));
            string sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
            if (string.IsNullOrEmpty(sourceGuid)) throw new InvalidOperationException("Import the definition source before synchronizing: " + sourcePath);
            var index = DeucarianDefinitionIndex.Read();
            var record = index.records.Find(x => x.sourceGuid == sourceGuid);
            if (record != null && (record.schema != schema.Id || record.identity != spec.Id)) throw new InvalidOperationException("A synchronized definition's schema and ID are stable. Create a new definition to change its identity: " + sourcePath);
            ValidateName(schema, spec);
            if (index.records.Any(x => x.sourceGuid != sourceGuid && x.schema == schema.Id && x.identity == spec.Id))
                throw new InvalidOperationException("Duplicate " + schema.DisplayName + " identity '" + spec.Id + "'. Duplicate through the definition editor to receive a new identity.");
            ScriptableObject asset;
            if (record == null)
            {
                var matches = AssetDatabase.FindAssets("t:" + schema.AssetType.Name, new[] { "Assets" })
                    .Select(x => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(x), schema.AssetType) as ScriptableObject)
                    .Where(x => x != null && schema.Read(x).Id == spec.Id).ToArray();
                if (matches.Length > 1) throw new InvalidOperationException("Multiple assets use definition ID '" + spec.Id + "'. Give each definition its own identity before synchronizing.");
                asset = matches.Length == 1 ? matches[0] : CreateAsset(schema, spec, sourcePath);
                record = new DeucarianDefinitionRecord { schema = schema.Id, sourceGuid = sourceGuid, assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)), identity = spec.Id };
                index.records.Add(record);
                if (Hash(schema, schema.Read(asset)) != Hash(schema, spec))
                {
                    index.Save();
                    throw new InvalidOperationException("The imported code and asset disagree for '" + spec.Name + "'. Use Keep code or Keep asset in the definition editor.");
                }
            }
            else
            {
                if (record.schema != schema.Id || record.identity != spec.Id) throw new InvalidOperationException("A synchronized definition's schema and ID are stable. Create a new definition to change its identity: " + sourcePath);
                asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(record.assetGuid), schema.AssetType) as ScriptableObject;
                if (asset == null) throw new InvalidOperationException("The asset for " + sourcePath + " was removed. Restore it, or remove the definition through its editor. Synchronization will not silently recreate deleted assets.");
                var assetSpec = schema.Read(asset);
                string codeHash = Hash(schema, spec);
                string assetHash = Hash(schema, assetSpec);
                bool codeChanged = codeHash != record.lastHash;
                bool assetChanged = assetHash != record.lastHash;
                if (codeChanged && assetChanged && codeHash != assetHash)
                    throw new InvalidOperationException("Both code and asset changed for '" + spec.Name + "'. Use Keep code or Keep asset in the definition editor to resolve the conflict.");
                if (assetChanged && !codeChanged)
                {
                    schema.Validate(assetSpec);
                    if (assetSpec.Id != record.identity) throw new InvalidOperationException("A definition's stable ID cannot be edited. Create a new definition instead.");
                    ValidateName(schema, assetSpec);
                    spec = assetSpec;
                    WriteSource(sourcePath, schema, spec);
                }
                else if (codeChanged) schema.Apply(asset, spec);
            }
            WriteSource(sourcePath, schema, spec);
            record.lastHash = Hash(schema, spec);
            index.Save();
            AssetDatabase.SaveAssets();
            schema.RefreshCatalog();
            foreach (var source in DeucarianKeyGeneration.Sources()) DeucarianKeyGeneration.Refresh(source);
            return asset;
        }

        public static void Resolve(DeucarianDefinitionSchema schema, DeucarianDefinitionRecord record, bool keepCode)
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(record.sourceGuid);
            var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(record.assetGuid), schema.AssetType) as ScriptableObject;
            if (asset == null || !File.Exists(sourcePath)) throw new InvalidOperationException("Restore the missing declaration or asset before resolving its conflict.");
            var spec = keepCode ? DeucarianDefinitionSource.Read(schema, File.ReadAllText(sourcePath)) : schema.Read(asset);
            schema.Validate(spec);
            if (spec.Id != record.identity) throw new InvalidOperationException("Restore the original stable ID before resolving this definition.");
            ValidateName(schema, spec);
            Undo.RecordObject(asset, "Resolve definition changes");
            if (keepCode) schema.Apply(asset, spec);
            WriteSource(sourcePath, schema, spec);
            var index = DeucarianDefinitionIndex.Read();
            index.records.Find(x => x.sourceGuid == record.sourceGuid).lastHash = Hash(schema, spec);
            index.Save(); AssetDatabase.SaveAssets(); schema.RefreshCatalog();
            DeucarianDefinitionImport.ClearError(sourcePath);
            DeucarianDefinitionImport.ClearError(AssetDatabase.GetAssetPath(asset));
            foreach (var provider in DeucarianKeyGeneration.Sources()) DeucarianKeyGeneration.Refresh(provider);
        }

        public static void Delete(DeucarianDefinitionSchema schema, DeucarianDefinitionRecord record)
        {
            string source = AssetDatabase.GUIDToAssetPath(record.sourceGuid);
            string asset = AssetDatabase.GUIDToAssetPath(record.assetGuid);
            if (!string.IsNullOrEmpty(source)) RequireProjectPath(source);
            if (!string.IsNullOrEmpty(asset)) RequireProjectPath(asset);
            if (!string.IsNullOrEmpty(source) && File.Exists(source) && !AssetDatabase.MoveAssetToTrash(source)) throw new IOException("Could not remove " + source);
            if (!string.IsNullOrEmpty(asset) && File.Exists(asset) && !AssetDatabase.MoveAssetToTrash(asset)) throw new IOException("Could not remove " + asset + ". Restore the declaration from Trash before retrying, or remove the remaining asset in Unity.");
            var index = DeucarianDefinitionIndex.Read(); index.records.RemoveAll(x => x.sourceGuid == record.sourceGuid); index.Save();
            schema.RefreshCatalog();
            foreach (var provider in DeucarianKeyGeneration.Sources()) DeucarianKeyGeneration.Refresh(provider);
        }

        public static IReadOnlyList<string> ValidateAll()
        {
            var errors = new List<string>();
            var schemas = DeucarianDefinitionSchema.Discover().ToDictionary(x => x.Id);
            var knownSources = new HashSet<string>(Records.Select(x => x.sourceGuid), StringComparer.Ordinal);
            if (Directory.Exists("Assets"))
                foreach (var file in Directory.GetFiles("Assets", "*.definition.cs", SearchOption.AllDirectories))
                {
                    string sourcePath = file.Replace('\\', '/');
                    if (!knownSources.Contains(AssetDatabase.AssetPathToGUID(sourcePath))) errors.Add("Synchronize the new declaration before building: " + sourcePath);
                }
            foreach (var record in Records)
                try
                {
                    if (!schemas.TryGetValue(record.schema, out var schema)) throw new InvalidOperationException("Install the definition's owning package: " + record.schema);
                    string path = AssetDatabase.GUIDToAssetPath(record.sourceGuid);
                    if (!File.Exists(path)) throw new InvalidOperationException("Restore the missing source for definition '" + record.identity + "', or delete it in its definition editor.");
                    var spec = DeucarianDefinitionSource.Read(schema, File.ReadAllText(path));
                    schema.ValidateReady(spec);
                    var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(record.assetGuid), schema.AssetType) as ScriptableObject;
                    if (asset != null) schema.ValidateAssetReady(asset);
                    if (asset == null || spec.Id != record.identity || Hash(schema, spec) != record.lastHash || Hash(schema, schema.Read(asset)) != record.lastHash)
                        throw new InvalidOperationException("Definition '" + spec.Name + "' is stale or conflicted. Synchronize it in its definition editor before building.");
                }
                catch (Exception error) { errors.Add(error.Message); }
            foreach (var schema in schemas.Values)
                try { schema.RefreshCatalog(true); }
                catch (Exception error) { errors.Add(error.Message); }
            return errors;
        }

        private static ScriptableObject CreateAsset(DeucarianDefinitionSchema schema, DeucarianDefinitionSpec spec, string sourcePath)
        {
            string directory = Path.GetDirectoryName(sourcePath).Replace('\\', '/');
            directory = directory.Substring(0, directory.LastIndexOf("/Editor", StringComparison.Ordinal));
            string path = AssetDatabase.GenerateUniqueAssetPath(directory + "/" + DeucarianDefinitionSource.Identifier(spec.Name) + ".asset");
            var asset = ScriptableObject.CreateInstance(schema.AssetType);
            AssetDatabase.CreateAsset(asset, path);
            try { schema.Apply(asset, spec); }
            catch { AssetDatabase.DeleteAsset(path); throw; }
            return asset;
        }
        private static void WriteSource(string path, DeucarianDefinitionSchema schema, DeucarianDefinitionSpec spec)
        {
            string value = DeucarianDefinitionSource.Write(schema, spec);
            if (File.ReadAllText(path) == value) return;
            File.WriteAllText(path, value); AssetDatabase.ImportAsset(path);
        }
        private static string Hash(DeucarianDefinitionSchema schema, DeucarianDefinitionSpec spec)
        {
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(DeucarianDefinitionSource.Value(spec, schema.SpecType, 0)))).Replace("-", string.Empty);
        }
        private static void EnsureAuthoringAssembly(DeucarianDefinitionSchema schema, string directory)
        {
            string path = directory + "/Deucarian.Definitions." + schema.Id + ".asmdef";
            string references = string.Join(", ", DeucarianDefinitionAssemblies.References(schema.SpecType).Select(x => "\"" + x + "\""));
            string value = "{\n  \"name\": \"Deucarian.Definitions." + schema.Id + "\",\n  \"references\": [" + references + "],\n  \"includePlatforms\": [\"Editor\"]\n}\n";
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path);
                if (existing == value) return;
                string owned = "^\\s*\\{\\s*\"name\"\\s*:\\s*\"Deucarian\\.Definitions\\." + Regex.Escape(schema.Id) + "\"\\s*,\\s*\"references\"\\s*:\\s*\\[(?:\\s*\"[A-Za-z0-9_.-]+\"\\s*,?)*\\s*\\]\\s*,\\s*\"includePlatforms\"\\s*:\\s*\\[\\s*\"Editor\"\\s*\\]\\s*\\}\\s*$";
                if (!Regex.IsMatch(existing, owned)) throw new InvalidOperationException("Move the custom asmdef outside the generated authoring folder: " + path);
            }
            File.WriteAllText(path, value); AssetDatabase.ImportAsset(path);
        }
        private static void ValidateName(DeucarianDefinitionSchema schema, DeucarianDefinitionSpec spec)
        {
            string name = DeucarianKeySourceText.Identifier(spec.Name);
            foreach (string guid in AssetDatabase.FindAssets("t:" + schema.AssetType.Name, new[] { "Assets" }))
            {
                var other = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), schema.AssetType) as ScriptableObject;
                if (other == null) continue;
                var value = schema.Read(other);
                if (value.Id != spec.Id && DeucarianKeySourceText.Identifier(value.Name) == name)
                    throw new InvalidOperationException("Another " + schema.DisplayName + " definition generates '" + name + "'. Choose a distinct name before synchronizing.");
            }
        }

        private static void RequireProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal) || path.Split('/').Contains("..")) throw new ArgumentException("Definitions must be project-owned assets under Assets: " + path);
        }
    }
}
