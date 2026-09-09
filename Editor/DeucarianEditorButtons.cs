using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorButtons
    {
        private static GUIStyle primaryButton;
        private static GUIStyle secondaryButton;
        private static GUIStyle disabledButton;

        public static GUIStyle PrimaryStyle => Resolve(ref primaryButton, "primary", true);
        public static GUIStyle SecondaryStyle => Resolve(ref secondaryButton, "secondary", false);
        public static GUIStyle DisabledStyle => Resolve(ref disabledButton, "disabled", false);

        public static bool Primary(string label, bool enabled, params GUILayoutOption[] options)
        {
            using (new EditorGUI.DisabledScope(!enabled))
                return GUILayout.Button(new GUIContent(label ?? string.Empty), PrimaryStyle, options);
        }

        public static bool Secondary(string label, bool enabled = true, params GUILayoutOption[] options)
        {
            using (new EditorGUI.DisabledScope(!enabled))
                return GUILayout.Button(new GUIContent(label ?? string.Empty), SecondaryStyle, options);
        }

        private static GUIStyle Resolve(ref GUIStyle cached, string role, bool primary)
        {
            string key = role + "-" + DeucarianEditorTheme.IsDark;
            if (cached != null && cached.name == key) return cached;
            var style = DeucarianEditorStyles.CopyStyle(() => EditorStyles.miniButton);
            style.name = key;
            style.fontSize = 16;
            style.fontStyle = primary ? FontStyle.Bold : FontStyle.Normal;
            if (primary) DeucarianEditorTypography.ApplyStrong(style);
            else DeucarianEditorTypography.ApplyBody(style);
            style.alignment = TextAnchor.MiddleCenter;
            style.fixedHeight = primary ? 42 : 36;
            style.padding = new RectOffset(14, 14, 4, 4);
            style.margin = new RectOffset(2, 2, 4, 4);
            style.border = new RectOffset(4, 4, 4, 4);
            DeucarianEditorInputStyles.SetText(style, primary ? Color.white : DeucarianEditorSurfacePalette.Text);
            style.normal.background = DeucarianEditorTextures.Bordered("button-" + role,
                primary ? DeucarianEditorSurfacePalette.Primary : DeucarianEditorSurfacePalette.Field,
                primary ? DeucarianEditorSurfacePalette.Accent : DeucarianEditorSurfacePalette.Border);
            style.hover.background = DeucarianEditorTextures.Bordered("button-hover-" + role,
                primary ? DeucarianEditorSurfacePalette.PrimaryHover : DeucarianEditorSurfacePalette.Hover,
                DeucarianEditorSurfacePalette.Accent);
            style.active.background = DeucarianEditorTextures.Bordered("button-active-" + role,
                DeucarianEditorSurfacePalette.Selected, DeucarianEditorSurfacePalette.Accent);
            style.focused.background = style.hover.background;
            style.onNormal.background = style.active.background;
            style.onHover.background = style.hover.background;
            style.onActive.background = style.active.background;
            style.onFocused.background = style.focused.background;
            if (role == "disabled") DeucarianEditorInputStyles.SetText(style, DeucarianEditorSurfacePalette.Muted);
            cached = style;
            return cached;
        }
    }
}
