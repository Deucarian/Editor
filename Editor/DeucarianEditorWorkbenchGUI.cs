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
        public const float ButtonHeight = 24f;
        public const float PanelSpacing = 8f;
        public const float StatusRowHeight = 20f;
        public const float StatusMarkerSize = 18f;
        public const float StatusMarkerGap = 4f;
        public const float CompactIconActionHeight = 28f;
        public const float CompactIconSize = DeucarianEditorIconTextButton.IconSize;
        public const float CompactIconTextGap = DeucarianEditorIconTextButton.IconTextGap;

        private static bool initialized;
        private static bool lastProSkin;
        private static GUIStyle windowStyle;
        private static GUIStyle sidebarStyle;
        private static GUIStyle detailsStyle;
        private static GUIStyle sampleRowStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle subtitleStyle;
        private static GUIStyle sectionTitleStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle boldLabelStyle;
        private static GUIStyle wordWrappedMiniLabelStyle;
        private static GUIStyle miniLabelStyle;
        private static GUIStyle mutedMiniLabelStyle;
        private static GUIStyle rowTitleStyle;
        private static GUIStyle rowSubLabelStyle;
        private static GUIStyle rowStatusStyle;
        private static GUIStyle markerStyle;
        private static GUIStyle foldoutStyle;
        private static GUIStyle primaryButtonStyle;
        private static GUIStyle secondaryButtonStyle;
        private static GUIStyle compactIconActionLabelStyle;

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
        public static Color InteractiveTextColor => EditorGUIUtility.isProSkin
            ? TextColor
            : new Color(31f / 255f, 43f / 255f, 50f / 255f, 1f);
        public static Color RowBackgroundColor => new Color(32f / 255f, 47f / 255f, 56f / 255f, 0.46f);
        public static Color RowHoverColor => new Color(32f / 255f, 47f / 255f, 56f / 255f, 0.62f);
        public static Color RowSelectedColor => new Color(35f / 255f, 62f / 255f, 66f / 255f, 0.58f);

        public static GUIStyle WindowStyle { get { EnsureStyles(); return windowStyle; } }
        public static GUIStyle SidebarStyle { get { EnsureStyles(); return sidebarStyle; } }
        public static GUIStyle DetailsStyle { get { EnsureStyles(); return detailsStyle; } }
        public static GUIStyle SampleRowStyle { get { EnsureStyles(); return sampleRowStyle; } }
        public static GUIStyle TitleStyle { get { EnsureStyles(); return titleStyle; } }
        public static GUIStyle SubtitleStyle { get { EnsureStyles(); return subtitleStyle; } }
        public static GUIStyle SectionTitleStyle { get { EnsureStyles(); return sectionTitleStyle; } }
        public static GUIStyle LabelStyle { get { EnsureStyles(); return labelStyle; } }
        public static GUIStyle BoldLabelStyle { get { EnsureStyles(); return boldLabelStyle; } }
        public static GUIStyle WordWrappedMiniLabelStyle { get { EnsureStyles(); return wordWrappedMiniLabelStyle; } }
        public static GUIStyle MiniLabelStyle { get { EnsureStyles(); return miniLabelStyle; } }
        public static GUIStyle MutedMiniLabelStyle { get { EnsureStyles(); return mutedMiniLabelStyle; } }
        public static GUIStyle RowTitleStyle { get { EnsureStyles(); return rowTitleStyle; } }
        public static GUIStyle RowSubLabelStyle { get { EnsureStyles(); return rowSubLabelStyle; } }
        public static GUIStyle RowStatusStyle { get { EnsureStyles(); return rowStatusStyle; } }
        public static GUIStyle MarkerStyle { get { EnsureStyles(); return markerStyle; } }
        public static GUIStyle FoldoutStyle { get { EnsureStyles(); return foldoutStyle; } }
        public static GUIStyle PrimaryButtonStyle { get { EnsureStyles(); return primaryButtonStyle; } }
        public static GUIStyle SecondaryButtonStyle { get { EnsureStyles(); return secondaryButtonStyle; } }

        public static void ClearCache()
        {
            initialized = false;
            windowStyle = null;
            sidebarStyle = null;
            detailsStyle = null;
            sampleRowStyle = null;
            titleStyle = null;
            subtitleStyle = null;
            sectionTitleStyle = null;
            labelStyle = null;
            boldLabelStyle = null;
            wordWrappedMiniLabelStyle = null;
            miniLabelStyle = null;
            mutedMiniLabelStyle = null;
            rowTitleStyle = null;
            rowSubLabelStyle = null;
            rowStatusStyle = null;
            markerStyle = null;
            foldoutStyle = null;
            primaryButtonStyle = null;
            secondaryButtonStyle = null;
            compactIconActionLabelStyle = null;
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
            return new DeucarianEditorWorkbenchPanelScope(PanelSpacing);
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

        public static Rect DrawLabeledField(
            string label,
            string tooltip = null,
            float labelWidth = 140f,
            float height = 18f)
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
                    compactIconActionLabelStyle,
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
            Rect rowRect = GUILayoutUtility.GetRect(1f, StatusRowHeight, GUILayout.ExpandWidth(true));
            Rect markerRect = new Rect(rowRect.x, rowRect.y + 1f, StatusMarkerSize, StatusMarkerSize);
            Rect labelRect = new Rect(
                markerRect.xMax + StatusMarkerGap,
                rowRect.y,
                Mathf.Max(0f, rowRect.width - StatusMarkerSize - StatusMarkerGap),
                StatusRowHeight);

            DrawColoredLabel(markerRect, new GUIContent(marker ?? string.Empty, safeText), MarkerStyle, DeucarianEditorStatusBadge.GetColor(status));
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

        private static void EnsureStyles()
        {
            bool proSkin = EditorGUIUtility.isProSkin;
            if (initialized && lastProSkin == proSkin && windowStyle != null)
            {
                return;
            }

            initialized = true;
            lastProSkin = proSkin;

            windowStyle = new GUIStyle
            {
                padding = new RectOffset(
                    DeucarianEditorLayoutMetrics.PageHorizontalPadding,
                    DeucarianEditorLayoutMetrics.PageHorizontalPadding,
                    DeucarianEditorLayoutMetrics.PageTopPadding,
                    DeucarianEditorLayoutMetrics.PageBottomPadding)
            };
            sidebarStyle = new GUIStyle { padding = new RectOffset(10, 10, 10, 10) };
            detailsStyle = new GUIStyle { padding = new RectOffset(10, 10, 10, 10) };
            sampleRowStyle = new GUIStyle
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 2, 6)
            };

            titleStyle = CopyStyle(() => DeucarianEditorStyles.PackageHeaderTitle);
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = TextColor;
            titleStyle.wordWrap = true;

            subtitleStyle = CopyStyle(() => DeucarianEditorStyles.PackageHeaderSubtitle);
            subtitleStyle.normal.textColor = MutedTextColor;
            sectionTitleStyle = CopyStyle(() => DeucarianEditorStyles.SectionTitle);
            sectionTitleStyle.normal.textColor = TextColor;

            labelStyle = CopyStyle(() => EditorStyles.label);
            labelStyle.normal.textColor = TextColor;

            boldLabelStyle = CopyStyle(() => EditorStyles.boldLabel);
            boldLabelStyle.normal.textColor = TextColor;

            wordWrappedMiniLabelStyle = CopyStyle(() => EditorStyles.wordWrappedMiniLabel);
            wordWrappedMiniLabelStyle.normal.textColor = MutedTextColor;
            wordWrappedMiniLabelStyle.wordWrap = true;

            miniLabelStyle = CopyStyle(() => EditorStyles.wordWrappedMiniLabel);
            miniLabelStyle.normal.textColor = TextColor;
            miniLabelStyle.wordWrap = true;
            miniLabelStyle.clipping = TextClipping.Overflow;

            mutedMiniLabelStyle = CopyStyle(() => DeucarianEditorStyles.MutedLabel);
            mutedMiniLabelStyle.fontSize = GetWordWrappedMiniLabelFontSize();
            mutedMiniLabelStyle.wordWrap = true;
            mutedMiniLabelStyle.clipping = TextClipping.Overflow;

            rowTitleStyle = CopyStyle(() => EditorStyles.miniBoldLabel);
            rowTitleStyle.normal.textColor = TextColor;
            rowTitleStyle.wordWrap = true;
            rowTitleStyle.clipping = TextClipping.Clip;

            rowSubLabelStyle = CopyStyle(() => EditorStyles.wordWrappedMiniLabel);
            rowSubLabelStyle.normal.textColor = MutedTextColor;
            rowSubLabelStyle.wordWrap = true;
            rowSubLabelStyle.clipping = TextClipping.Clip;

            rowStatusStyle = CopyStyle(() => EditorStyles.miniLabel);
            rowStatusStyle.normal.textColor = TextColor;
            rowStatusStyle.alignment = TextAnchor.MiddleLeft;
            rowStatusStyle.clipping = TextClipping.Clip;

            markerStyle = CopyStyle(() => EditorStyles.miniBoldLabel);
            markerStyle.alignment = TextAnchor.MiddleCenter;
            markerStyle.fontSize = 10;
            markerStyle.normal.textColor = TextColor;

            foldoutStyle = CopyStyle(() => EditorStyles.foldout);
            foldoutStyle.normal.textColor = TextColor;
            foldoutStyle.onNormal.textColor = TextColor;
            foldoutStyle.hover.textColor = TextColor;
            foldoutStyle.onHover.textColor = TextColor;
            foldoutStyle.fontStyle = FontStyle.Bold;

            primaryButtonStyle = CopyStyle(() => EditorStyles.miniButton);
            primaryButtonStyle.fontStyle = FontStyle.Bold;
            primaryButtonStyle.fixedHeight = ButtonHeight;

            secondaryButtonStyle = CopyStyle(() => DeucarianEditorStyles.ToolbarButton);
            secondaryButtonStyle.fixedHeight = ButtonHeight;

            compactIconActionLabelStyle = CopyStyle(() => EditorStyles.label);
            compactIconActionLabelStyle.alignment = TextAnchor.MiddleLeft;
            compactIconActionLabelStyle.padding = new RectOffset(0, 0, 0, 0);
            compactIconActionLabelStyle.margin = new RectOffset(0, 0, 0, 0);
            compactIconActionLabelStyle.normal.textColor = TextColor;
        }

        private static GUIStyle CopyStyle(Func<GUIStyle> styleFactory)
        {
            try
            {
                GUIStyle style = styleFactory?.Invoke();
                return style == null ? new GUIStyle() : new GUIStyle(style);
            }
            catch
            {
                return new GUIStyle();
            }
        }

        private static int GetWordWrappedMiniLabelFontSize()
        {
            try
            {
                return EditorStyles.wordWrappedMiniLabel.fontSize;
            }
            catch
            {
                return 9;
            }
        }
    }

    /// <summary>
    /// Shared settings-page actions. Packages provide only their domain callback;
    /// labels, icons, tooltips, geometry, and padding remain Editor-owned.
    /// </summary>
    public static class DeucarianEditorSettingsActions
    {
        public const string ResetToDefaultsLabel = "Reset to Defaults";
        public const string ResetToDefaultsTooltip = "Restore the package defaults.";
        public const string ResetActionClass = "deucarian-settings-action--reset";
        public const float ResetButtonWidth = 164f;
        public const float ResetButtonHeight = DeucarianEditorWorkbenchGUI.CompactIconActionHeight;

        public static Button CreateResetToDefaultsButton(
            Action resetAction,
            string tooltip = null,
            bool enabled = true,
            float width = ResetButtonWidth)
        {
            Button button = DeucarianEditorIconTextButton.Create(
                DeucarianEditorIconIds.Reset,
                ResetToDefaultsLabel,
                resetAction,
                string.IsNullOrWhiteSpace(tooltip) ? ResetToDefaultsTooltip : tooltip,
                true);
            button.AddToClassList(ResetActionClass);
            button.style.width = Mathf.Max(0f, width);
            button.style.minWidth = Mathf.Max(0f, width);
            button.style.height = ResetButtonHeight;
            button.style.minHeight = ResetButtonHeight;
            button.SetEnabled(enabled);
            return button;
        }

        public static bool DrawResetToDefaultsButton(
            Action resetAction,
            string tooltip = null,
            bool enabled = true,
            float width = ResetButtonWidth)
        {
            bool clicked = DeucarianEditorWorkbenchGUI.DrawCompactIconAction(
                DeucarianEditorIconIds.Reset,
                ResetToDefaultsLabel,
                string.IsNullOrWhiteSpace(tooltip) ? ResetToDefaultsTooltip : tooltip,
                enabled,
                GUILayout.Width(Mathf.Max(0f, width)));
            if (clicked)
            {
                resetAction?.Invoke();
            }

            return clicked;
        }
    }

    public sealed class DeucarianEditorWorkbenchPanelScope : IDisposable
    {
        private readonly float trailingSpace;
        private bool disposed;

        internal DeucarianEditorWorkbenchPanelScope(float trailingSpace)
        {
            this.trailingSpace = trailingSpace;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            DeucarianEditorWorkbenchGUI.EndPanel(trailingSpace);
        }
    }
}
