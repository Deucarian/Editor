using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Semantic surface roles shared by workspace, inspector, graph and IMGUI presentation.</summary>
    public static class DeucarianEditorSurfacePalette
    {
        public static Color Background => Pick(27, 40, 44, 246, 248, 250);
        public static Color Sidebar => Pick(21, 33, 37, 237, 241, 244);
        public static Color Field => Pick(28, 43, 47, 255, 255, 255);
        public static Color Hover => Pick(39, 59, 64, 228, 237, 240);
        public static Color Border => Pick(61, 82, 89, 189, 201, 208);
        public static Color Text => Pick(242, 244, 247, 32, 42, 50);
        public static Color Muted => Pick(186, 195, 205, 82, 97, 109);
        public static Color Accent => Pick(0, 216, 230, 0, 105, 112);
        public static Color Selected => Pick(7, 83, 92, 213, 238, 238);
        public static Color Primary => Pick(0, 207, 223, 8, 123, 129);
        public static Color PrimaryText => Pick(16, 33, 36, 255, 255, 255);
        public static Color PrimaryHover => Pick(54, 224, 236, 8, 104, 109);
        public static Color Warning => Pick(255, 203, 74, 149, 99, 0);
        public static Color Error => Pick(255, 107, 112, 198, 49, 64);
        public static Color Info => Pick(86, 186, 255, 22, 110, 176);
        public static Color Success => Pick(105, 206, 160, 32, 118, 79);

        private static Color Pick(byte r, byte g, byte b, byte lr, byte lg, byte lb) =>
            DeucarianEditorTheme.IsDark ? new Color32(r, g, b, 255) : new Color32(lr, lg, lb, 255);
    }
}
