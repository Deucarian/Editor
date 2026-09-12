using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Deucarian.Editor
{
    /// <summary>A lazily queried, explicitly invalidated project and package asset index.</summary>
    public sealed class DeucarianEditorAssetCatalog
    {
        private readonly Type type;
        private List<Object> assets;

        public DeucarianEditorAssetCatalog(Type type)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            if (!typeof(Object).IsAssignableFrom(type)) throw new ArgumentException("A Unity object type is required.", nameof(type));
        }

        public void Invalidate() => assets = null;

        public IReadOnlyList<Object> Find()
        {
            if (assets != null) return assets;
            assets = new List<Object>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + type.Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (var candidate in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (candidate != null && type.IsInstanceOfType(candidate) && !assets.Contains(candidate)) assets.Add(candidate);
            }
            assets.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(
                AssetDatabase.GetAssetPath(a) + "/" + a.name, AssetDatabase.GetAssetPath(b) + "/" + b.name));
            return assets;
        }

        public static bool IsPackageAsset(Object asset) => asset != null &&
            AssetDatabase.GetAssetPath(asset).StartsWith("Packages/", StringComparison.Ordinal);

        public static string Origin(Object asset)
        {
            if (asset == null) return "No asset selected";
            string path = AssetDatabase.GetAssetPath(asset);
            return path.StartsWith("Packages/", StringComparison.Ordinal) ? "Package asset"
                : path.StartsWith("Assets/", StringComparison.Ordinal) ? "Project asset" : "Built-in / scene object";
        }

        public static bool IsUnusedProjectPath(string path, string extension = ".asset") =>
            !string.IsNullOrEmpty(path) && path.StartsWith("Assets/", StringComparison.Ordinal) &&
            !path.Contains("..") && !path.Contains("\\") && path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) &&
            System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)) &&
            !System.IO.File.Exists(path) && !System.IO.Directory.Exists(path) &&
            string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets));

        public static Object CopyToProject(Object source)
        {
            if (source == null) return null;
            string sourcePath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(sourcePath) || AssetDatabase.IsSubAsset(source)) return null;
            string path = EditorUtility.SaveFilePanelInProject("Customize " + source.name, source.name + " Custom",
                System.IO.Path.GetExtension(sourcePath).TrimStart('.'), "Choose where to save your project-owned copy.");
            if (string.IsNullOrEmpty(path)) return null;
            if (!IsUnusedProjectPath(path, System.IO.Path.GetExtension(sourcePath)) || !AssetDatabase.CopyAsset(sourcePath, path))
            {
                EditorUtility.DisplayDialog("Could not customize asset", "Choose a new project path and try again.", "OK");
                return null;
            }
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath(path, source.GetType());
        }
    }
}
