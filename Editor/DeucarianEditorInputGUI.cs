using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.Editor
{
    /// <summary>Aligned form controls. Native serialized property drawers keep their Unity editing contract.</summary>
    public static class DeucarianEditorInputGUI
    {
        public const float RowHeight = 32;
        private static DeucarianEditorInputStyles Styles => DeucarianEditorWorkbenchGUI.InputStyles;

        public static bool Toggle(bool value, params GUILayoutOption[] options) =>
            EditorGUI.Toggle(Row(GUIContent.none, options), value);
        public static double DoubleField(double value, params GUILayoutOption[] options) =>
            EditorGUI.DoubleField(Row(GUIContent.none, options), value, Styles.Text);
        public static double DelayedDoubleField(double value, params GUILayoutOption[] options) =>
            EditorGUI.DelayedDoubleField(Row(GUIContent.none, options), value, Styles.Text);
        public static long LongField(long value, params GUILayoutOption[] options) =>
            EditorGUI.LongField(Row(GUIContent.none, options), value, Styles.Text);
        public static string DelayedTextField(string value, params GUILayoutOption[] options) =>
            EditorGUI.DelayedTextField(Row(GUIContent.none, options), value, Styles.Text);

        public static string TextField(string label, string value, params GUILayoutOption[] options) =>
            TextField(new GUIContent(label), value, options);
        public static string TextField(GUIContent label, string value, params GUILayoutOption[] options) =>
            EditorGUI.TextField(Row(label, options), value, Styles.Text);
        public static string TextField(string value, params GUILayoutOption[] options) =>
            EditorGUI.TextField(Row(GUIContent.none, options), value, Styles.Text);
        public static string TextField(string value, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUI.TextField(Row(GUIContent.none, options), value, style);
        public static string DelayedTextField(string label, string value, params GUILayoutOption[] options) =>
            EditorGUI.DelayedTextField(Row(new GUIContent(label), options), value, Styles.Text);
        public static string PasswordField(string label, string value, params GUILayoutOption[] options) =>
            EditorGUI.PasswordField(Row(new GUIContent(label), options), value, Styles.Text);
        public static string PasswordField(string value, params GUILayoutOption[] options) =>
            EditorGUI.PasswordField(Row(GUIContent.none, options), value, Styles.Text);
        public static string TextArea(string value, params GUILayoutOption[] options) =>
            EditorGUILayout.TextArea(value, Styles.TextArea, options);
        public static string TextArea(string value, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUILayout.TextArea(value, style, options);
        public static bool Toggle(string label, bool value, params GUILayoutOption[] options) =>
            EditorGUI.Toggle(Row(new GUIContent(label), options), value);
        public static bool Toggle(GUIContent label, bool value, params GUILayoutOption[] options) =>
            EditorGUI.Toggle(Row(label, options), value);
        public static bool ToggleLeft(string label, bool value, params GUILayoutOption[] options) =>
            EditorGUI.ToggleLeft(Row(GUIContent.none, options), label, value, DeucarianEditorWorkbenchGUI.LabelStyle);
        public static bool ToggleLeft(GUIContent label, bool value, params GUILayoutOption[] options) =>
            EditorGUI.ToggleLeft(Row(GUIContent.none, options), label, value, DeucarianEditorWorkbenchGUI.LabelStyle);
        public static Enum EnumPopup(string label, Enum value, params GUILayoutOption[] options) =>
            EditorGUI.EnumPopup(Row(new GUIContent(label), options), value, Styles.Popup);
        public static Enum EnumPopup(GUIContent label, Enum value, params GUILayoutOption[] options) =>
            EditorGUI.EnumPopup(Row(label, options), value, Styles.Popup);
        public static int Popup(string label, int value, string[] choices, params GUILayoutOption[] options) =>
            EditorGUI.Popup(Row(new GUIContent(label), options), value, choices, Styles.Popup);
        public static int Popup(int value, string[] choices, params GUILayoutOption[] options) =>
            EditorGUI.Popup(Row(GUIContent.none, options), value, choices, Styles.Popup);
        public static int Popup(int value, string[] choices, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUI.Popup(Row(GUIContent.none, options), value, choices, style);
        public static Object ObjectField(string label, Object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options) =>
            EditorGUI.ObjectField(Row(new GUIContent(label), options), value, type, allowSceneObjects);
        public static Object ObjectField(GUIContent label, Object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options) =>
            EditorGUI.ObjectField(Row(label, options), value, type, allowSceneObjects);
        public static Object ObjectField(Object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options) =>
            EditorGUI.ObjectField(Row(GUIContent.none, options), value, type, allowSceneObjects);
        public static double DoubleField(string label, double value, params GUILayoutOption[] options) =>
            EditorGUI.DoubleField(Row(new GUIContent(label), options), value, Styles.Text);
        public static double DelayedDoubleField(string label, double value, params GUILayoutOption[] options) =>
            EditorGUI.DelayedDoubleField(Row(new GUIContent(label), options), value, Styles.Text);
        public static long LongField(string label, long value, params GUILayoutOption[] options) =>
            EditorGUI.LongField(Row(new GUIContent(label), options), value, Styles.Text);
        public static float Slider(string label, float value, float min, float max, params GUILayoutOption[] options) =>
            EditorGUI.Slider(Row(new GUIContent(label), options), value, min, max);
        public static int IntSlider(string label, int value, int min, int max, params GUILayoutOption[] options) =>
            EditorGUI.IntSlider(Row(new GUIContent(label), options), value, min, max);
        public static bool Foldout(bool value, string label, bool toggleOnLabelClick = false, params GUILayoutOption[] options) =>
            EditorGUI.Foldout(Row(GUIContent.none, options), value, label, toggleOnLabelClick, DeucarianEditorWorkbenchGUI.FoldoutStyle);
        public static bool Foldout(bool value, string label, bool toggleOnLabelClick, GUIStyle style, params GUILayoutOption[] options) =>
            Foldout(value, new GUIContent(label), toggleOnLabelClick, style, options);
        public static bool Foldout(bool value, GUIContent label, bool toggleOnLabelClick, GUIStyle style, params GUILayoutOption[] options) =>
            EditorGUI.Foldout(Row(GUIContent.none, options), value, label, toggleOnLabelClick, style);

        private static Rect Row(GUIContent label, GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, RowHeight, options);
            if (label == null || string.IsNullOrEmpty(label.text)) return rect;
            float previous = EditorGUIUtility.labelWidth;
            try
            {
                EditorGUIUtility.labelWidth = Mathf.Min(previous, Mathf.Clamp(rect.width * .4f, 80, 180));
                return EditorGUI.PrefixLabel(rect, label, DeucarianEditorWorkbenchGUI.LabelStyle);
            }
            finally { EditorGUIUtility.labelWidth = previous; }
        }
    }
}
