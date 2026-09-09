using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorTextures
    {
        internal static Texture2D Bordered(string name, Color fill, Color border) =>
            DeucarianEditorTextureCache.GetBordered(name + "-" + DeucarianEditorTheme.IsDark, fill, border);

        public static Texture2D Background()
        {
            return DeucarianEditorVisualShell.GetDefaultBackgroundTexture();
        }

        public static Texture2D Solid(string name, Color color)
        {
            return DeucarianEditorTextureCache.Get(string.IsNullOrWhiteSpace(name) ? "solid" : name, color);
        }
    }
}
