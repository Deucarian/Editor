using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorColors
    {
        public static Color Teal
        {
            get { return FromRgb(72, 145, 139); }
        }

        public static Color Blue
        {
            get { return FromRgb(76, 121, 165); }
        }

        public static Color Slate
        {
            get { return FromRgb(73, 82, 92); }
        }

        public static Color Silver
        {
            get { return FromRgb(174, 184, 193); }
        }

        public static Color HeaderBackground
        {
            get { return EditorGUIUtility.isProSkin ? FromRgb(38, 47, 53) : FromRgb(227, 232, 235); }
        }

        public static Color SectionBackground
        {
            get { return EditorGUIUtility.isProSkin ? FromRgb(45, 52, 58) : FromRgb(238, 241, 243); }
        }

        public static Color Border
        {
            get { return EditorGUIUtility.isProSkin ? FromRgb(58, 72, 80) : FromRgb(190, 201, 208); }
        }

        public static Color TitleText
        {
            get { return EditorGUIUtility.isProSkin ? FromRgb(232, 237, 240) : FromRgb(31, 43, 50); }
        }

        public static Color BodyText
        {
            get { return EditorGUIUtility.isProSkin ? FromRgb(207, 216, 222) : FromRgb(46, 56, 63); }
        }

        public static Color MutedText
        {
            get { return EditorGUIUtility.isProSkin ? FromRgb(155, 166, 174) : FromRgb(91, 105, 114); }
        }

        public static Color BadgeText
        {
            get { return EditorGUIUtility.isProSkin ? FromRgb(235, 240, 243) : FromRgb(255, 255, 255); }
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
