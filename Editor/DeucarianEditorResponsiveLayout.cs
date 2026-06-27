using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorResponsiveLayout
    {
        public const float WideBreakpoint = 1180f;
        public const float MediumBreakpoint = 760f;
        public const float PreviewPanelWidth = 390f;
        public const float NarrowSidebarWidth = 176f;

        public static DeucarianEditorResponsiveLayoutState Calculate(float windowWidth, float windowHeight)
        {
            bool wide = windowWidth >= WideBreakpoint;
            bool narrow = windowWidth < MediumBreakpoint;
            float sidebarWidth = narrow ? NarrowSidebarWidth : DeucarianEditorSpacing.SidebarWidth;
            float previewWidth = wide ? PreviewPanelWidth : Mathf.Max(260f, windowWidth - sidebarWidth - DeucarianEditorSpacing.Large);
            float stackedPreviewHeight = Mathf.Clamp(windowHeight * 0.36f, 240f, 360f);
            return new DeucarianEditorResponsiveLayoutState(wide, narrow, sidebarWidth, previewWidth, stackedPreviewHeight);
        }
    }

    public readonly struct DeucarianEditorResponsiveLayoutState
    {
        public DeucarianEditorResponsiveLayoutState(
            bool wide,
            bool narrow,
            float sidebarWidth,
            float previewWidth,
            float stackedPreviewHeight)
        {
            Wide = wide;
            Narrow = narrow;
            SidebarWidth = sidebarWidth;
            PreviewWidth = previewWidth;
            StackedPreviewHeight = stackedPreviewHeight;
        }

        public bool Wide { get; }
        public bool Narrow { get; }
        public bool PreviewStacked => !Wide;
        public float SidebarWidth { get; }
        public float PreviewWidth { get; }
        public float StackedPreviewHeight { get; }
    }
}
