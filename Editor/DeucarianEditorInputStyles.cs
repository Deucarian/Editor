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
