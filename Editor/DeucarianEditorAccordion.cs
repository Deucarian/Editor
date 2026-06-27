using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorAccordion
    {
        private const string KeyPrefix = "Deucarian.Editor.Accordion.";

        private static GUIStyle headerStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle summaryStyle;
        private static GUIStyle indicatorStyle;

        public static string BuildStateKey(params string[] segments)
        {
            var builder = new StringBuilder(KeyPrefix);
            if (segments == null || segments.Length == 0)
            {
                builder.Append("default");
                return builder.ToString();
            }

            for (int i = 0; i < segments.Length; i++)
            {
                if (i > 0) builder.Append('/');
                builder.Append(NormalizeSegment(segments[i]));
            }

            return builder.ToString();
        }

        public static bool GetFoldoutState(string stateKey, bool defaultOpen)
        {
            return EditorPrefs.GetBool(NormalizeKey(stateKey), defaultOpen);
        }

        public static void SetFoldoutState(string stateKey, bool open)
        {
            EditorPrefs.SetBool(NormalizeKey(stateKey), open);
        }

        public static bool DrawFoldoutCard(
            string stateKey,
            string title,
            string summary,
            Action drawContent,
            bool defaultOpen = true,
            bool enabled = true,
            Action drawHeaderActions = null)
        {
            bool open = GetFoldoutState(stateKey, defaultOpen);
            string key = NormalizeKey(stateKey);
            Rect headerRect = EditorGUILayout.BeginHorizontal(HeaderStyle, GUILayout.MinHeight(40f));
            DrawHeaderBackground(headerRect, open, enabled);

            GUILayout.Label(open ? "v" : ">", IndicatorStyle, GUILayout.Width(18f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title ?? string.Empty, TitleStyle);
            if (!string.IsNullOrWhiteSpace(summary))
            {
                EditorGUILayout.LabelField(summary, SummaryStyle);
            }

            EditorGUILayout.EndVertical();
            if (drawHeaderActions != null)
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    drawHeaderActions();
                }
            }

            EditorGUILayout.EndHorizontal();

            Rect toggleRect = headerRect;
            if (drawHeaderActions != null)
            {
                toggleRect.xMax = Mathf.Max(toggleRect.xMin, toggleRect.xMax - 132f);
            }

            if (enabled && Event.current != null && Event.current.type == EventType.MouseDown && Event.current.button == 0 && toggleRect.Contains(Event.current.mousePosition))
            {
                open = !open;
                SetFoldoutState(key, open);
                GUI.changed = true;
                Event.current.Use();
            }

            if (open)
            {
                Rect bodyRect = EditorGUILayout.BeginVertical(BodyStyle);
                if (Event.current != null && Event.current.type == EventType.Repaint)
                {
                    DeucarianEditorVisualShell.DrawInsetSurface(
                        bodyRect,
                        DeucarianEditorTheme.GlassPanelSoft,
                        DeucarianEditorTheme.BorderSubtle,
                        DeucarianEditorSpacing.CardRadius);
                }

                using (new EditorGUI.DisabledScope(!enabled))
                {
                    drawContent?.Invoke();
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(DeucarianEditorSpacing.Small);
            }
            else
            {
                GUILayout.Space(DeucarianEditorSpacing.Tiny);
            }

            return open;
        }

        public static string NormalizeSegmentForTests(string segment)
        {
            return NormalizeSegment(segment);
        }

        private static void DrawHeaderBackground(Rect rect, bool open, bool enabled)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            bool hover = enabled && rect.Contains(Event.current.mousePosition);
            Color background = open
                ? new Color(0.08f, 0.25f, 0.28f, 0.80f)
                : new Color(0.07f, 0.13f, 0.17f, 0.66f);
            if (hover)
            {
                background = open
                    ? new Color(0.10f, 0.32f, 0.34f, 0.86f)
                    : new Color(0.09f, 0.20f, 0.24f, 0.78f);
            }

            Color border = open ? DeucarianEditorTheme.Accent : DeucarianEditorTheme.BorderSubtle;
            DeucarianEditorVisualShell.DrawInsetSurface(rect, background, border, DeucarianEditorSpacing.CardRadius);
            if (open)
            {
                Rect accent = new Rect(rect.x + 1f, rect.y + 3f, 3f, Mathf.Max(0f, rect.height - 6f));
                EditorGUI.DrawRect(accent, DeucarianEditorTheme.Accent);
            }
        }

        private static string NormalizeKey(string stateKey)
        {
            return string.IsNullOrWhiteSpace(stateKey) ? BuildStateKey("default") : stateKey;
        }

        private static string NormalizeSegment(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                return "empty";
            }

            string trimmed = segment.Trim();
            var builder = new StringBuilder(trimmed.Length);
            bool previousSeparator = false;
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                bool valid = char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_';
                if (valid)
                {
                    builder.Append(char.ToLowerInvariant(c));
                    previousSeparator = false;
                }
                else if (!previousSeparator)
                {
                    builder.Append('_');
                    previousSeparator = true;
                }
            }

            string result = builder.ToString().Trim('_');
            return string.IsNullOrWhiteSpace(result) ? "empty" : result;
        }

        private static GUIStyle HeaderStyle
        {
            get
            {
                if (headerStyle == null)
                {
                    headerStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(12, 12, 8, 8),
                        margin = new RectOffset(0, 0, 2, 0)
                    };
                }

                return headerStyle;
            }
        }

        private static GUIStyle BodyStyle
        {
            get
            {
                if (bodyStyle == null)
                {
                    bodyStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(12, 12, 10, 12),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return bodyStyle;
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
                        wordWrap = true,
                        fontSize = 13,
                        fontStyle = FontStyle.Bold
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
                    summaryStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        wordWrap = true
                    };
                    summaryStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return summaryStyle;
            }
        }

        private static GUIStyle IndicatorStyle
        {
            get
            {
                if (indicatorStyle == null)
                {
                    indicatorStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13
                    };
                    indicatorStyle.normal.textColor = DeucarianEditorTheme.Accent;
                }

                return indicatorStyle;
            }
        }
    }
}
