using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Semantic surface roles shared by workspace, inspector, graph and IMGUI presentation.</summary>
    public static class DeucarianEditorSurfacePalette
    {
        public static Color Background => Pick(32, 43, 46, 246, 248, 250);
        public static Color Sidebar => Pick(27, 37, 40, 237, 241, 244);
        public static Color Field => Pick(34, 48, 51, 255, 255, 255);
        public static Color Hover => Pick(43, 59, 63, 228, 237, 240);
        public static Color Border => Pick(61, 78, 82, 189, 201, 208);
        public static Color Text => Pick(242, 244, 247, 32, 42, 50);
        public static Color Muted => Pick(186, 195, 205, 82, 97, 109);
        public static Color Accent => Pick(114, 225, 223, 0, 105, 112);
        public static Color Selected => Pick(33, 78, 82, 213, 238, 238);
        public static Color Primary => Pick(50, 189, 194, 8, 123, 129);
        public static Color PrimaryText => Pick(16, 33, 36, 255, 255, 255);
        public static Color PrimaryHover => Pick(83, 208, 211, 8, 104, 109);
        public static Color Warning => Pick(255, 203, 74, 149, 99, 0);
        public static Color Error => Pick(255, 107, 112, 198, 49, 64);
        public static Color Info => Pick(86, 186, 255, 22, 110, 176);
        public static Color Success => Pick(105, 206, 160, 32, 118, 79);

        private static Color Pick(byte r, byte g, byte b, byte lr, byte lg, byte lb) =>
            DeucarianEditorTheme.IsDark ? new Color32(r, g, b, 255) : new Color32(lr, lg, lb, 255);
    }
}
