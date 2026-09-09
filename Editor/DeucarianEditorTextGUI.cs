using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Readable, wrapping text for both standalone and embedded editor workflows.</summary>
    public static class DeucarianEditorTextGUI
    {
        public static void LabelField(string text, params GUILayoutOption[] options) =>
            EditorGUILayout.LabelField(text, DeucarianEditorWorkbenchGUI.LabelStyle, options);
        public static void LabelField(GUIContent text, params GUILayoutOption[] options) =>
            EditorGUILayout.LabelField(text, DeucarianEditorWorkbenchGUI.LabelStyle, options);
        public static void LabelField(string text, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUILayout.LabelField(text, style, options);
        public static void LabelField(GUIContent text, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUILayout.LabelField(text, style, options);
        public static void LabelField(string label, string value, params GUILayoutOption[] options) =>
            LabelField(new GUIContent(label), new GUIContent(value), options);
        public static void LabelField(GUIContent label, GUIContent value, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.HorizontalScope(options))
            {
                EditorGUILayout.LabelField(label, DeucarianEditorWorkbenchGUI.MutedMiniLabelStyle,
                    GUILayout.Width(Mathf.Min(EditorGUIUtility.labelWidth, 180)));
                EditorGUILayout.LabelField(value, DeucarianEditorWorkbenchGUI.LabelStyle, GUILayout.MinWidth(0));
            }
        }
        public static void LabelField(string label, string value, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUILayout.LabelField(label, value, style, options);
        public static void HelpBox(string message, MessageType type, bool wide = true)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            string icon = type == MessageType.Error ? "circle-x" : type == MessageType.Warning ? "triangle-alert" : "info";
            Color color = type == MessageType.Error ? DeucarianEditorSurfacePalette.Error :
                type == MessageType.Warning ? DeucarianEditorSurfacePalette.Warning : DeucarianEditorSurfacePalette.Muted;
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect marker = GUILayoutUtility.GetRect(18, 22, GUILayout.Width(18));
                DeucarianEditorIcons.DrawIcon(marker, DeucarianEditorIcons.GetIcon(icon), color);
                GUILayout.Space(8);
                EditorGUILayout.LabelField(message, DeucarianEditorWorkbenchGUI.WordWrappedMiniLabelStyle,
                    GUILayout.MinWidth(0));
            }
            GUILayout.Space(4);
        }
    }
}
