using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorWorkbenchOptions
    {
        public DeucarianEditorWorkbenchOptions()
        {
            IncludeToolbar = true;
            ToolbarLayout = DeucarianEditorWorkbenchToolbarLayout.Responsive;
            DrawerMode = DeucarianEditorWorkbenchDrawerMode.Inline;
        }

        public bool IncludeToolbar { get; set; }
        public bool IncludeHeader { get; set; }
        public bool IncludeDrawer { get; set; }
        public bool IncludeFooter { get; set; }
        public string HeaderPackageKey { get; set; }
        public string HeaderTitle { get; set; }
        public string HeaderSubtitle { get; set; }
        public DeucarianEditorWorkbenchToolbarLayout ToolbarLayout { get; set; }
        public DeucarianEditorWorkbenchDrawerMode DrawerMode { get; set; }
        public Texture2D Background { get; set; }
        public string TopSafeFadeName { get; set; }
    }
}
