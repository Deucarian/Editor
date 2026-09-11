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
        private bool revealSelection = true;
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
            revealSelection = true;
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
            AddTool(workspace.Navigation, DeucarianToolIds.ControlCenter, "Overview", DeucarianEditorIconIds.Home);
            var menu = new ToolbarMenu { name = "workspace-navigation-menu", text = "Navigate…" };
            menu.AddToClassList("dw-navigation-menu");
            workspace.Navigation.Add(menu);
            var rail = DeucarianEditorWorkspaceControls.Scroll("workspace-navigation-rail");
            rail.AddToClassList("dw-navigation-rail");
            workspace.Navigation.Add(rail);
            var railMenus = new Dictionary<string, ToolbarMenu>(StringComparer.Ordinal);
            menu.menu.AppendAction("Overview", _ => DeucarianEditorNavigation.Open(workspace.Root, DeucarianToolIds.ControlCenter, "overview"));
            scroll = DeucarianEditorWorkspaceControls.Scroll("workspace-navigation-scroll");
            scroll.AddToClassList("dw-navigation-tree");
            workspace.Navigation.Add(scroll);
            var groups = new Dictionary<string, Foldout>(StringComparer.Ordinal);
            string query = filter ? workspace.SearchField.value?.Trim() ?? "" : "";
            filteredView = query.Length > 0;
            DeucarianToolRegistry.TryGet(selected, out var selectedTool);
            string selectedPath = selectedTool?.NavigationPath ?? "";
            Button selectedButton = null;
            foreach (var tool in DeucarianToolRegistry.GetTools())
            {
                if (tool.Id == DeucarianToolIds.ControlCenter) continue;
                string searchable = tool.DisplayName + " " + tool.NavigationPath + " " + tool.Description + " " + string.Join(" ", tool.SearchTerms);
                if (query.Length > 0 && searchable.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;
                var destination = tool;
                menu.menu.AppendAction(tool.NavigationPath + "/" + tool.DisplayName,
                    _ => DeucarianEditorNavigation.Open(workspace.Root, destination.Id),
                    tool.CreatePage == null ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
                string topGroup = tool.NavigationPath.Split('/')[0].Trim();
                if (topGroup.Length == 0) topGroup = "Tools";
                if (!railMenus.TryGetValue(topGroup, out var railMenu))
                {
                    railMenu = new ToolbarMenu { name = "workspace-rail-" + topGroup, text = topGroup, tooltip = topGroup };
                    railMenu.AddToClassList("dw-rail-menu");
                    railMenu.Add(DeucarianEditorWorkspaceControls.Icon(tool.NavigationGroupIcon ?? GroupIcon(topGroup)));
                    railMenu.EnableInClassList("dw-selected", selectedPath == topGroup || selectedPath.StartsWith(topGroup + "/", StringComparison.Ordinal));
                    railMenus.Add(topGroup, railMenu); rail.Add(railMenu);
                }
                string railLabel = tool.NavigationPath.Length > topGroup.Length
                    ? tool.NavigationPath.Substring(topGroup.Length + 1) + "/" + tool.NavigationLabel : tool.NavigationLabel;
                if (tool.IsFeatureEnabled != null && !tool.IsFeatureEnabled()) railLabel += " (off)";
                railMenu.menu.AppendAction(railLabel, _ => DeucarianEditorNavigation.Open(workspace.Root, destination.Id),
                    tool.CreatePage == null ? DropdownMenuAction.Status.Disabled
                    : tool.Id == selected ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
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
                        bool open = activePath && revealSelection || state.IsExpanded(path, activePath);
                        group = new Foldout { text = label, value = query.Length > 0 || open,
                            name = "workspace-group-" + path };
                        group.AddToClassList("dw-navigation-group");
                        string groupIcon = tool.NavigationGroupIcon ?? GroupIcon(label);
                        if (!string.IsNullOrEmpty(groupIcon))
                        {
                            group.AddToClassList("dw-navigation-icon-group");
                            var toggle = group.Q<Toggle>();
                            var symbol = DeucarianEditorWorkspaceControls.Icon(groupIcon);
                            symbol.AddToClassList("dw-navigation-group-icon");
                            toggle.Insert(0, symbol);
                        }
                        group.RegisterValueChangedCallback(evt => { if (evt.target == group) state.SetExpanded(groupPath, evt.newValue); });
                        groups.Add(path, group);
                        parent.Add(group);
                    }
                    parent = group;
                }
                var item = AddTool(parent, tool.Id, tool.NavigationLabel, tool.IconKey);
                if (tool.Id == selected) selectedButton = item;
            }
            var advanced = workspace.AddNavigation("advanced", "Advanced", "cog",
                () => DeucarianEditorNavigation.Open(workspace.Root, DeucarianToolIds.ControlCenter, "project"), true);
            advanced.tooltip = "Project checks and all registered tools, in this window.";
            workspace.SelectNavigation(workspace.SelectedNavigation ?? (selected == DeucarianEditorWorkspaceNavigation.AudioToolId ? "audio" : selected));
            var currentScroll = scroll;
            float targetOffset = filteredView ? 0 : state.ScrollOffset;
            int restoreAttempts = 0;
            int revealAttempts = 0;
            float lastHeight = -1;
            bool offsetRestored = false;
            currentScroll.schedule.Execute(() =>
            {
                if (disposed || scroll != currentScroll) return;
                float height = currentScroll.contentContainer.layout.height;
                bool settled = currentScroll.contentViewport.layout.height > 0 && height > 0 && height == lastHeight;
                lastHeight = height;
                if (!settled && ++restoreAttempts < 8) return;
                if (!offsetRestored)
                {
                    currentScroll.scrollOffset = new UnityEngine.Vector2(0, targetOffset);
                    offsetRestored = true;
                    return;
                }
                if (revealSelection && !filteredView && selectedButton != null && selectedButton.layout.height > 0)
                {
                    var viewport = currentScroll.contentViewport;
                    float scale = viewport.worldBound.height / viewport.layout.height;
                    float below = selectedButton.worldBound.yMax - viewport.worldBound.yMax;
                    float above = selectedButton.worldBound.yMin - viewport.worldBound.yMin;
                    if (scale > 0 && (below > .5f || above < -.5f) && revealAttempts++ < 8)
                    {
                        currentScroll.scrollOffset += new UnityEngine.Vector2(0, (below > 0 ? below : above) / scale);
                        return;
                    }
                }
                revealSelection = false;
                restoringScroll = false;
                SaveScroll();
            }).Every(16).Until(() => disposed || scroll != currentScroll || !restoringScroll);
            currentScroll.verticalScroller.valueChanged += value =>
            {
                if (!restoringScroll && !filteredView && currentScroll.panel != null && scroll == currentScroll)
                    state.ScrollOffset = value;
            };
        }

        private void SaveScroll()
        {
            if (!restoringScroll && scroll != null && !filteredView)
                state.ScrollOffset = scroll.scrollOffset.y;
        }

        private Button AddTool(VisualElement parent, string id, string label, string icon)
        {
            string navigationId = id == DeucarianEditorWorkspaceNavigation.AudioToolId ? "audio" : id;
            var button = workspace.AddNavigation(navigationId, label, icon,
                () => DeucarianEditorNavigation.Open(workspace.Root, id, id == DeucarianToolIds.ControlCenter ? "overview" : null));
            if (parent != workspace.Navigation) parent.Add(button);
            else button.AddToClassList("dw-navigation-overview");
            bool available = DeucarianToolRegistry.TryGet(id, out var descriptor) && descriptor.CreatePage != null;
            if (parent != workspace.Navigation) button.AddToClassList("dw-nav-text-only");
            if (descriptor?.IsFeatureEnabled != null && !descriptor.IsFeatureEnabled())
            {
                button.AddToClassList("dw-nav-off");
                button.Add(DeucarianEditorWorkspaceControls.Label("Off", "dw-nav-status"));
            }
            button.SetEnabled(available);
            button.tooltip = available ? "Show " + label + " in this window. Right-click to open separately."
                : "This package needs an in-window page update.";
            if (available) button.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("Open in new window", _ => DeucarianEditorToolWindow.Open(id))));
            return button;
        }

        private static string GroupIcon(string group)
        {
            switch (group)
            {
                case "Theming": case "Appearance": return DeucarianEditorIconIds.Palette;
                case "Audio": return DeucarianEditorIconIds.Audio;
                case "Notifications": return DeucarianEditorIconIds.Notifications;
                case "Connections": return DeucarianEditorIconIds.Integration;
                case "Communication": return DeucarianEditorIconIds.Communication;
                case "Diagnostics": return DeucarianEditorIconIds.Activity;
                case "Experience": return DeucarianEditorIconIds.Gamepad;
                case "Authoring": return DeucarianEditorIconIds.Authoring;
                case "Developer": return DeucarianEditorIconIds.Developer;
                default: return DeucarianEditorIconIds.Package;
            }
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
