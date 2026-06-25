using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorTheme
    {
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
    }
}
