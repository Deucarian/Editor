using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorActionGUI
    {
        public static bool Button(string text, params GUILayoutOption[] options) =>
            GUILayout.Button(text, DeucarianEditorButtons.SecondaryStyle, options);
        public static bool Button(GUIContent text, params GUILayoutOption[] options) =>
            GUILayout.Button(text, DeucarianEditorButtons.SecondaryStyle, options);
        public static bool Button(Texture image, params GUILayoutOption[] options) =>
            GUILayout.Button(image, DeucarianEditorButtons.SecondaryStyle, options);
        public static bool Button(string text, GUIStyle style, params GUILayoutOption[] options) => GUILayout.Button(text, style, options);
        public static bool Button(GUIContent text, GUIStyle style, params GUILayoutOption[] options) => GUILayout.Button(text, style, options);

        public static int Toolbar(int selected, string[] labels, params GUILayoutOption[] options)
            => Toolbar(EditorGUIUtility.currentViewWidth, selected, labels, options);

        public static int Toolbar(EditorWindow owner, int selected, string[] labels, params GUILayoutOption[] options)
            => Toolbar(owner.position.width, selected, labels, options);

        private static int Toolbar(float width, int selected, string[] labels, GUILayoutOption[] options)
        {
            if (labels == null || labels.Length == 0) return selected;
            // Wrapped rows keep every destination reachable at narrow widths and high UI scales.
            float available = Mathf.Max(120, width - 32);
            int columns = Mathf.Max(1, Mathf.Min(labels.Length, Mathf.FloorToInt(available / 140)));
            return GUILayout.SelectionGrid(selected, labels, columns, DeucarianEditorWorkbenchGUI.InputStyles.Tab, options);
        }
    }
}
