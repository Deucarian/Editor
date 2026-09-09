using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>One workspace's installed-tool hierarchy, filtering, and expansion state.</summary>
    internal sealed class DeucarianEditorNavigationTree : IDisposable
    {
        private readonly DeucarianEditorWorkspace workspace;
        private readonly string selected;
        private readonly bool filter;
        private readonly Dictionary<string, bool> expanded = new Dictionary<string, bool>(StringComparer.Ordinal);
        private bool disposed;

        internal DeucarianEditorNavigationTree(DeucarianEditorWorkspace workspace, string selected, bool filter)
        {
            this.workspace = workspace;
            this.selected = selected == "audio" ? DeucarianEditorWorkspaceNavigation.AudioToolId : selected;
            this.filter = filter;
            workspace.Root.RegisterCallback<AttachToPanelEvent>(OnAttach);
            workspace.Root.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            workspace.SearchField.RegisterValueChangedCallback(OnSearch);
            if (workspace.Root.panel != null) Subscribe();
            Render();
        }

        private void Subscribe() { DeucarianToolRegistry.Changed -= Render; DeucarianToolRegistry.Changed += Render; }
        private void OnAttach(AttachToPanelEvent evt) { Subscribe(); Render(); }
        private void OnDetach(DetachFromPanelEvent evt) => DeucarianToolRegistry.Changed -= Render;
        private void OnSearch(ChangeEvent<string> evt) { if (filter) Render(); }

        private void Render()
        {
            if (disposed) return;
            workspace.ClearNavigation();
            AddTool(workspace.Navigation, DeucarianToolIds.ControlCenter, "Overview", DeucarianEditorIconIds.Dashboard);
            var menu = new ToolbarMenu { name = "workspace-navigation-menu", text = "Navigate…" };
            menu.AddToClassList("dw-navigation-menu");
            workspace.Navigation.Add(menu);
            menu.menu.AppendAction("Overview", _ => DeucarianEditorNavigation.Open(workspace.Root, DeucarianToolIds.ControlCenter));
            var scroll = DeucarianEditorWorkspaceControls.Scroll("workspace-navigation-scroll");
            scroll.AddToClassList("dw-navigation-tree");
            workspace.Navigation.Add(scroll);
            var groups = new Dictionary<string, Foldout>(StringComparer.Ordinal);
            string query = filter ? workspace.SearchField.value?.Trim() ?? "" : "";
            foreach (var tool in DeucarianToolRegistry.GetTools())
            {
                if (tool.Id == DeucarianToolIds.ControlCenter) continue;
                string searchable = tool.DisplayName + " " + tool.NavigationPath + " " + tool.Description + " " + string.Join(" ", tool.SearchTerms);
                if (query.Length > 0 && searchable.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;
                var destination = tool;
                menu.menu.AppendAction(tool.NavigationPath + "/" + tool.DisplayName,
                    _ => DeucarianEditorNavigation.Open(workspace.Root, destination.Id),
                    tool.CreatePage == null ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
                VisualElement parent = scroll;
                string path = "";
                foreach (string part in tool.NavigationPath.Split('/'))
                {
                    string label = part.Trim();
                    if (label.Length == 0) continue;
                    path = path.Length == 0 ? label : path + "/" + label;
                    if (!groups.TryGetValue(path, out var group))
                    {
                        string groupPath = path;
                        bool open = expanded.TryGetValue(path, out bool value) && value;
                        group = new Foldout { text = label, value = query.Length > 0 || open,
                            name = "workspace-group-" + path };
                        group.AddToClassList("dw-navigation-group");
                        group.RegisterValueChangedCallback(evt => { if (evt.target == group) expanded[groupPath] = evt.newValue; });
                        groups.Add(path, group);
                        parent.Add(group);
                    }
                    if (tool.Id == selected) group.SetValueWithoutNotify(true);
                    parent = group;
                }
                AddTool(parent, tool.Id, tool.DisplayName, tool.IconKey);
            }
            var advanced = workspace.AddNavigation("advanced", "Advanced", DeucarianEditorIconIds.Settings,
                () => DeucarianEditorNavigation.Open(workspace.Root, DeucarianToolIds.ControlCenter, "developer"), true);
            advanced.tooltip = "Project checks and all registered tools, in this window.";
            workspace.SelectNavigation(selected == DeucarianEditorWorkspaceNavigation.AudioToolId ? "audio" : selected);
        }

        private void AddTool(VisualElement parent, string id, string label, string icon)
        {
            string navigationId = id == DeucarianEditorWorkspaceNavigation.AudioToolId ? "audio" : id;
            var button = workspace.AddNavigation(navigationId, label, icon,
                () => DeucarianEditorNavigation.Open(workspace.Root, id));
            if (parent != workspace.Navigation) parent.Add(button);
            else button.AddToClassList("dw-navigation-overview");
            bool available = DeucarianToolRegistry.TryGet(id, out var descriptor) && descriptor.CreatePage != null;
            button.SetEnabled(available);
            button.tooltip = available ? "Show " + label + " in this window. Right-click to open separately."
                : "This package needs an in-window page update.";
            if (available) button.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("Open in new window", _ => DeucarianEditorToolWindow.Open(id))));
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            DeucarianToolRegistry.Changed -= Render;
            workspace.Root.UnregisterCallback<AttachToPanelEvent>(OnAttach);
            workspace.Root.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            workspace.SearchField.UnregisterValueChangedCallback(OnSearch);
        }
    }
}
