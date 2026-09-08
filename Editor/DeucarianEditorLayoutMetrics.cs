using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>
    /// Canonical spacing metrics shared by UI Toolkit and IMGUI editor surfaces.
    /// Keep the matching USS declarations in DeucarianEditor.uss synchronized.
    /// </summary>
    public static class DeucarianEditorLayoutMetrics
    {
        public const int PageHorizontalPadding = 10;
        public const int PageVerticalPadding = 8;
        public const int PageTopPadding = PageVerticalPadding;
        public const int PageBottomPadding = PageVerticalPadding;
        public const int SurfaceHorizontalPadding = 10;
        public const int SurfaceVerticalPadding = 8;
        public const int SurfaceSpacing = 8;
        public const int FooterHorizontalPadding = 10;
        public const int FooterVerticalPadding = 0;
        public const int CommandControlHeight = 28;
        public const int CommandControlHorizontalPadding = 8;
        public const int TextLineHeight = 18;
        public const int CommandBarSingleRowHeight = 46;
        public const int CommandBarTwoRowHeight = 78;
        public const int FooterHeight = 34;
        public const int ControlHeight = CommandControlHeight;
        public const int CommandBarHeight = CommandBarSingleRowHeight;
        public const int CommandBarStackedHeight = CommandBarTwoRowHeight;
        public const int IconTextHorizontalPadding = CommandControlHorizontalPadding;
        public const int IconTextVerticalPadding = 0;
        public const int IconTextGap = 8;
        public const int IconSize = 14;
        public const int PackageHeaderHorizontalPadding = SurfaceHorizontalPadding;
        public const int PackageHeaderVerticalPadding = SurfaceVerticalPadding;
        public const int PackageHeaderIconSize = 24;
        public const int PackageHeaderIconTextGap = 10;
        public const int PackageHeaderBottomMargin = 8;
    }
}
