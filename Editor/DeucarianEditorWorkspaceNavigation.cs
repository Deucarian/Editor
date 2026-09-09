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
            Add(workspace, AudioToolId, "Audio", "headset", navigationId: "audio", openNew: openAudio);
            Add(workspace, "deucarian.notifications.lab", "Notifications", DeucarianEditorIconIds.Sample);
            Add(workspace, DeucarianToolIds.Diagnostics, "Diagnostics", DeucarianEditorIconIds.Activity);
            Add(workspace, DeucarianToolIds.ControlCenter, "Advanced", DeucarianEditorIconIds.Settings,
                navigationId: "advanced", route: "developer", footer: true);
            workspace.SelectNavigation(selectedTool);
            workspace.SearchField.RegisterValueChangedCallback(evt =>
            {
                if (!filterNavigation) return;
                foreach (var item in workspace.Navigation.Children())
                    item.style.display = (item.Q<Label>()?.text ?? "").IndexOf(evt.newValue ?? "", StringComparison.OrdinalIgnoreCase) >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            });
        }

        private static void Add(DeucarianEditorWorkspace workspace, string toolId, string label, string icon,
            string navigationId = null, string route = null, bool footer = false, Action openNew = null)
        {
            var button = workspace.AddNavigation(navigationId ?? toolId, label, icon,
                () => DeucarianEditorNavigateEvent.Send(workspace.Root, toolId, route), footer);
            bool available = DeucarianToolRegistry.TryGet(toolId, out var tool) && tool.CreatePage != null;
            button.SetEnabled(available);
            button.tooltip = available ? "Show " + label + " in this window. Right-click to open separately." :
                "Install or update this tool to enable in-window navigation.";
            button.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("Open in new window", _ =>
                {
                    DeucarianEditorToolWindow.Open(toolId, route);
                })));
        }
    }
}
