using UnityEngine;

namespace Deucarian.Editor
{
    public enum DeucarianEditorTerritory
    {
        Grove,
        Cobalt,
        Tideline,
        Oxblood,
        Mineral
    }

    public enum DeucarianEditorGraphStatus
    {
        Installed,
        Available,
        Update,
        Warning,
        Missing,
        Checking,
        Unknown
    }

    /// <summary>
    /// Brand and semantic color roles shared by every Deucarian editor tool.
    /// Consumers should ask for a role instead of embedding product-specific colors.
    /// </summary>
    public static class DeucarianEditorPalette
    {
        public static Color Grove => DeucarianEditorTheme.IsDark ? Rgb(143, 169, 145) : Rgb(53, 85, 69);
        public static Color Cobalt => DeucarianEditorTheme.IsDark ? Rgb(121, 149, 213) : Rgb(35, 75, 145);
        public static Color Tideline => DeucarianEditorTheme.IsDark ? DeucarianEditorColors.SeaGlass : DeucarianEditorColors.DeepTeal;
        public static Color Oxblood => DeucarianEditorTheme.IsDark ? Rgb(211, 122, 114) : Rgb(141, 45, 43);
        public static Color Mineral => DeucarianEditorTheme.IsDark ? Rgb(170, 166, 158) : Rgb(93, 90, 85);
        public static Color Brass => DeucarianEditorTheme.IsDark ? Rgb(209, 163, 90) : Rgb(138, 100, 45);
        public static Color Ember => DeucarianEditorTheme.IsDark ? Rgb(214, 136, 85) : Rgb(164, 75, 49);

        public static Color ResolveTerritory(DeucarianEditorTerritory territory)
        {
            switch (territory)
            {
                case DeucarianEditorTerritory.Grove:
                    return Grove;
                case DeucarianEditorTerritory.Cobalt:
                    return Cobalt;
                case DeucarianEditorTerritory.Oxblood:
                    return Oxblood;
                case DeucarianEditorTerritory.Mineral:
                    return Mineral;
                default:
                    return Tideline;
            }
        }

        internal static Color Rgb(byte red, byte green, byte blue)
        {
            return new Color32(red, green, blue, 255);
        }
    }

    /// <summary>
    /// Complete semantic palette for graph-like editor experiences.
    /// Layout and domain state stay with the consuming package; visual meaning lives here.
    /// </summary>
    public static class DeucarianEditorGraphTheme
    {
        public static Color Canvas => DeucarianEditorTheme.IsDark
            ? DeucarianEditorPalette.Rgb(25, 25, 23)
            : DeucarianEditorPalette.Rgb(244, 242, 235);

        public static Color CanvasInset => DeucarianEditorTheme.IsDark
            ? DeucarianEditorPalette.Rgb(20, 22, 21)
            : DeucarianEditorPalette.Rgb(237, 234, 225);

        public static Color Surface => DeucarianEditorTheme.IsDark
            ? DeucarianEditorPalette.Rgb(43, 42, 39)
            : DeucarianEditorPalette.Rgb(255, 255, 255);

        public static Color SurfaceRaised => DeucarianEditorTheme.IsDark
            ? DeucarianEditorPalette.Rgb(52, 51, 47)
            : DeucarianEditorPalette.Rgb(250, 248, 242);

        public static Color SurfaceHover => DeucarianEditorTheme.IsDark
            ? DeucarianEditorPalette.Rgb(48, 61, 58)
            : DeucarianEditorPalette.Rgb(235, 247, 245);

        public static Color SurfaceSelected => DeucarianEditorTheme.IsDark
            ? DeucarianEditorPalette.Rgb(43, 70, 68)
            : DeucarianEditorPalette.Rgb(218, 239, 236);

        public static Color SurfaceRelated => DeucarianEditorTheme.IsDark
            ? DeucarianEditorPalette.Rgb(45, 55, 52)
            : DeucarianEditorPalette.Rgb(233, 244, 242);

        public static Color Border => DeucarianEditorTheme.IsDark
            ? WithAlpha(DeucarianEditorPalette.Mineral, 0.28f)
            : WithAlpha(DeucarianEditorColors.MineralInk, 0.16f);

        public static Color BorderHover => WithAlpha(DeucarianEditorPalette.Tideline, DeucarianEditorTheme.IsDark ? 0.62f : 0.48f);
        public static Color BorderSelected => WithAlpha(DeucarianEditorPalette.Tideline, DeucarianEditorTheme.IsDark ? 0.92f : 0.76f);
        public static Color Edge => WithAlpha(DeucarianEditorPalette.Mineral, DeucarianEditorTheme.IsDark ? 0.42f : 0.34f);
        public static Color EdgeEmphasis => WithAlpha(DeucarianEditorPalette.Tideline, DeucarianEditorTheme.IsDark ? 0.84f : 0.72f);
        public static Color EdgeUnderlay => DeucarianEditorTheme.IsDark
            ? WithAlpha(DeucarianEditorColors.MineralInk, 0.78f)
            : WithAlpha(DeucarianEditorPalette.Mineral, 0.20f);

        public static Color Installed => DeucarianEditorPalette.Tideline;
        public static Color Available => DeucarianEditorPalette.Mineral;
        public static Color Update => DeucarianEditorPalette.Brass;
        public static Color Warning => DeucarianEditorPalette.Ember;
        public static Color Missing => DeucarianEditorPalette.Oxblood;
        public static Color Checking => DeucarianEditorPalette.Cobalt;
        public static Color Unknown => WithAlpha(DeucarianEditorPalette.Mineral, 0.78f);
        public static Color Success => DeucarianEditorPalette.Grove;

        public static Color OrbitFill => WithAlpha(DeucarianEditorPalette.Tideline, DeucarianEditorTheme.IsDark ? 0.18f : 0.10f);
        public static Color OrbitStroke => WithAlpha(DeucarianEditorPalette.Tideline, DeucarianEditorTheme.IsDark ? 0.50f : 0.38f);
        public static Color WarningMarkerFill => Update;
        public static Color WarningMarkerStroke => DeucarianEditorTheme.IsDark
            ? WithAlpha(DeucarianEditorColors.MineralInk, 0.90f)
            : WithAlpha(DeucarianEditorColors.MineralInk, 0.72f);

        public static Color ResolveStatus(DeucarianEditorGraphStatus status)
        {
            switch (status)
            {
                case DeucarianEditorGraphStatus.Installed:
                    return Installed;
                case DeucarianEditorGraphStatus.Available:
                    return Available;
                case DeucarianEditorGraphStatus.Update:
                    return Update;
                case DeucarianEditorGraphStatus.Warning:
                    return Warning;
                case DeucarianEditorGraphStatus.Missing:
                    return Missing;
                case DeucarianEditorGraphStatus.Checking:
                    return Checking;
                default:
                    return Unknown;
            }
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
