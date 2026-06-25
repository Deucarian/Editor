using System;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorCards
    {
        public static void DrawHeaderCard(string title, string subtitle, string meta = null)
        {
            BeginCard(null, true);
            DrawAccentLine();
            EditorGUILayout.LabelField(title ?? string.Empty, HeaderTitleStyle);
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                EditorGUILayout.LabelField(subtitle, HeaderSubtitleStyle);
            }

            if (!string.IsNullOrWhiteSpace(meta))
            {
                GUILayout.Space(DeucarianEditorSpacing.Small);
                DeucarianEditorStatusBadge.Draw(meta, DeucarianEditorStatus.Info, GUILayout.Width(150f));
            }

            EndCard();
        }

        public static void DrawCard(string title, Action content, string subtitle = null)
        {
            BeginCard(title, false, subtitle);
            content?.Invoke();
            EndCard();
        }

        public static Rect BeginCard(string title = null, bool header = false, string subtitle = null)
        {
            GUIStyle style = header ? HeaderCardStyle : CardStyle;
            Rect rect = EditorGUILayout.BeginVertical(style);
            DeucarianEditorVisualShell.DrawFrostedSurface(
                rect,
                header ? DeucarianEditorTheme.GlassPanelStrong : DeucarianEditorTheme.GlassPanel,
                header ? DeucarianEditorTheme.Accent : DeucarianEditorTheme.Border);

            if (!string.IsNullOrWhiteSpace(title))
            {
                DrawAccentLine();
                EditorGUILayout.LabelField(title, SectionTitleStyle);
                if (!string.IsNullOrWhiteSpace(subtitle))
                {
                    EditorGUILayout.LabelField(subtitle, MutedStyle);
                    GUILayout.Space(DeucarianEditorSpacing.Small);
                }
            }

            return rect;
        }

        public static void EndCard()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(DeucarianEditorSpacing.Small);
        }

        public static void DrawInlineCard(Action content)
        {
            Rect rect = EditorGUILayout.BeginVertical(InlineCardStyle);
            DeucarianEditorVisualShell.DrawInsetSurface(
                rect,
                DeucarianEditorTheme.GlassPanelSoft,
                DeucarianEditorTheme.BorderSubtle,
                DeucarianEditorSpacing.CardRadius);
            content?.Invoke();
            EditorGUILayout.EndVertical();
        }

        public static void DrawAccentLine()
        {
            Rect rect = GUILayoutUtility.GetRect(1f, 2f, GUILayout.ExpandWidth(true));
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, DeucarianEditorTheme.Accent);
            }
        }

        private static GUIStyle cardStyle;
        private static GUIStyle headerCardStyle;
        private static GUIStyle inlineCardStyle;
        private static GUIStyle headerTitleStyle;
        private static GUIStyle headerSubtitleStyle;
        private static GUIStyle sectionTitleStyle;
        private static GUIStyle mutedStyle;

        private static GUIStyle CardStyle
        {
            get
            {
                if (cardStyle == null)
                {
                    cardStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(14, 14, 12, 12),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return cardStyle;
            }
        }

        private static GUIStyle HeaderCardStyle
        {
            get
            {
                if (headerCardStyle == null)
                {
                    headerCardStyle = new GUIStyle(CardStyle)
                    {
                        padding = new RectOffset(18, 18, 14, 14)
                    };
                }

                return headerCardStyle;
            }
        }

        private static GUIStyle InlineCardStyle
        {
            get
            {
                if (inlineCardStyle == null)
                {
                    inlineCardStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(10, 10, 8, 8),
                        margin = new RectOffset(0, 0, 4, 8)
                    };
                }

                return inlineCardStyle;
            }
        }

        private static GUIStyle HeaderTitleStyle
        {
            get
            {
                if (headerTitleStyle == null)
                {
                    headerTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 20,
                        fontStyle = FontStyle.Bold,
                        wordWrap = true
                    };
                    headerTitleStyle.normal.textColor = DeucarianEditorTheme.Text;
                }

                return headerTitleStyle;
            }
        }

        private static GUIStyle HeaderSubtitleStyle
        {
            get
            {
                if (headerSubtitleStyle == null)
                {
                    headerSubtitleStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 12,
                        wordWrap = true
                    };
                    headerSubtitleStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return headerSubtitleStyle;
            }
        }

        private static GUIStyle SectionTitleStyle
        {
            get
            {
                if (sectionTitleStyle == null)
                {
                    sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        wordWrap = true
                    };
                    sectionTitleStyle.normal.textColor = DeucarianEditorTheme.Text;
                }

                return sectionTitleStyle;
            }
        }

        private static GUIStyle MutedStyle
        {
            get
            {
                if (mutedStyle == null)
                {
                    mutedStyle = new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true
                    };
                    mutedStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return mutedStyle;
            }
        }
    }
}
