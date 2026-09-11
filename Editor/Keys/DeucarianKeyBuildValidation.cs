using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deucarian.Editor
{
    /// <summary>Blocks player builds with stale generated definitions or unresolved serialized key selections.</summary>
    public sealed class DeucarianKeyBuildValidation : IPreprocessBuildWithReport, IProcessSceneWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (var source in DeucarianKeyGeneration.Sources())
                try { DeucarianKeyGeneration.Validate(source); }
                catch (System.Exception error) { throw new BuildFailedException(error.GetBaseException().Message); }

            var drawers = DeucarianKeyValidation.Drawers();
            var errors = new List<string>();
            var included = new HashSet<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled) IncludeDependencies(scene.path, included);
            foreach (var asset in PlayerSettings.GetPreloadedAssets())
                if (asset != null) IncludeDependencies(AssetDatabase.GetAssetPath(asset), included);
            foreach (string path in AssetDatabase.GetAllAssetPaths())
                if (path.Contains("/Resources/")) IncludeDependencies(path, included);
            ValidateAssets(included, drawers, errors);
            Fail(errors);
        }

        private static void ValidateAssets(HashSet<string> included, IReadOnlyList<DeucarianKeyDrawer> drawers, List<string> errors)
        {
            foreach (string path in included)
            {
                if (path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is GameObject prefab) ValidateHierarchy(prefab, drawers, errors);
                    else if (asset is ScriptableObject) errors.AddRange(DeucarianKeyValidation.Validate(asset, drawers));
                }
            }
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null) return;
            var drawers = DeucarianKeyValidation.Drawers();
            var errors = new List<string>();
            foreach (var root in scene.GetRootGameObjects()) ValidateHierarchy(root, drawers, errors);
            var included = new HashSet<string>();
            IncludeDependencies(scene.path, included);
            ValidateAssets(included, drawers, errors);
            Fail(errors);
        }

        private static void IncludeDependencies(string path, HashSet<string> included)
        {
            if (string.IsNullOrEmpty(path)) return;
            foreach (string dependency in AssetDatabase.GetDependencies(path, true)) included.Add(dependency);
        }

        private static void ValidateHierarchy(GameObject root, IReadOnlyList<DeucarianKeyDrawer> drawers, List<string> errors)
        {
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null) errors.AddRange(DeucarianKeyValidation.Validate(component, drawers));
        }

        private static void Fail(List<string> errors)
        {
            if (errors.Count != 0) throw new BuildFailedException("Fix these typed key selections before building:\n" + string.Join("\n", errors));
        }
    }
}
