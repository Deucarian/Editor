using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Writes an owned asset-reference catalog only when its contents change.</summary>
    public static class DeucarianDefinitionCatalog
    {
        public static T Update<T>(string path, string entriesField, IEnumerable<UnityEngine.Object> entries, bool validateOnly = false) where T : ScriptableObject
        {
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Split('/').Contains(".."))
                throw new ArgumentException("Generated catalogs must be under Assets.", nameof(path));
            var values = entries.ToArray();
            var catalog = AssetDatabase.LoadAssetAtPath<T>(path);
            if (validateOnly)
            {
                if (catalog == null && values.Length == 0) return null;
                if (catalog == null) throw new InvalidOperationException("Restore the generated catalog by synchronizing its definitions: " + path);
                using (var snapshot = new SerializedObject(catalog))
                {
                    var field = snapshot.FindProperty(entriesField);
                    if (field == null || field.arraySize != values.Length || Enumerable.Range(0, values.Length).Any(i => field.GetArrayElementAtIndex(i).objectReferenceValue != values[i]))
                        throw new InvalidOperationException("Generated catalog entries are stale. Synchronize the definitions before building: " + path);
                }
                return catalog;
            }
            if (catalog == null)
            {
                if (File.Exists(path)) throw new InvalidOperationException("The generated catalog path already contains a different asset: " + path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                catalog = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(catalog, path);
            }
            using (var serialized = new SerializedObject(catalog))
            {
                var field = serialized.FindProperty(entriesField) ?? throw new InvalidOperationException("Catalog field is missing: " + entriesField);
                field.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++) field.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                if (serialized.ApplyModifiedPropertiesWithoutUndo()) AssetDatabase.SaveAssets();
            }
            return catalog;
        }
    }
}
