using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorSidebar
    {
        private static GUIStyle sidebarStyle;
        private static GUIStyle headingStyle;
        private static GUIStyle itemLayoutStyle;
        private static GUIStyle itemLabelStyle;
        private static GUIStyle selectedItemLabelStyle;
        private static GUIStyle disabledItemLabelStyle;

        public static GUIStyle ContainerStyle
        {
            get
            {
                if (sidebarStyle == null)
                {
                    sidebarStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(12, 12, 12, 12),
                        margin = new RectOffset(0, 10, 0, 0)
                    };
                    sidebarStyle.normal.background = DeucarianEditorTextures.Solid("sidebar", DeucarianEditorTheme.GlassPanel);
                }

                return sidebarStyle;
            }
        }

        public static GUIStyle HeadingStyle
        {
            get
            {
                if (headingStyle == null)
                {
                    headingStyle = new GUIStyle(EditorStyles.miniBoldLabel)
                    {
                        fontSize = 11,
                        wordWrap = true,
                        margin = new RectOffset(0, 0, 0, 8)
                    };
                    headingStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return headingStyle;
            }
        }

        public static bool DrawItem(string label, string tooltip, bool selected, bool enabled, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(label ?? string.Empty, tooltip ?? string.Empty);
            Rect rect = GUILayoutUtility.GetRect(content, ItemLayoutStyle, options);
            Event currentEvent = Event.current;

            if (currentEvent != null && currentEvent.type == EventType.Repaint)
            {
                bool hovered = enabled && rect.Contains(currentEvent.mousePosition);
                Color background = selected
                    ? new Color(0.07f, 0.30f, 0.32f, 0.82f)
                    : hovered
                        ? new Color(0.09f, 0.22f, 0.27f, 0.78f)
                        : enabled
                            ? new Color(0.07f, 0.13f, 0.17f, 0.68f)
                            : new Color(0.07f, 0.09f, 0.11f, 0.52f);
                Color border = selected
                    ? DeucarianEditorTheme.Accent
                    : hovered
                        ? DeucarianEditorColors.WithAlpha(DeucarianEditorColors.Teal, 0.44f)
                        : DeucarianEditorTheme.BorderSubtle;
                DeucarianEditorVisualShell.DrawInsetSurface(rect, background, border, 5f);

                if (selected)
                {
                    Rect accentRect = new Rect(rect.x, rect.y + 4f, 3f, rect.height - 8f);
                    EditorGUI.DrawRect(accentRect, DeucarianEditorTheme.Accent);
                }
            }

            if (enabled)
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            }

            Rect labelRect = new Rect(rect.x + 12f, rect.y, rect.width - 20f, rect.height);
            GUI.Label(labelRect, content, enabled ? selected ? SelectedItemLabelStyle : ItemLabelStyle : DisabledItemLabelStyle);

            if (enabled && currentEvent != null && currentEvent.type == EventType.MouseUp && rect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();
                return true;
            }

            return false;
        }

        private static GUIStyle ItemLayoutStyle
        {
            get
            {
                if (itemLayoutStyle == null)
                {
                    itemLayoutStyle = new GUIStyle(GUIStyle.none)
                    {
                        fixedHeight = 38f,
                        margin = new RectOffset(0, 0, 0, 6)
                    };
                }

                return itemLayoutStyle;
            }
        }

        private static GUIStyle ItemLabelStyle
        {
            get
            {
                if (itemLabelStyle == null)
                {
                    itemLabelStyle = CreateItemLabelStyle(DeucarianEditorTheme.MutedText, FontStyle.Bold);
                }

                return itemLabelStyle;
            }
        }

        private static GUIStyle SelectedItemLabelStyle
        {
            get
            {
                if (selectedItemLabelStyle == null)
                {
                    selectedItemLabelStyle = CreateItemLabelStyle(DeucarianEditorTheme.Text, FontStyle.Bold);
                }

                return selectedItemLabelStyle;
            }
        }

        private static GUIStyle DisabledItemLabelStyle
        {
            get
            {
                if (disabledItemLabelStyle == null)
                {
                    disabledItemLabelStyle = CreateItemLabelStyle(new Color(0.44f, 0.51f, 0.56f, 0.72f), FontStyle.Normal);
                }

                return disabledItemLabelStyle;
            }
        }

        private static GUIStyle CreateItemLabelStyle(Color textColor, FontStyle fontStyle)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = fontStyle,
                clipping = TextClipping.Ellipsis,
                padding = new RectOffset(0, 0, 0, 1)
            };
            style.normal.textColor = textColor;
            return style;
        }
    }
}
