using System;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Owned copies; styling a Deucarian surface never mutates Unity's shared EditorStyles.</summary>
    public sealed class DeucarianEditorInputStyles
    {
        public GUIStyle Text { get; }
        public GUIStyle TextArea { get; }
        public GUIStyle Popup { get; }
        public GUIStyle Toggle { get; }
        public GUIStyle Tab { get; }
        public GUIStyle Toolbar { get; }
        public GUIStyle Search { get; }
        public GUIStyle NativeCaption { get; }
        public GUIStyle SliderTrack { get; }
        public GUIStyle SliderThumb { get; }
        public GUIStyle Switch { get; }

        internal DeucarianEditorInputStyles()
        {
            Text = Field(() => EditorStyles.textField);
            TextArea = Field(() => EditorStyles.textArea);
            TextArea.wordWrap = true;
            TextArea.alignment = TextAnchor.UpperLeft;
            TextArea.padding = new RectOffset(10, 10, 8, 8);
            Popup = Field(() => EditorStyles.popup);
            Toggle = DeucarianEditorStyles.CopyStyle(() => EditorStyles.toggle);
            DeucarianEditorTypography.ApplyBody(Toggle);
            Toggle.fontSize = 16;
            SetText(Toggle, DeucarianEditorSurfacePalette.Text);
            Tab = new GUIStyle(DeucarianEditorButtons.SecondaryStyle);
            Tab.fixedHeight = 36;
            Tab.normal.background = null;
            Tab.onNormal.background = DeucarianEditorTextures.Solid("tab-selected-" + DeucarianEditorTheme.IsDark, DeucarianEditorSurfacePalette.Selected);
            Tab.onNormal.textColor = DeucarianEditorSurfacePalette.Accent;
            Tab.onHover.background = Tab.onNormal.background;
            Tab.onHover.textColor = Tab.onNormal.textColor;
            Toolbar = new GUIStyle { padding = new RectOffset(0, 0, 4, 8) };
            Search = Field(() => EditorStyles.toolbarSearchField);
            NativeCaption = DeucarianEditorStyles.CopyStyle(() => EditorStyles.miniLabel);
            DeucarianEditorTypography.ApplyBody(NativeCaption);
            SetText(NativeCaption, DeucarianEditorSurfacePalette.Muted);
            SliderTrack = new GUIStyle { fixedHeight = 4, stretchWidth = true, margin = new RectOffset() };
            SliderThumb = new GUIStyle { fixedWidth = 14, fixedHeight = 14 };
            SliderThumb.normal.background = DeucarianEditorTextures.Bordered("slider-thumb", Color.white, Color.white);
            SliderThumb.hover.background = SliderThumb.normal.background;
            SliderThumb.active.background = SliderThumb.normal.background;
            Switch = new GUIStyle { fixedWidth = 54, fixedHeight = 28, border = new RectOffset(4, 4, 4, 4) };
            Switch.normal.background = DeucarianEditorTextures.Bordered("switch-off",
                DeucarianEditorSurfacePalette.Border, DeucarianEditorSurfacePalette.Border);
            Switch.onNormal.background = DeucarianEditorTextures.Bordered("switch-on",
                DeucarianEditorSurfacePalette.Primary, DeucarianEditorSurfacePalette.Primary);
            Switch.hover.background = Switch.normal.background;
            Switch.onHover.background = Switch.onNormal.background;
            Switch.focused.background = DeucarianEditorTextures.Bordered("switch-focus",
                DeucarianEditorSurfacePalette.Border, DeucarianEditorSurfacePalette.Accent);
            Switch.onFocused.background = DeucarianEditorTextures.Bordered("switch-on-focus",
                DeucarianEditorSurfacePalette.Primary, DeucarianEditorSurfacePalette.Accent);
        }

        private static GUIStyle Field(Func<GUIStyle> source)
        {
            var style = DeucarianEditorStyles.CopyStyle(source);
            style.fontSize = 16;
            style.alignment = TextAnchor.MiddleLeft;
            DeucarianEditorTypography.ApplyBody(style);
            style.padding = new RectOffset(8, 8, 4, 4);
            style.fixedHeight = 0;
            SetText(style, DeucarianEditorSurfacePalette.Text);
            // Preserve popup/search affordances supplied by Unity; plain text surfaces are owned.
            if (style.name == EditorStyles.textField.name || style.name == EditorStyles.textArea.name)
            {
                style.border = new RectOffset(4, 4, 4, 4);
                style.normal.background = DeucarianEditorTextures.Bordered("input",
                    DeucarianEditorSurfacePalette.Field, DeucarianEditorSurfacePalette.Border);
                style.focused.background = DeucarianEditorTextures.Bordered("input-focus",
                    DeucarianEditorSurfacePalette.Field, DeucarianEditorSurfacePalette.Accent);
                style.hover.background = style.normal.background;
            }
            return style;
        }

        internal static void SetText(GUIStyle style, Color color)
        {
            style.normal.textColor = color; style.hover.textColor = color;
            style.active.textColor = color; style.focused.textColor = color;
            style.onNormal.textColor = color; style.onHover.textColor = color;
            style.onActive.textColor = color; style.onFocused.textColor = color;
        }
    }
}
