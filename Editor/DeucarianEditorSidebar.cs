using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorSidebar
    {
        private static GUIStyle sidebarStyle;
        private static GUIStyle buttonStyle;
        private static GUIStyle selectedButtonStyle;
        private static GUIStyle disabledButtonStyle;
        private static GUIStyle headingStyle;

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
            GUIStyle style = enabled
                ? selected ? SelectedButtonStyle : ButtonStyle
                : DisabledButtonStyle;
            using (new EditorGUI.DisabledScope(!enabled))
            {
                return GUILayout.Button(new GUIContent(label ?? string.Empty, tooltip ?? string.Empty), style, options);
            }
        }

        private static GUIStyle ButtonStyle
        {
            get
            {
                if (buttonStyle == null)
                {
                    buttonStyle = CreateButtonStyle(new Color(0.10f, 0.15f, 0.19f, 0.45f), DeucarianEditorTheme.MutedText, FontStyle.Bold);
                }

                return buttonStyle;
            }
        }

        private static GUIStyle SelectedButtonStyle
        {
            get
            {
                if (selectedButtonStyle == null)
                {
                    selectedButtonStyle = CreateButtonStyle(new Color(0.13f, 0.34f, 0.36f, 0.76f), DeucarianEditorTheme.Text, FontStyle.Bold);
                }

                return selectedButtonStyle;
            }
        }

        private static GUIStyle DisabledButtonStyle
        {
            get
            {
                if (disabledButtonStyle == null)
                {
                    disabledButtonStyle = CreateButtonStyle(new Color(0.08f, 0.11f, 0.14f, 0.35f), new Color(0.44f, 0.51f, 0.56f, 0.75f), FontStyle.Normal);
                }

                return disabledButtonStyle;
            }
        }

        private static GUIStyle CreateButtonStyle(Color background, Color text, FontStyle fontStyle)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = fontStyle,
                fixedHeight = 38f,
                padding = new RectOffset(12, 8, 5, 5),
                margin = new RectOffset(0, 0, 0, 6),
                border = new RectOffset(4, 4, 4, 4)
            };
            style.normal.background = DeucarianEditorTextures.Solid("sidebar-button-" + background.GetHashCode(), background);
            style.hover.background = DeucarianEditorTextures.Solid("sidebar-button-hover-" + background.GetHashCode(), DeucarianEditorColors.WithAlpha(DeucarianEditorColors.Teal, 0.34f));
            style.normal.textColor = text;
            style.hover.textColor = DeucarianEditorTheme.Text;
            return style;
        }
    }
}
