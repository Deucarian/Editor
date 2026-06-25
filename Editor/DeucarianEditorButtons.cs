using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorButtons
    {
        private static GUIStyle primaryButton;
        private static GUIStyle secondaryButton;
        private static GUIStyle disabledButton;

        public static GUIStyle PrimaryStyle
        {
            get
            {
                if (primaryButton == null)
                {
                    primaryButton = CreateBaseButton(true);
                    primaryButton.normal.background = DeucarianEditorTextures.Solid("button-primary", new Color(0.13f, 0.40f, 0.42f, 0.90f));
                    primaryButton.hover.background = DeucarianEditorTextures.Solid("button-primary-hover", new Color(0.16f, 0.50f, 0.52f, 0.95f));
                    primaryButton.active.background = DeucarianEditorTextures.Solid("button-primary-active", new Color(0.10f, 0.32f, 0.35f, 1f));
                }

                return primaryButton;
            }
        }

        public static GUIStyle SecondaryStyle
        {
            get
            {
                if (secondaryButton == null)
                {
                    secondaryButton = CreateBaseButton(false);
                    secondaryButton.normal.background = DeucarianEditorTextures.Solid("button-secondary", new Color(0.11f, 0.18f, 0.23f, 0.82f));
                    secondaryButton.hover.background = DeucarianEditorTextures.Solid("button-secondary-hover", new Color(0.14f, 0.25f, 0.30f, 0.88f));
                    secondaryButton.active.background = DeucarianEditorTextures.Solid("button-secondary-active", new Color(0.08f, 0.14f, 0.19f, 0.94f));
                }

                return secondaryButton;
            }
        }

        public static GUIStyle DisabledStyle
        {
            get
            {
                if (disabledButton == null)
                {
                    disabledButton = CreateBaseButton(false);
                    disabledButton.normal.background = DeucarianEditorTextures.Solid("button-disabled", new Color(0.12f, 0.15f, 0.18f, 0.62f));
                    disabledButton.normal.textColor = new Color(0.50f, 0.58f, 0.63f, 0.92f);
                }

                return disabledButton;
            }
        }

        public static bool Primary(string label, bool enabled, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(label ?? string.Empty);
            using (new EditorGUI.DisabledScope(!enabled))
            {
                return GUILayout.Button(content, enabled ? PrimaryStyle : DisabledStyle, options);
            }
        }

        public static bool Secondary(string label, bool enabled = true, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(label ?? string.Empty);
            using (new EditorGUI.DisabledScope(!enabled))
            {
                return GUILayout.Button(content, enabled ? SecondaryStyle : DisabledStyle, options);
            }
        }

        private static GUIStyle CreateBaseButton(bool primary)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = primary ? FontStyle.Bold : FontStyle.Normal,
                fixedHeight = DeucarianEditorSpacing.ControlHeight,
                padding = new RectOffset(10, 10, 3, 4),
                border = new RectOffset(4, 4, 4, 4)
            };
            style.normal.textColor = DeucarianEditorTheme.Text;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            return style;
        }
    }
}
