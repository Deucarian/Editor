using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>
    /// Additive IMGUI primitives for hybrid editor workbenches. The metrics mirror
    /// the established Package Installer surfaces without changing legacy helpers.
    /// </summary>
    public static class DeucarianEditorWorkbenchGUI
    {
        public const float DetailLabelWidth = 118f;
        public const float ButtonHeight = DeucarianEditorLayoutMetrics.CommandControlHeight;
        public const float PanelSpacing = DeucarianEditorLayoutMetrics.SurfaceSpacing;
        public const float StatusRowHeight = 20f;
        public const float StatusMarkerSize = 18f;
        public const float StatusMarkerGap = 4f;
        public const float CompactIconActionHeight = DeucarianEditorLayoutMetrics.CommandControlHeight;
        public const float CompactIconSize = DeucarianEditorIconTextButton.IconSize;
        public const float CompactIconTextGap = DeucarianEditorIconTextButton.IconTextGap;

        private static readonly DeucarianEditorWorkbenchStyleCache styles = new DeucarianEditorWorkbenchStyleCache();

        public static Color MainBackgroundColor => DeucarianEditorVisualShell.DeepBackground;
        public static Color SidebarBackgroundColor => DeucarianEditorVisualShell.MainPanel;
        public static Color DetailsBackgroundColor => DeucarianEditorVisualShell.MainPanel;
        public static Color PanelBackgroundColor => DeucarianEditorVisualShell.NestedSurface;
        public static Color HeaderPanelBackgroundColor => DeucarianEditorVisualShell.HeaderPanel;
        public static Color SampleRowBackgroundColor => DeucarianEditorVisualShell.NestedSurface;
        public static Color PanelBorderColor => DeucarianEditorVisualShell.Border;
        public static Color InteractiveBorderColor => DeucarianEditorVisualShell.InteractiveBorder;
        public static Color SeparatorColor => DeucarianEditorVisualShell.SubtleBorder;
        public static Color TextColor => DeucarianEditorVisualShell.Text;
        public static Color MutedTextColor => DeucarianEditorVisualShell.MutedText;
        public static Color InteractiveTextColor => TextColor;
        public static Color RowBackgroundColor => DeucarianEditorTheme.IsDark
            ? new Color(48f / 255f, 46f / 255f, 42f / 255f, 0.46f)
            : new Color(242f / 255f, 239f / 255f, 231f / 255f, 0.56f);
        public static Color RowHoverColor => DeucarianEditorTheme.IsDark
            ? new Color(62f / 255f, 65f / 255f, 60f / 255f, 0.66f)
            : new Color(98f / 255f, 186f / 255f, 182f / 255f, 0.16f);
        public static Color RowSelectedColor => DeucarianEditorTheme.IsDark
            ? new Color(15f / 255f, 98f / 255f, 106f / 255f, 0.52f)
            : new Color(98f / 255f, 186f / 255f, 182f / 255f, 0.25f);

        public static GUIStyle WindowStyle => styles.Current.WindowStyle;
        public static GUIStyle EmbeddedPageStyle => styles.Current.EmbeddedPageStyle;
        public static GUIStyle SidebarStyle => styles.Current.SidebarStyle;
        public static GUIStyle DetailsStyle => styles.Current.DetailsStyle;
        public static GUIStyle SampleRowStyle => styles.Current.SampleRowStyle;
        public static GUIStyle TitleStyle => styles.Current.TitleStyle;
        public static GUIStyle SubtitleStyle => styles.Current.SubtitleStyle;
        public static GUIStyle SectionTitleStyle => styles.Current.SectionTitleStyle;
        public static GUIStyle LabelStyle => styles.Current.LabelStyle;
        public static GUIStyle BoldLabelStyle => styles.Current.BoldLabelStyle;
        public static GUIStyle WordWrappedMiniLabelStyle => styles.Current.WordWrappedMiniLabelStyle;
        public static GUIStyle MiniLabelStyle => styles.Current.MiniLabelStyle;
        public static GUIStyle MutedMiniLabelStyle => styles.Current.MutedMiniLabelStyle;
        public static GUIStyle RowTitleStyle => styles.Current.RowTitleStyle;
        public static GUIStyle RowSubLabelStyle => styles.Current.RowSubLabelStyle;
        public static GUIStyle RowStatusStyle => styles.Current.RowStatusStyle;
        public static GUIStyle MarkerStyle => styles.Current.MarkerStyle;
        public static GUIStyle FoldoutStyle => styles.Current.FoldoutStyle;
        public static GUIStyle PrimaryButtonStyle => styles.Current.PrimaryButtonStyle;
        public static GUIStyle SecondaryButtonStyle => styles.Current.SecondaryButtonStyle;

        public static void ClearCache()
        {
            styles.Clear();
        }

        public static void DrawPanel(string title, Action content, params GUILayoutOption[] options)
        {
            using (BeginPanel(title, options))
            {
                content?.Invoke();
            }
        }

        public static DeucarianEditorWorkbenchPanelScope BeginPanel(
            string title = null,
            params GUILayoutOption[] options)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                EditorGUILayout.LabelField(title, SectionTitleStyle);
            }

            Rect rect = EditorGUILayout.BeginVertical(DeucarianEditorStyles.SectionBox, options);
            DrawSurface(rect, PanelBackgroundColor, PanelBorderColor);
            // SectionBox owns the one canonical bottom spacing. Adding a second
            // GUILayout.Space here doubles the gap between adjacent surfaces.
            return new DeucarianEditorWorkbenchPanelScope(0f);
        }

        public static DeucarianEditorWorkbenchPanelScope BeginSurface(
            GUIStyle style,
            Color backgroundColor,
            Color borderColor,
            params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.BeginVertical(style ?? GUIStyle.none, options);
            DrawSurface(rect, backgroundColor, borderColor);
            return new DeucarianEditorWorkbenchPanelScope(0f);
        }

        public static void DrawSurface(Rect rect, Color backgroundColor, Color borderColor)
        {
            DeucarianEditorVisualShell.DrawFrostedSurface(rect, backgroundColor, borderColor);
        }

        public static void DrawSeparator()
        {
            Rect rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, SeparatorColor);
            }
        }

        public static void DrawKeyValueRow(string label, string value)
        {
            DrawReadOnlyRow(label, value);
        }

        public static void DrawReadOnlyRow(string label, string value, string tooltip = null)
        {
            string displayValue = string.IsNullOrWhiteSpace(value) ? "-" : value;
            using (new EditorGUILayout.HorizontalScope())
            {
                string safeTooltip = string.IsNullOrWhiteSpace(tooltip) ? displayValue : tooltip;
                var labelContent = new GUIContent(label ?? string.Empty, safeTooltip);
                var valueContent = new GUIContent(displayValue, safeTooltip);
                EditorGUILayout.LabelField(labelContent, MutedMiniLabelStyle, GUILayout.Width(DetailLabelWidth));
                EditorGUILayout.LabelField(valueContent, MiniLabelStyle, GUILayout.ExpandWidth(true));
            }
        }

        public static DeucarianEditorWorkbenchPanelScope BeginSettingsPage(params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.BeginVertical(WindowStyle, options);
            DeucarianEditorVisualShell.DrawWindowBackground(rect);
            return new DeucarianEditorWorkbenchPanelScope(0f);
        }

        /// <summary>
        /// Begins IMGUI content embedded inside a workbench shell. The shell already
        /// owns page padding and wallpaper, so this scope deliberately adds no inset.
        /// </summary>
        public static DeucarianEditorWorkbenchPanelScope BeginEmbeddedPage(
            params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(EmbeddedPageStyle, options);
            return new DeucarianEditorWorkbenchPanelScope(0f);
        }

        public static Rect DrawLabeledField(
            string label,
            string tooltip = null,
            float labelWidth = 140f,
            float height = DeucarianEditorLayoutMetrics.TextLineHeight)
        {
            Rect row = EditorGUILayout.GetControlRect(false, height);
            float safeLabelWidth = Mathf.Clamp(labelWidth, 0f, row.width);
            Rect labelRect = new Rect(row.x, row.y, safeLabelWidth, row.height);
            Rect controlRect = new Rect(
                labelRect.xMax + 6f,
                row.y,
                Mathf.Max(0f, row.width - safeLabelWidth - 6f),
                row.height);
            DrawColoredLabel(
                labelRect,
                new GUIContent(label ?? string.Empty, tooltip ?? string.Empty),
                LabelStyle,
                TextColor);
            return controlRect;
        }

        public static bool DrawCompactIconAction(
            string iconId,
            string text,
            string tooltip,
            bool enabled = true,
            params GUILayoutOption[] options)
        {
            return DrawCompactIconAction(
                iconId,
                text,
                tooltip,
                enabled,
                false,
                options);
        }

        public static bool DrawCompactIconAction(
            string iconId,
            string text,
            string tooltip,
            bool enabled,
            bool primary,
            params GUILayoutOption[] options)
        {
            Rect row = GUILayoutUtility.GetRect(
                1f,
                CompactIconActionHeight,
                options == null || options.Length == 0
                    ? new[] { GUILayout.ExpandWidth(true) }
                    : options);
            using (new EditorGUI.DisabledScope(!enabled))
            {
                bool clicked = GUI.Button(
                    row,
                    new GUIContent(string.Empty, tooltip ?? text ?? string.Empty),
                    primary ? PrimaryButtonStyle : SecondaryButtonStyle);
                DeucarianEditorIconTextButton.CalculateImGuiContentRects(
                    row,
                    out Rect iconRect,
                    out Rect textRect);
                Color interactiveText = InteractiveTextColor;
                Color tint = enabled
                    ? interactiveText
                    : DeucarianEditorColors.WithAlpha(interactiveText, 0.58f);
                DeucarianEditorIcons.DrawIcon(iconRect, DeucarianEditorIcons.GetIcon(iconId), tint);
                DrawColoredLabel(
                    textRect,
                    new GUIContent(text ?? string.Empty, tooltip ?? string.Empty),
                    styles.Current.CompactIconActionLabelStyle,
                    tint);
                return clicked;
            }
        }

        public static void DrawStatusRow(
            string marker,
            string text,
            DeucarianEditorStatus status = DeucarianEditorStatus.Info)
        {
            string safeText = text ?? string.Empty;
            GetStatusRowRects(out Rect markerRect, out Rect labelRect);

            DrawColoredLabel(markerRect, new GUIContent(marker ?? string.Empty, safeText), MarkerStyle, DeucarianEditorStatusBadge.GetColor(status));
            DrawColoredLabel(labelRect, new GUIContent(safeText, safeText), MiniLabelStyle, TextColor);
        }

        /// <summary>Draws a status row with a tintable package-owned Lucide icon.</summary>
        public static void DrawStatusIconRow(
            string iconId,
            string text,
            DeucarianEditorStatus status = DeucarianEditorStatus.Info)
        {
            string safeText = text ?? string.Empty;
            GetStatusRowRects(out Rect iconRect, out Rect labelRect);
            DeucarianEditorIcons.DrawIcon(
                iconRect,
                DeucarianEditorIcons.GetIcon(iconId),
                DeucarianEditorStatusBadge.GetColor(status));
            DrawColoredLabel(labelRect, new GUIContent(safeText, safeText), MiniLabelStyle, TextColor);
        }

        internal static void EndPanel(float trailingSpace)
        {
            EditorGUILayout.EndVertical();
            if (trailingSpace > 0f)
            {
                GUILayout.Space(trailingSpace);
            }
        }

        private static void DrawColoredLabel(Rect rect, GUIContent content, GUIStyle style, Color color)
        {
            Color previousColor = GUI.contentColor;
            try
            {
                // Preserve the released Package Installer composition: Unity multiplies
                // GUI.contentColor with the style state's text color at draw time.
                GUI.contentColor = color;
                GUI.Label(rect, content, style);
            }
            finally
            {
                GUI.contentColor = previousColor;
            }
        }

        private static void GetStatusRowRects(out Rect markerRect, out Rect labelRect)
        {
            Rect rowRect = GUILayoutUtility.GetRect(1f, StatusRowHeight, GUILayout.ExpandWidth(true));
            markerRect = new Rect(rowRect.x, rowRect.y + 1f, StatusMarkerSize, StatusMarkerSize);
            labelRect = new Rect(
                markerRect.xMax + StatusMarkerGap,
                rowRect.y,
                Mathf.Max(0f, rowRect.width - StatusMarkerSize - StatusMarkerGap),
                StatusRowHeight);
        }

    }


}
