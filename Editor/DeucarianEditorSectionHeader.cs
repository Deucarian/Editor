using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorSectionHeader
    {
        private static GUIStyle titleStyle;
        private static GUIStyle summaryStyle;

        public static void Draw(string title, string summary = null)
        {
            Rect accent = GUILayoutUtility.GetRect(1f, 2f, GUILayout.ExpandWidth(true));
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(accent, DeucarianEditorTheme.Accent);
            }

            EditorGUILayout.LabelField(title ?? string.Empty, TitleStyle);
            if (!string.IsNullOrWhiteSpace(summary))
            {
                EditorGUILayout.LabelField(summary, SummaryStyle);
            }
        }

        private static GUIStyle TitleStyle
        {
            get
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        wordWrap = true
                    };
                    titleStyle.normal.textColor = DeucarianEditorTheme.Text;
                }

                return titleStyle;
            }
        }

        private static GUIStyle SummaryStyle
        {
            get
            {
                if (summaryStyle == null)
                {
                    summaryStyle = new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true
                    };
                    summaryStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return summaryStyle;
            }
        }
    }
}
