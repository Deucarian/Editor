using System;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorWorkbenchStyles
    {
        private readonly GUIStyle windowStyle;
        private readonly GUIStyle embeddedPageStyle;
        private readonly GUIStyle sidebarStyle;
        private readonly GUIStyle detailsStyle;
        private readonly GUIStyle sampleRowStyle;
        private readonly GUIStyle titleStyle;
        private readonly GUIStyle subtitleStyle;
        private readonly GUIStyle sectionTitleStyle;
        private readonly GUIStyle labelStyle;
        private readonly GUIStyle boldLabelStyle;
        private readonly GUIStyle wordWrappedMiniLabelStyle;
        private readonly GUIStyle miniLabelStyle;
        private readonly GUIStyle mutedMiniLabelStyle;
        private readonly GUIStyle rowTitleStyle;
        private readonly GUIStyle rowSubLabelStyle;
        private readonly GUIStyle rowStatusStyle;
        private readonly GUIStyle markerStyle;
        private readonly GUIStyle foldoutStyle;
        private readonly GUIStyle primaryButtonStyle;
        private readonly GUIStyle secondaryButtonStyle;
        private readonly GUIStyle compactIconActionLabelStyle;

        internal DeucarianEditorWorkbenchStyles()
        {
            windowStyle = new GUIStyle
            {
                padding = new RectOffset(
                    DeucarianEditorLayoutMetrics.PageHorizontalPadding,
                    DeucarianEditorLayoutMetrics.PageHorizontalPadding,
                    DeucarianEditorLayoutMetrics.PageTopPadding,
                    DeucarianEditorLayoutMetrics.PageBottomPadding)
            };
            embeddedPageStyle = new GUIStyle
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            sidebarStyle = new GUIStyle
            {
                padding = new RectOffset(
                    DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceVerticalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceVerticalPadding)
            };
            detailsStyle = new GUIStyle
            {
                padding = new RectOffset(
                    DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceVerticalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceVerticalPadding)
            };
            sampleRowStyle = new GUIStyle
            {
                padding = new RectOffset(
                    DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceVerticalPadding,
                    DeucarianEditorLayoutMetrics.SurfaceVerticalPadding),
                margin = new RectOffset(
                    0,
                    0,
                    0,
                    DeucarianEditorLayoutMetrics.SurfaceSpacing)
            };

            titleStyle = CopyStyle(() => DeucarianEditorStyles.PackageHeaderTitle);
            DeucarianEditorTypography.ApplyDisplay(titleStyle);
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = DeucarianEditorVisualShell.Text;
            titleStyle.wordWrap = true;

            subtitleStyle = CopyStyle(() => DeucarianEditorStyles.PackageHeaderSubtitle);
            DeucarianEditorTypography.ApplyBody(subtitleStyle);
            subtitleStyle.normal.textColor = DeucarianEditorVisualShell.MutedText;
            sectionTitleStyle = CopyStyle(() => DeucarianEditorStyles.SectionTitle);
            DeucarianEditorTypography.ApplyStrong(sectionTitleStyle);
            sectionTitleStyle.normal.textColor = DeucarianEditorVisualShell.Text;

            labelStyle = CopyStyle(() => EditorStyles.label);
            DeucarianEditorTypography.ApplyBody(labelStyle);
            labelStyle.normal.textColor = DeucarianEditorVisualShell.Text;

            boldLabelStyle = CopyStyle(() => EditorStyles.boldLabel);
            DeucarianEditorTypography.ApplyStrong(boldLabelStyle);
            boldLabelStyle.normal.textColor = DeucarianEditorVisualShell.Text;

            wordWrappedMiniLabelStyle = CopyStyle(() => EditorStyles.wordWrappedMiniLabel);
            DeucarianEditorTypography.ApplyBody(wordWrappedMiniLabelStyle);
            wordWrappedMiniLabelStyle.normal.textColor = DeucarianEditorVisualShell.MutedText;
            wordWrappedMiniLabelStyle.wordWrap = true;

            miniLabelStyle = CopyStyle(() => EditorStyles.wordWrappedMiniLabel);
            DeucarianEditorTypography.ApplyBody(miniLabelStyle);
            miniLabelStyle.normal.textColor = DeucarianEditorVisualShell.Text;
            miniLabelStyle.wordWrap = true;
            miniLabelStyle.clipping = TextClipping.Overflow;

            mutedMiniLabelStyle = CopyStyle(() => DeucarianEditorStyles.MutedLabel);
            DeucarianEditorTypography.ApplyBody(mutedMiniLabelStyle);
            mutedMiniLabelStyle.fontSize = GetWordWrappedMiniLabelFontSize();
            mutedMiniLabelStyle.wordWrap = true;
            mutedMiniLabelStyle.clipping = TextClipping.Overflow;

            rowTitleStyle = CopyStyle(() => EditorStyles.miniBoldLabel);
            DeucarianEditorTypography.ApplyStrong(rowTitleStyle);
            rowTitleStyle.normal.textColor = DeucarianEditorVisualShell.Text;
            rowTitleStyle.wordWrap = true;
            rowTitleStyle.clipping = TextClipping.Clip;

            rowSubLabelStyle = CopyStyle(() => EditorStyles.wordWrappedMiniLabel);
            DeucarianEditorTypography.ApplyBody(rowSubLabelStyle);
            rowSubLabelStyle.normal.textColor = DeucarianEditorVisualShell.MutedText;
            rowSubLabelStyle.wordWrap = true;
            rowSubLabelStyle.clipping = TextClipping.Clip;

            rowStatusStyle = CopyStyle(() => EditorStyles.miniLabel);
            DeucarianEditorTypography.ApplyBody(rowStatusStyle);
            rowStatusStyle.normal.textColor = DeucarianEditorVisualShell.Text;
            rowStatusStyle.alignment = TextAnchor.MiddleLeft;
            rowStatusStyle.clipping = TextClipping.Clip;

            markerStyle = CopyStyle(() => EditorStyles.miniBoldLabel);
            DeucarianEditorTypography.ApplyStrong(markerStyle);
            markerStyle.alignment = TextAnchor.MiddleCenter;
            markerStyle.fontSize = 10;
            markerStyle.normal.textColor = DeucarianEditorVisualShell.Text;

            foldoutStyle = CopyStyle(() => EditorStyles.foldout);
            DeucarianEditorTypography.ApplyStrong(foldoutStyle);
            foldoutStyle.normal.textColor = DeucarianEditorVisualShell.Text;
            foldoutStyle.onNormal.textColor = DeucarianEditorVisualShell.Text;
            foldoutStyle.hover.textColor = DeucarianEditorVisualShell.Text;
            foldoutStyle.onHover.textColor = DeucarianEditorVisualShell.Text;

            primaryButtonStyle = CopyStyle(() => EditorStyles.miniButton);
            DeucarianEditorTypography.ApplyStrong(primaryButtonStyle);
            primaryButtonStyle.fixedHeight = DeucarianEditorLayoutMetrics.CommandControlHeight;

            secondaryButtonStyle = CopyStyle(() => DeucarianEditorStyles.ToolbarButton);
            DeucarianEditorTypography.ApplyStrong(secondaryButtonStyle);
            secondaryButtonStyle.fixedHeight = DeucarianEditorLayoutMetrics.CommandControlHeight;

            compactIconActionLabelStyle = CopyStyle(() => EditorStyles.label);
            DeucarianEditorTypography.ApplyStrong(compactIconActionLabelStyle);
            compactIconActionLabelStyle.alignment = TextAnchor.MiddleLeft;
            compactIconActionLabelStyle.padding = new RectOffset(0, 0, 0, 0);
            compactIconActionLabelStyle.margin = new RectOffset(0, 0, 0, 0);
            compactIconActionLabelStyle.normal.textColor = DeucarianEditorVisualShell.Text;
        }

        internal GUIStyle WindowStyle => windowStyle;
        internal GUIStyle EmbeddedPageStyle => embeddedPageStyle;
        internal GUIStyle SidebarStyle => sidebarStyle;
        internal GUIStyle DetailsStyle => detailsStyle;
        internal GUIStyle SampleRowStyle => sampleRowStyle;
        internal GUIStyle TitleStyle => titleStyle;
        internal GUIStyle SubtitleStyle => subtitleStyle;
        internal GUIStyle SectionTitleStyle => sectionTitleStyle;
        internal GUIStyle LabelStyle => labelStyle;
        internal GUIStyle BoldLabelStyle => boldLabelStyle;
        internal GUIStyle WordWrappedMiniLabelStyle => wordWrappedMiniLabelStyle;
        internal GUIStyle MiniLabelStyle => miniLabelStyle;
        internal GUIStyle MutedMiniLabelStyle => mutedMiniLabelStyle;
        internal GUIStyle RowTitleStyle => rowTitleStyle;
        internal GUIStyle RowSubLabelStyle => rowSubLabelStyle;
        internal GUIStyle RowStatusStyle => rowStatusStyle;
        internal GUIStyle MarkerStyle => markerStyle;
        internal GUIStyle FoldoutStyle => foldoutStyle;
        internal GUIStyle PrimaryButtonStyle => primaryButtonStyle;
        internal GUIStyle SecondaryButtonStyle => secondaryButtonStyle;
        internal GUIStyle CompactIconActionLabelStyle => compactIconActionLabelStyle;

        private static GUIStyle CopyStyle(Func<GUIStyle> styleFactory)
        {
            return DeucarianEditorStyles.CopyStyle(styleFactory);
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
}
