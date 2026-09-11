using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Deucarian.Editor
{
    /// <summary>Shared import workflow; domain source providers own discovery, identity, and projection.</summary>
    public static class DeucarianKeyGeneration
    {
        public const string AutomaticRefreshDisabledSessionKey = "Deucarian.Keys.AutomaticRefreshDisabled";
        public static IReadOnlyList<DeucarianAssetKeySource> Sources()
        {
            var result = new List<DeucarianAssetKeySource>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<DeucarianAssetKeySource>())
                if (type.IsVisible && !type.IsAbstract && !type.ContainsGenericParameters && type.GetConstructor(Type.EmptyTypes) != null)
                    result.Add((DeucarianAssetKeySource)Activator.CreateInstance(type));
            result.Sort((a, b) => string.Compare(a.OutputPath, b.OutputPath, StringComparison.Ordinal));
            return result;
        }

        public static void Refresh(DeucarianAssetKeySource source)
        {
            string expected = DeucarianKeySourceText.Create(source);
            bool exists = File.Exists(source.OutputPath);
            if (!exists && source.ReadDefinitions().Count == 0) return;
            if (File.Exists(source.AssemblyPath) && File.ReadAllText(source.AssemblyPath) != source.AssemblyDefinition)
                throw new InvalidOperationException(source.AssemblyPath + " differs from its generated definition. Move the custom assembly file out of the generated folder, then reimport a source definition.");
            if (!File.Exists(source.AssemblyPath))
            {
                Directory.CreateDirectory(source.OutputDirectory);
                File.WriteAllText(source.AssemblyPath, source.AssemblyDefinition);
                AssetDatabase.ImportAsset(source.AssemblyPath);
            }
            if (exists)
            {
                string current = File.ReadAllText(source.OutputPath);
                if (current == expected) return;
                if (!current.StartsWith(DeucarianKeySourceText.Header, StringComparison.Ordinal))
                    throw new InvalidOperationException(source.OutputPath + " is not an owned generated file. Move or rename it before generating definition keys.");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(source.OutputPath));
            File.WriteAllText(source.OutputPath, expected);
            AssetDatabase.ImportAsset(source.OutputPath);
        }

        public static void Validate(DeucarianAssetKeySource source)
        {
            string expected = DeucarianKeySourceText.Create(source);
            if (!File.Exists(source.OutputPath) && source.ReadDefinitions().Count == 0) return;
            if (!File.Exists(source.OutputPath) || File.ReadAllText(source.OutputPath) != expected ||
                !File.Exists(source.AssemblyPath) || File.ReadAllText(source.AssemblyPath) != source.AssemblyDefinition)
                throw new InvalidOperationException(source.OutputPath + " is missing or stale. Reimport a source definition and let Unity finish generating and compiling its typed keys before building.");
        }

        public static string FindError(Type keyType)
        {
            foreach (var source in Sources())
                if (source.KeyType == keyType)
                    try { Validate(source); } catch (Exception error) { return error.GetBaseException().Message; }
            return null;
        }
    }

    internal sealed class DeucarianKeyImportRefresh : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer ||
                SessionState.GetBool(DeucarianKeyGeneration.AutomaticRefreshDisabledSessionKey, false)) return;
            foreach (var source in DeucarianKeyGeneration.Sources())
            {
                try { DeucarianKeyGeneration.Refresh(source); }
                catch (Exception) { /* The Inspector and build validator show source-specific repair guidance. */ }
            }
        }
    }
}
