using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.Editor
{
    public static class DeucarianEditorObjectFieldRow
    {
        public static T Draw<T>(string label, T value, bool allowSceneObjects = false, string hint = null)
            where T : Object
        {
            T result = value;
            DeucarianEditorFieldRow.Draw(label, () =>
            {
                result = (T)EditorGUILayout.ObjectField(result, typeof(T), allowSceneObjects);
                DeucarianEditorMiniToolbar.PingButton(result);
                DeucarianEditorMiniToolbar.SelectButton(result);
            }, hint);
            return result;
        }
    }
}
