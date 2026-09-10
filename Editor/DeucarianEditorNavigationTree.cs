using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Renders the installed-tool hierarchy using its owning window's navigation state.</summary>
    internal sealed class DeucarianEditorNavigationTree : IDisposable
    {
        private readonly DeucarianEditorWorkspace workspace;
        private readonly string selected;
        private readonly bool filter;
        private DeucarianEditorNavigationState state = new DeucarianEditorNavigationState();
        private ScrollView scroll;
        private bool restoringScroll;
        private bool filteredView;
        private bool disposed;

        internal DeucarianEditorNavigationTree(DeucarianEditorWorkspace workspace, string selected, bool filter)
        {
            this.workspace = workspace;
            this.selected = selected == "audio" ? DeucarianEditorWorkspaceNavigation.AudioToolId : selected;
            this.filter = filter;
            workspace.Root.RegisterCallback<AttachToPanelEvent>(OnAttach);
            workspace.Root.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            workspace.SearchField.RegisterValueChangedCallback(OnSearch);
            if (workspace.Root.panel != null)
            {
                state = workspace.Root.GetFirstAncestorOfType<DeucarianEditorPageHost>()?.NavigationState ?? state;
                Subscribe();
            }
            Render();
        }

        private void Subscribe() { DeucarianToolRegistry.Changed -= Render; DeucarianToolRegistry.Changed += Render; }
        private void OnAttach(AttachToPanelEvent evt)
        {
            restoringScroll = true;
            state = workspace.Root.GetFirstAncestorOfType<DeucarianEditorPageHost>()?.NavigationState ?? state;
            Subscribe();
            Render();
        }
        private void OnDetach(DetachFromPanelEvent evt)
        {
            SaveScroll();
            restoringScroll = true;
            DeucarianToolRegistry.Changed -= Render;
        }
        private void OnSearch(ChangeEvent<string> evt) { if (filter) Render(); }

        private void Render()
        {
            if (disposed) return;
            SaveScroll();
            restoringScroll = true;
            workspace.ClearNavigation();
            AddTool(workspace.Navigation, DeucarianToolIds.ControlCenter, "Overview", DeucarianEditorIconIds.Dashboard);
            var menu = new ToolbarMenu { name = "workspace-navigation-menu", text = "Navigate…" };
            menu.AddToClassList("dw-navigation-menu");
            workspace.Navigation.Add(menu);
            menu.menu.AppendAction("Overview", _ => DeucarianEditorNavigation.Open(workspace.Root, DeucarianToolIds.ControlCenter, "overview"));
            scroll = DeucarianEditorWorkspaceControls.Scroll("workspace-navigation-scroll");
            scroll.AddToClassList("dw-navigation-tree");
            workspace.Navigation.Add(scroll);
            var groups = new Dictionary<string, Foldout>(StringComparer.Ordinal);
            string query = filter ? workspace.SearchField.value?.Trim() ?? "" : "";
            filteredView = query.Length > 0;
            DeucarianToolRegistry.TryGet(selected, out var selectedTool);
            string selectedPath = selectedTool?.NavigationPath ?? "";
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
                        bool activePath = selectedPath == path || selectedPath.StartsWith(path + "/", StringComparison.Ordinal);
                        bool open = state.IsExpanded(path, activePath);
                        group = new Foldout { text = label, value = query.Length > 0 || open,
                            name = "workspace-group-" + path };
                        group.AddToClassList("dw-navigation-group");
                        if (!string.IsNullOrEmpty(tool.NavigationGroupIcon))
                        {
                            group.AddToClassList("dw-navigation-icon-group");
                            var toggle = group.Q<Toggle>();
                            var symbol = DeucarianEditorWorkspaceControls.Icon(tool.NavigationGroupIcon);
                            symbol.AddToClassList("dw-navigation-group-icon");
                            toggle.Insert(0, symbol);
                        }
                        group.RegisterValueChangedCallback(evt => { if (evt.target == group) state.SetExpanded(groupPath, evt.newValue); });
                        groups.Add(path, group);
                        parent.Add(group);
                    }
                    parent = group;
                }
                AddTool(parent, tool.Id, tool.NavigationLabel, tool.IconKey);
            }
            var advanced = workspace.AddNavigation("advanced", "Advanced", DeucarianEditorIconIds.Settings,
                () => DeucarianEditorNavigation.Open(workspace.Root, DeucarianToolIds.ControlCenter, "developer"), true);
            advanced.tooltip = "Project checks and all registered tools, in this window.";
            workspace.SelectNavigation(workspace.SelectedNavigation ?? (selected == DeucarianEditorWorkspaceNavigation.AudioToolId ? "audio" : selected));
            var currentScroll = scroll;
            float targetOffset = filteredView ? 0 : state.ScrollOffset;
            int restoreAttempts = 0;
            currentScroll.schedule.Execute(() =>
            {
                if (disposed || scroll != currentScroll) return;
                if (targetOffset > 0 && currentScroll.verticalScroller.highValue <= 0 && ++restoreAttempts < 8) return;
                currentScroll.scrollOffset = new UnityEngine.Vector2(0, targetOffset);
                restoringScroll = false;
            }).Every(16).Until(() => disposed || scroll != currentScroll || !restoringScroll);
            currentScroll.verticalScroller.valueChanged += _ =>
            {
                if (!restoringScroll && currentScroll.panel != null && scroll == currentScroll) SaveScroll();
            };
        }

        private void SaveScroll()
        {
            if (!restoringScroll && scroll != null && !filteredView)
                state.ScrollOffset = scroll.scrollOffset.y;
        }

        private void AddTool(VisualElement parent, string id, string label, string icon)
        {
            string navigationId = id == DeucarianEditorWorkspaceNavigation.AudioToolId ? "audio" : id;
            var button = workspace.AddNavigation(navigationId, label, icon,
                () => DeucarianEditorNavigation.Open(workspace.Root, id, id == DeucarianToolIds.ControlCenter ? "overview" : null));
            if (parent != workspace.Navigation) parent.Add(button);
            else button.AddToClassList("dw-navigation-overview");
            bool available = DeucarianToolRegistry.TryGet(id, out var descriptor) && descriptor.CreatePage != null;
            if (descriptor != null && !descriptor.ShowNavigationIcon) button.AddToClassList("dw-nav-text-only");
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
