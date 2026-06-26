using System;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorFieldRow
    {
        private static GUIStyle rowStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle hintStyle;

        public static string TextField(string label, string value, string hint = null)
        {
            string result = value ?? string.Empty;
            Draw(label, () => result = EditorGUILayout.TextField(result), hint);
            return result;
        }

        public static string TextArea(string label, string value, string hint = null)
        {
            string result = value ?? string.Empty;
            Draw(label, () => result = EditorGUILayout.TextArea(result, GUILayout.MinHeight(42f)), hint);
            return result;
        }

        public static int IntField(string label, int value, string hint = null)
        {
            int result = value;
            Draw(label, () => result = EditorGUILayout.IntField(result), hint);
            return result;
        }

        public static float FloatField(string label, float value, string hint = null)
        {
            float result = value;
            Draw(label, () => result = EditorGUILayout.FloatField(result), hint);
            return result;
        }

        public static double DoubleField(string label, double value, string hint = null)
        {
            double result = value;
            Draw(label, () => result = EditorGUILayout.DoubleField(result), hint);
            return result;
        }

        public static bool Toggle(string label, bool value, string hint = null)
        {
            bool result = value;
            Draw(label, () => result = EditorGUILayout.Toggle(result), hint);
            return result;
        }

        public static T EnumPopup<T>(string label, T value, string hint = null) where T : Enum
        {
            T result = value;
            Draw(label, () => result = (T)EditorGUILayout.EnumPopup(result), hint);
            return result;
        }

        public static void Draw(string label, Action drawField, string hint = null, bool enabled = true)
        {
            Rect rect = EditorGUILayout.BeginVertical(RowStyle);
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                DeucarianEditorVisualShell.DrawInsetSurface(
                    rect,
                    new Color(0.06f, 0.12f, 0.16f, 0.50f),
                    DeucarianEditorTheme.BorderSubtle,
                    6f);
            }

            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUI.DisabledScope(!enabled))
            {
                EditorGUILayout.LabelField(label ?? string.Empty, LabelStyle, GUILayout.Width(148f));
                drawField?.Invoke();
            }

            if (!string.IsNullOrWhiteSpace(hint))
            {
                EditorGUILayout.LabelField(hint, HintStyle);
            }

            EditorGUILayout.EndVertical();
        }

        private static GUIStyle RowStyle
        {
            get
            {
                if (rowStyle == null)
                {
                    rowStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(8, 8, 5, 6),
                        margin = new RectOffset(0, 0, 2, 5)
                    };
                }

                return rowStyle;
            }
        }

        private static GUIStyle LabelStyle
        {
            get
            {
                if (labelStyle == null)
                {
                    labelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
                    {
                        wordWrap = true,
                        alignment = TextAnchor.MiddleLeft
                    };
                    labelStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return labelStyle;
            }
        }

        private static GUIStyle HintStyle
        {
            get
            {
                if (hintStyle == null)
                {
                    hintStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        wordWrap = true,
                        margin = new RectOffset(148, 0, 2, 0)
                    };
                    hintStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return hintStyle;
            }
        }
    }
}
