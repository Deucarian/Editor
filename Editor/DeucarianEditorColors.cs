using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorColors
    {
        public static bool IsDark
        {
            get { return EditorGUIUtility.isProSkin; }
        }

        public static Color Salt
        {
            get { return FromRgb(242, 239, 231); }
        }

        public static Color SeaGlass
        {
            get { return FromRgb(98, 186, 182); }
        }

        public static Color DeepTeal
        {
            get { return FromRgb(15, 98, 106); }
        }

        public static Color MineralInk
        {
            get { return FromRgb(27, 26, 24); }
        }

        public static Color Teal
        {
            get { return SeaGlass; }
        }

        public static Color Blue
        {
            get { return DeepTeal; }
        }

        public static Color Slate
        {
            get { return IsDark ? FromRgb(86, 84, 79) : FromRgb(103, 100, 94); }
        }

        public static Color Silver
        {
            get { return IsDark ? FromRgb(188, 184, 176) : FromRgb(121, 118, 111); }
        }

        public static Color HeaderBackground => DeucarianEditorSurfacePalette.Field;

        public static Color SectionBackground => DeucarianEditorSurfacePalette.Field;

        public static Color Border => DeucarianEditorSurfacePalette.Border;

        public static Color TitleText => DeucarianEditorSurfacePalette.Text;

        public static Color BodyText => DeucarianEditorSurfacePalette.Text;

        public static Color MutedText => DeucarianEditorSurfacePalette.Muted;

        public static Color BadgeText
        {
            get { return Salt; }
        }

        public static Color GetStatusColor(DeucarianEditorStatus status)
        {
            switch (status)
            {
                case DeucarianEditorStatus.Success:
                    return FromRgb(71, 137, 104);
                case DeucarianEditorStatus.Warning:
                    return FromRgb(170, 128, 57);
                case DeucarianEditorStatus.Error:
                    return FromRgb(163, 82, 82);
                case DeucarianEditorStatus.Disabled:
                    return EditorGUIUtility.isProSkin ? FromRgb(90, 96, 101) : FromRgb(145, 153, 159);
                default:
                    return Blue;
            }
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static Color FromRgb(byte red, byte green, byte blue)
        {
            return new Color32(red, green, blue, 255);
        }
    }
}
