using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorWorkspaceNavigation
    {
        public const string AudioToolId = "deucarian.theming.audio-palette-lab";

        public static void Populate(DeucarianEditorWorkspace workspace, string selectedTool, Action openAudio = null, bool filterNavigation = true)
        {
            Add(workspace, DeucarianToolIds.ControlCenter, "Overview", DeucarianEditorIconIds.Dashboard);
            Add(workspace, DeucarianToolIds.PackageInstaller, "Packages", DeucarianEditorIconIds.Package);
            Add(workspace, DeucarianToolIds.ThemeManager, "Appearance", DeucarianEditorIconIds.Palette);
            var audio = workspace.AddNavigation("audio", "Audio", "headset", openAudio ?? (() => DeucarianToolRegistry.TryOpen(AudioToolId)));
            bool audioAvailable = openAudio != null || DeucarianToolRegistry.TryGet(AudioToolId, out _);
            audio.SetEnabled(audioAvailable);
            audio.tooltip = audioAvailable ? "Open Audio Palette Lab" : "Audio Palette Lab is not installed in this project.";
            Add(workspace, "deucarian.notifications.lab", "Notifications", DeucarianEditorIconIds.Sample);
            Add(workspace, DeucarianToolIds.Diagnostics, "Diagnostics", DeucarianEditorIconIds.Activity);
            workspace.AddNavigation("advanced", "Advanced", DeucarianEditorIconIds.Settings,
                () => DeucarianControlCenterWindow.Open(DeucarianControlCenterArea.Developer), true);
            workspace.SelectNavigation(selectedTool);
            workspace.SearchField.RegisterValueChangedCallback(evt =>
            {
                if (!filterNavigation) return;
                foreach (var item in workspace.Navigation.Children())
                    item.style.display = (item.Q<Label>()?.text ?? "").IndexOf(evt.newValue ?? "", StringComparison.OrdinalIgnoreCase) >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            });
        }

        private static void Add(DeucarianEditorWorkspace workspace, string toolId, string label, string icon)
        {
            var button = workspace.AddNavigation(toolId, label, icon, () => DeucarianToolRegistry.TryOpen(toolId));
            bool available = DeucarianToolRegistry.TryGet(toolId, out _);
            button.SetEnabled(available);
            button.tooltip = available ? "Open " + label : "This tool is not installed in the current project.";
        }
    }
}
