using System;

namespace Deucarian.Editor
{
    public static class DeucarianEditorWorkspaceNavigation
    {
        public const string AudioToolId = "deucarian.theming.audio-palette-lab";

        public static void Populate(DeucarianEditorWorkspace workspace, string selectedTool,
            Action openAudio = null, bool filterNavigation = true)
        {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));
            workspace.BindNavigation(new DeucarianEditorNavigationTree(workspace, selectedTool, filterNavigation));
        }
    }
}
