using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Domain-owned projection of existing assets into generated, asset-free caller keys.</summary>
    public abstract class DeucarianAssetKeySource
    {
        public abstract Type KeyType { get; }
        public abstract Type DefinitionSetAttribute { get; }
        public abstract string GeneratedClassName { get; }
        public abstract IReadOnlyList<DeucarianKeyChoice> ReadDefinitions();
        public string GeneratedAssemblyName => "Deucarian.GeneratedKeys." + KeyType.Name;
        public string OutputDirectory => "Assets/DeucarianGeneratedKeys/" + KeyType.Name;
        public string OutputPath => OutputDirectory + "/" + GeneratedClassName + ".g.cs";
        public string AssemblyPath => OutputDirectory + "/" + GeneratedAssemblyName + ".asmdef";
        public string AssemblyDefinition => "{\n  \"name\": \"" + GeneratedAssemblyName +
            "\",\n  \"rootNamespace\": \"Deucarian.Generated\",\n  \"references\": [\"" +
            KeyType.Assembly.GetName().Name + "\"],\n  \"autoReferenced\": true\n}\n";
    }

    public abstract class DeucarianAssetKeySource<TAsset> : DeucarianAssetKeySource where TAsset : ScriptableObject
    {
        protected abstract DeucarianKeyChoice ReadDefinition(TAsset asset);

        public override IReadOnlyList<DeucarianKeyChoice> ReadDefinitions()
        {
            var choices = new List<DeucarianKeyChoice>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(TAsset).Name, new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!paths.Add(path)) continue;
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (!(asset is TAsset definition)) continue;
                    DeucarianKeyChoice choice = ReadDefinition(definition);
                    if (choice == null) continue;
                    if (!seen.Add(choice.Id)) throw new InvalidOperationException("Duplicate " + KeyType.Name + " definition '" + choice.Id + "' at " + path + ". Give each definition a unique stable ID, then let Unity regenerate its keys.");
                    choices.Add(choice);
                }
            }
            choices.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
            return choices;
        }
    }
}
