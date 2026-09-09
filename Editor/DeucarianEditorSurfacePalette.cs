using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Semantic surface roles shared by workspace, inspector, graph and IMGUI presentation.</summary>
    public static class DeucarianEditorSurfacePalette
    {
        public static Color Background => Pick(35, 40, 43, 246, 248, 250);
        public static Color Sidebar => Pick(32, 37, 40, 237, 241, 244);
        public static Color Field => Pick(36, 41, 44, 255, 255, 255);
        public static Color Hover => Pick(48, 57, 61, 228, 237, 240);
        public static Color Border => Pick(65, 73, 78, 189, 201, 208);
        public static Color Text => Pick(242, 244, 247, 32, 42, 50);
        public static Color Muted => Pick(186, 195, 205, 82, 97, 109);
        public static Color Accent => Pick(114, 225, 223, 0, 105, 112);
        public static Color Selected => Pick(33, 78, 82, 213, 238, 238);
        public static Color Primary => Pick(23, 143, 148, 8, 123, 129);
        public static Color PrimaryHover => Pick(27, 163, 168, 8, 104, 109);
        public static Color Warning => Pick(255, 203, 74, 149, 99, 0);
        public static Color Error => Pick(255, 107, 112, 198, 49, 64);
        public static Color Info => Pick(86, 186, 255, 22, 110, 176);
        public static Color Success => Pick(105, 206, 160, 32, 118, 79);

        private static Color Pick(byte r, byte g, byte b, byte lr, byte lg, byte lb) =>
            DeucarianEditorTheme.IsDark ? new Color32(r, g, b, 255) : new Color32(lr, lg, lb, 255);
    }
}
