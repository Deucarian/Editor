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
            var header = DeucarianEditorWorkspaceControls.Region("workspace-header", "dw-header");
            var brand = DeucarianEditorWorkspaceControls.Region("workspace-brand", "dw-brand");
            brand.Add(DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Package));
            brand.Add(DeucarianEditorWorkspaceControls.Label("Deucarian", "dw-brand-name"));
            header.Add(brand);
            ContextButton = DeucarianEditorWorkspaceControls.Button(context, null);
            ContextButton.name = "workspace-context";
            ContextButton.AddToClassList("dw-project");
            ContextButton.tooltip = "Current project. This workspace never switches projects automatically.";
            header.Add(ContextButton);
            var spacer = DeucarianEditorWorkspaceControls.Region(null, "dw-spacer");
            header.Add(spacer);
            var search = DeucarianEditorWorkspaceControls.Region(null, "dw-search");
            search.Add(DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Search));
            SearchField = new TextField { name = "workspace-search", tooltip = "Find a tool · Ctrl/Cmd+K" };
            search.Add(SearchField);
            var placeholder = DeucarianEditorWorkspaceControls.Label("Find a tool…", "dw-search-placeholder");
            searchPlaceholder = placeholder;
            placeholder.pickingMode = PickingMode.Ignore;
            search.Add(placeholder);
            SearchField.RegisterValueChangedCallback(evt => placeholder.style.display = string.IsNullOrEmpty(evt.newValue) ? DisplayStyle.Flex : DisplayStyle.None);
            header.Add(search);
            Root.Insert(0, header);
            Sidebar = DeucarianEditorWorkspaceControls.Region("workspace-sidebar", "dw-sidebar");
            Navigation = DeucarianEditorWorkspaceControls.Region("workspace-navigation", "dw-navigation");
            Sidebar.Add(Navigation);
            Sidebar.Add(DeucarianEditorWorkspaceControls.Region(null, "dw-spacer"));
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
            Page.Add(pageHeader);
            Tabs = DeucarianEditorWorkspaceControls.Region("workspace-tabs", "dw-tabs");
            Scope = DeucarianEditorWorkspaceControls.Region("workspace-scope", "dw-scope");
            Content = DeucarianEditorWorkspaceControls.Region("workspace-content", "dw-content");
            var footerBar = DeucarianEditorWorkspaceControls.Region("workspace-footer-bar", "dw-footer");
            Footer = DeucarianEditorWorkspaceControls.Region("workspace-footer", "dw-footer-content");
            FooterLeading = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            FooterTrailing = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            Footer.Add(FooterLeading);
            Footer.Add(DeucarianEditorWorkspaceControls.Region(null, "dw-spacer"));
            Footer.Add(FooterTrailing);
            footerBar.Add(Footer);
            Page.Add(Tabs);
            Page.Add(Scope);
            Page.Add(Content);
            uiScale = new DeucarianEditorWorkspaceScale(Root, footerBar);
            resized = evt => ApplyWidth(evt.newRect.width);
            Root.RegisterCallback(resized);
            Root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            ApplyWidth(root.resolvedStyle.width);
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
        public VisualElement Drawer => workbench.Drawer;

        public void SetSearchPrompt(string prompt)
        {
            searchPlaceholder.text = prompt ?? string.Empty;
            searchPlaceholder.style.display = string.IsNullOrEmpty(SearchField.value) ? DisplayStyle.Flex : DisplayStyle.None;
            SearchField.tooltip = prompt + " · Ctrl/Cmd+K";
        }

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
            foreach (var entry in navigation)
                entry.Value.EnableInClassList("dw-selected", string.Equals(entry.Key, id, StringComparison.Ordinal));
        }

        public void ApplyWidth(float width)
        {
            bool known = !float.IsNaN(width) && !float.IsInfinity(width) && width > 0;
            Root.EnableInClassList("dw-compact", known && width < 1100);
            Root.EnableInClassList("dw-narrow", known && width < 760);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            navigationBinding?.Dispose();
            navigationBinding = null;
            uiScale.Dispose();
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
