using System;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Exact editor-only references in editable C# definition documents.</summary>
    public static class DeucarianDefinitionAssets
    {
        public static T Load<T>(string guid, long localId) where T : UnityEngine.Object => (T)Load(typeof(T), guid, localId);
        internal static UnityEngine.Object Load(Type type, string guid, long localId)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (var value in AssetDatabase.LoadAllAssetsAtPath(path))
                if (type.IsInstanceOfType(value) && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out string _, out long id) && id == localId) return value;
            throw new InvalidOperationException("Definition references a missing " + type.Name + " asset (" + guid + "). Assign an existing asset in the definition editor.");
        }
    }
}
