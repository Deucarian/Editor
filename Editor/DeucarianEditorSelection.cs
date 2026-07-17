using UnityEditor;
using Object = UnityEngine.Object;

namespace Deucarian.Editor
{
    /// <summary>
    /// Provides shared editor selection actions for Deucarian tooling.
    /// </summary>
    public static class DeucarianEditorSelection
    {
        /// <summary>
        /// Selects and pings the supplied Unity object. Null targets are ignored.
        /// </summary>
        /// <param name="target">Object to reveal in the Unity Editor.</param>
        public static void SelectAndPing(Object target)
        {
            if (target == null)
            {
                return;
            }

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }
    }
}
