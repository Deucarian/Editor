using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorTheme
    {
        public const string DarkClass = "deucarian-theme--dark";
        public const string LightClass = "deucarian-theme--light";

        public static bool IsDark => DeucarianEditorColors.IsDark;
        public static string CurrentClass => IsDark ? DarkClass : LightClass;
        public static Color WindowBackground => DeucarianEditorVisualShell.DeepBackground;
        public static Color GlassPanel => DeucarianEditorVisualShell.MainPanel;
        public static Color GlassPanelStrong => DeucarianEditorVisualShell.HeaderPanel;
        public static Color GlassPanelSoft => DeucarianEditorVisualShell.NestedSurface;
        public static Color Border => DeucarianEditorVisualShell.Border;
        public static Color BorderSubtle => DeucarianEditorVisualShell.SubtleBorder;
        public static Color Accent => DeucarianEditorVisualShell.InteractiveBorder;
        public static Color Text => DeucarianEditorVisualShell.Text;
        public static Color MutedText => DeucarianEditorVisualShell.MutedText;

        public static Color Warning => DeucarianEditorColors.WithAlpha(DeucarianEditorColors.GetStatusColor(DeucarianEditorStatus.Warning), 0.78f);
        public static Color Error => DeucarianEditorColors.WithAlpha(DeucarianEditorColors.GetStatusColor(DeucarianEditorStatus.Error), 0.82f);
        public static Color Success => DeucarianEditorColors.WithAlpha(DeucarianEditorColors.GetStatusColor(DeucarianEditorStatus.Success), 0.82f);

        public static Color Grove => DeucarianEditorPalette.Grove;
        public static Color Cobalt => DeucarianEditorPalette.Cobalt;
        public static Color Tideline => DeucarianEditorPalette.Tideline;
        public static Color Oxblood => DeucarianEditorPalette.Oxblood;
        public static Color Mineral => DeucarianEditorPalette.Mineral;
    }
}
