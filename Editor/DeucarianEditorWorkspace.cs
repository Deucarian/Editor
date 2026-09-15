using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Package-owned task layout. Consumers supply content and actions, never visual tokens.</summary>
    public sealed class DeucarianEditorWorkspace : IDisposable
    {
        public const string StyleSheetPath = DeucarianEditorUIResources.StylesPath + "/DeucarianWorkspace.uss";
        public static Vector2 MinimumWindowSize => new Vector2(820f, 650f);
        public static Vector2 PreferredWindowSize => new Vector2(1180f, 800f);

        public static void ConfigureWindow(EditorWindow window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            Rect bounds = FitWindowBounds(window.position, window.docked);
            window.minSize = MinimumWindowSize;
            if (!window.docked && window.position != bounds) window.position = bounds;
        }

        internal static Rect FitWindowBounds(Rect current, bool docked)
        {
            if (docked || current.width >= MinimumWindowSize.x && current.height >= MinimumWindowSize.y)
                return current;
            return new Rect(current.position, new Vector2(Mathf.Max(current.width, PreferredWindowSize.x),
                Mathf.Max(current.height, PreferredWindowSize.y)));
        }
        private readonly DeucarianEditorWorkbench workbench;
        private readonly VisualElement host;
        private readonly Dictionary<string, Button> navigation = new Dictionary<string, Button>(StringComparer.Ordinal);
        private readonly EventCallback<GeometryChangedEvent> resized;
        private bool disposed;
        private readonly Label searchPlaceholder;
        private readonly DeucarianEditorWorkspaceScale uiScale;
        private IDisposable navigationBinding;
        private readonly IVisualElementScheduledItem sectionLayout;

        public DeucarianEditorWorkspace(VisualElement root, string context, bool includeDrawer = false)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            host = root;
            workbench = DeucarianEditorWorkbench.Create(root, new DeucarianEditorWorkbenchOptions {
                IncludeToolbar = false, IncludeDrawer = includeDrawer, DrawerMode = DeucarianEditorWorkbenchDrawerMode.Overlay });
            Root = workbench.ShellContent;
            Root.AddToClassList("deucarian-workspace");
            root.AddToClassList("deucarian-workspace-host");
            DeucarianEditorUIResources.TryAddStyleSheet(root, StyleSheetPath);
            DeucarianEditorUIResources.TryAddStyleSheet(root, DeucarianEditorUIResources.StylesPath + "/DeucarianFeatures.uss");
            var header = DeucarianEditorWorkspaceControls.Region("workspace-header", "dw-header");
            var brand = DeucarianEditorWorkspaceControls.Region("workspace-brand", "dw-brand");
            var mark = new DeucarianEditorBrandMark();
            brand.Add(mark);
            brand.Add(DeucarianEditorWorkspaceControls.Label("Deucarian", "dw-brand-name"));
            header.Add(brand);
            ContextButton = DeucarianEditorWorkspaceControls.Button(context, null);
            ContextButton.name = "workspace-context";
            ContextButton.AddToClassList("dw-project");
            ContextButton.tooltip = context + "\nCurrent project. This workspace never switches projects automatically.";
            header.Add(ContextButton);
            var spacer = DeucarianEditorWorkspaceControls.Region(null, "dw-spacer");
            header.Add(spacer);
            var search = DeucarianEditorWorkspaceControls.Search("workspace-search", "Find a tool…", out var searchInput);
            SearchField = searchInput;
            SearchField.tooltip = "Find a tool · Ctrl/Cmd+K";
            searchPlaceholder = search.Q<Label>(className: "dw-search-placeholder");
            header.Add(search);
            Root.Insert(0, header);
            Root.Insert(0, new DeucarianEditorWorkspaceBackdrop());
            Sidebar = DeucarianEditorWorkspaceControls.Region("workspace-sidebar", "dw-sidebar");
            Navigation = DeucarianEditorWorkspaceControls.Region("workspace-navigation", "dw-navigation");
            Sidebar.Add(Navigation);
            NavigationFooter = DeucarianEditorWorkspaceControls.Region("workspace-navigation-footer", "dw-navigation-footer");
            Sidebar.Add(NavigationFooter);
            workbench.Main.Insert(0, Sidebar);
            Page = workbench.Content;
            Page.AddToClassList("dw-page");
            var pageHeader = DeucarianEditorWorkspaceControls.Region(null, "dw-page-header");
            var heading = DeucarianEditorWorkspaceControls.Region(null, "dw-heading");
            Title = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-title");
            Subtitle = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-subtitle");
            heading.Add(Title);
            heading.Add(Subtitle);
            pageHeader.Add(heading);
            PageActions = DeucarianEditorWorkspaceControls.Region("workspace-page-actions", "dw-page-actions");
            pageHeader.Add(PageActions);
            var contextScroll = new ScrollView(ScrollViewMode.Vertical) { name = "workspace-context-scroll" };
            contextScroll.AddToClassList("dw-context-scroll");
            contextScroll.Add(pageHeader);
            Page.Add(contextScroll);
            Tabs = DeucarianEditorWorkspaceControls.Region("workspace-tabs", "dw-tabs");
            Scope = DeucarianEditorWorkspaceControls.Region("workspace-scope", "dw-scope");
            Tabs.AddToClassList("dw-region-empty"); Scope.AddToClassList("dw-region-empty");
            Content = DeucarianEditorWorkspaceControls.Region("workspace-content", "dw-content");
            var footerBar = DeucarianEditorWorkspaceControls.Region("workspace-footer-bar", "dw-footer");
            Footer = DeucarianEditorWorkspaceControls.Region("workspace-footer", "dw-footer-content");
            FooterLeading = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            FooterTrailing = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            Footer.Add(FooterLeading);
            Footer.Add(DeucarianEditorWorkspaceControls.Region(null, "dw-spacer"));
            Footer.Add(FooterTrailing);
            footerBar.Add(Footer);
            var projectSettings = DeucarianEditorWorkspaceControls.IconButton("Project settings",
                DeucarianEditorIconIds.Settings, () => DeucarianEditorNavigation.Open(Root, DeucarianToolIds.ControlCenter, "settings"),
                DeucarianEditorButtonRole.Quiet);
            projectSettings.name = "workspace-project-settings";
            projectSettings.AddToClassList("dw-project-settings");
            footerBar.Insert(0, projectSettings);
            contextScroll.Add(Tabs);
            contextScroll.Add(Scope);
            Page.Add(Content);
            uiScale = new DeucarianEditorWorkspaceScale(Root, footerBar);
            resized = evt => ApplyWidth(evt.newRect.width);
            Root.RegisterCallback(resized);
            Root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            ApplyWidth(root.resolvedStyle.width);
            sectionLayout = Root.schedule.Execute(() => {
                Tabs.EnableInClassList("dw-region-empty", Tabs.childCount == 0);
                Scope.EnableInClassList("dw-region-empty", Scope.childCount == 0);
            }).Every(100);
        }

        public VisualElement Root { get; }
        public VisualElement Sidebar { get; }
        public VisualElement Navigation { get; }
        public VisualElement NavigationFooter { get; }
        public VisualElement Page { get; }
        public VisualElement PageActions { get; }
        public VisualElement Tabs { get; }
        public VisualElement Scope { get; }
        public VisualElement Content { get; }
        public VisualElement Footer { get; }
        public Label Title { get; }
        public Label Subtitle { get; }
        public Label FooterLeading { get; }
        public Label FooterTrailing { get; }
        public TextField SearchField { get; }
        public Button ContextButton { get; }
        internal string SelectedNavigation { get; private set; }
        public VisualElement Drawer => workbench.Drawer;

        public void SetSearchPrompt(string prompt)
        {
            searchPlaceholder.text = prompt ?? string.Empty;
            searchPlaceholder.style.display = string.IsNullOrEmpty(SearchField.value) ? DisplayStyle.Flex : DisplayStyle.None;
            SearchField.tooltip = prompt + " · Ctrl/Cmd+K";
        }

        /// <summary>Asset/context filters precede local tabs on collection pages.</summary>
        public void SetScopeBeforeTabs(bool before = true)
        {
            Scope.EnableInClassList("dw-scope-before-tabs", before);
            if (before) Scope.PlaceBehind(Tabs);
            else Scope.PlaceInFront(Tabs);
        }

        public void SetScopeStacked(bool stacked = true) => Scope.EnableInClassList("dw-scope-stacked", stacked);

        public Button AddNavigation(string id, string label, string icon, Action open, bool footer = false)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A navigation ID is required.", nameof(id));
            if (navigation.ContainsKey(id)) throw new ArgumentException("Navigation IDs must be unique.", nameof(id));
            var button = DeucarianEditorWorkspaceControls.Button(string.Empty, open);
            button.name = "workspace-nav-" + id;
            button.AddToClassList("dw-nav-item");
            button.Add(DeucarianEditorWorkspaceControls.Icon(icon));
            button.Add(DeucarianEditorWorkspaceControls.Label(label, "dw-nav-label"));
            button.tooltip = label;
            navigation.Add(id, button);
            (footer ? NavigationFooter : Navigation).Add(button);
            return button;
        }

        internal void BindNavigation(IDisposable binding)
        {
            navigationBinding?.Dispose();
            navigationBinding = binding;
        }

        internal void ClearNavigation()
        {
            navigation.Clear();
            Navigation.Clear();
            NavigationFooter.Clear();
        }

        public void SelectNavigation(string id)
        {
            SelectedNavigation = id;
            foreach (var entry in navigation)
                entry.Value.EnableInClassList("dw-selected", string.Equals(entry.Key, id, StringComparison.Ordinal));
        }

        public void ApplyWidth(float width)
        {
            bool known = !float.IsNaN(width) && !float.IsInfinity(width) && width > 0;
            Root.EnableInClassList("dw-compact", known && width < 1470);
            Root.EnableInClassList("dw-narrow", known && width < 1067);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            navigationBinding?.Dispose();
            navigationBinding = null;
            uiScale.Dispose();
            sectionLayout.Pause();
            Root.UnregisterCallback(resized);
            Root.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            if (host.Contains(Root)) host.RemoveFromClassList("deucarian-workspace-host");
            workbench.Dispose();
            navigation.Clear();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!(evt.ctrlKey || evt.commandKey) || evt.keyCode != KeyCode.K) return;
            SearchField.Focus();
            SearchField.SelectAll();
            evt.StopPropagation();
        }
    }
}
