using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>
    /// Package-owned DINish typography roles. Missing assets safely fall back to
    /// Unity's current editor font so consumers can update without a hard failure.
    /// </summary>
    public static class DeucarianEditorTypography
    {
        private static Font display;
        private static Font body;
        private static Font strong;

        public static Font Display => display != null ? display : (display = DeucarianEditorUIResources.LoadDisplayFont());
        public static Font Body => body != null ? body : (body = DeucarianEditorUIResources.LoadBodyFont());
        public static Font Strong => strong != null ? strong : (strong = DeucarianEditorUIResources.LoadStrongFont());

        public static GUIStyle ApplyDisplay(GUIStyle style)
        {
            return Apply(style, Display, FontStyle.Normal);
        }

        public static GUIStyle ApplyBody(GUIStyle style)
        {
            return Apply(style, Body, FontStyle.Normal);
        }

        public static GUIStyle ApplyStrong(GUIStyle style)
        {
            return Apply(style, Strong, FontStyle.Normal);
        }

        public static void ClearCache()
        {
            display = null;
            body = null;
            strong = null;
        }

        private static GUIStyle Apply(GUIStyle style, Font font, FontStyle fontStyle)
        {
            if (style == null)
            {
                return null;
            }

            if (font != null)
            {
                style.font = font;
                style.fontStyle = fontStyle;
            }

            return style;
        }
    }
}
