using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Unified, package-contributed Deucarian editor entry point.</summary>
    public sealed class DeucarianControlCenterWindow : EditorWindow
    {
        public const string MenuPath = "Tools/Deucarian/Control Center...";
        public const string DeveloperMenuPath =
            "Tools/Deucarian/Advanced/Developer Tools...";
        public const string LegacyMenuPath =
            "Tools/Deucarian/Advanced/Legacy Shortcuts...";

        private const int SnapshotIntervalMilliseconds = 15000;
        private DeucarianEditorPageSession navigation;
        private VisualElement pageRoot;
        private DeucarianEditorWorkspace workspace;
        private DeucarianControlCenterView view;
        private VisualElement settingsPage;
        private bool showSettings;
        private DeucarianControlCenterSnapshot snapshot;
        private IVisualElementScheduledItem periodicRefresh;
        private TextField searchField;
        private Label summary;
        [SerializeField] private DeucarianControlCenterArea selectedArea =
            DeucarianControlCenterArea.Overview;
        private string focusedTargetId;
        [SerializeField] private string searchQuery = string.Empty;
        private bool refreshQueued;
        private bool subscribed;

        public DeucarianControlCenterArea SelectedArea => selectedArea;
        public string FocusedTargetId => focusedTargetId;
        internal string SearchQuery => searchQuery;

        [MenuItem(MenuPath, priority = -1000)]
        private static void OpenMenu()
        {
            Open();
        }

        [MenuItem(DeveloperMenuPath, priority = 900)]
        private static void OpenDeveloperMenu()
        {
            Open(DeucarianControlCenterArea.Developer);
        }

        [MenuItem(LegacyMenuPath, priority = 901)]
        private static void OpenLegacyMenu()
        {
            DeucarianControlCenterWindow window =
                Open(DeucarianControlCenterArea.Developer);
            window.SetSearch("legacy shortcuts");
        }

        public static DeucarianControlCenterWindow Open()
        {
            return Open(DeucarianControlCenterArea.Overview);
        }

        public static DeucarianControlCenterWindow Open(
            DeucarianControlCenterArea area)
        {
            DeucarianControlCenterWindow window =
                DeucarianEditorWindowPages.GetStandalone<DeucarianControlCenterWindow>(
                    "Deucarian Control Center");
            window.showSettings = false;
            window.navigation?.Navigate(DeucarianToolIds.ControlCenter, area == DeucarianControlCenterArea.Developer ? "developer" : null);
            window.selectedArea = area;
            window.focusedTargetId = null;
            window.SetSearch(string.Empty);
            DeucarianEditorWorkspace.ConfigureWindow(window);
            window.Show();
            window.Focus();
            window.Render();
            return window;
        }

        public static DeucarianControlCenterWindow OpenProjectIssue(
            string issueCode)
        {
            DeucarianControlCenterWindow window =
                Open(DeucarianControlCenterArea.Project);
            window.focusedTargetId = string.IsNullOrWhiteSpace(issueCode)
                ? null
                : "deucarian.readiness.issue." + issueCode.Trim();
            window.Render();
            return window;
        }

        public void CreateGUI()
        {
            navigation?.Dispose();
            navigation = new DeucarianEditorPageSession(this, DeucarianToolIds.ControlCenter,
                BuildPage, ActivatePage);
        }

        internal static IDeucarianEditorPage CreatePage() =>
            DeucarianEditorWindowPages.Create<DeucarianControlCenterWindow>(
                (window, root) => window.BuildPage(root), (window, route) => window.ActivatePage(route));

        private void ActivatePage(string route)
        {
            if (string.IsNullOrEmpty(route)) return;
            bool wasSettings = showSettings;
            showSettings = route == "settings";
            if (showSettings) { if (!wasSettings) SetSearch(string.Empty); return; }
            var area = DeucarianControlCenterArea.Overview;
            foreach (DeucarianControlCenterArea candidate in Enum.GetValues(typeof(DeucarianControlCenterArea)))
                if (DeucarianControlCenterAreaIds.GetId(candidate) == route) { area = candidate; break; }
            if (wasSettings || selectedArea != area || !string.IsNullOrEmpty(searchQuery) || focusedTargetId != null) Navigate(area, null);
            workspace?.SelectNavigation(area == DeucarianControlCenterArea.Developer || area == DeucarianControlCenterArea.Project
                ? "advanced" : DeucarianToolIds.ControlCenter);
        }

        private void BuildPage(VisualElement root)
        {
            DisposeVisualTree();
            pageRoot = root;
            pageRoot.Clear();
            workspace = new DeucarianEditorWorkspace(pageRoot, Application.productName);
            workspace.Title.text = "Control Center";
            workspace.Subtitle.text = "Review project readiness and open installed Deucarian tools.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, DeucarianToolIds.ControlCenter, filterNavigation: false);
            workspace.SetSearchPrompt("Search tools and checks…");
            searchField = workspace.SearchField;
            searchField.name = "control-center-search";
            searchField.SetValueWithoutNotify(searchQuery);
            searchField.RegisterValueChangedCallback(evt => { searchQuery = evt.newValue ?? string.Empty; Render(); });
            summary = workspace.FooterTrailing;
            var refresh = DeucarianEditorWorkspaceControls.Button("Refresh", () => Refresh(true));
            refresh.name = "control-center-refresh";
            workspace.PageActions.Add(refresh);
            DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
            DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);

            view = new DeucarianControlCenterView(
                Navigate,
                () => Refresh(true));
            workspace.Content.Add(view.Root);
            settingsPage = DeucarianEditorWorkspaceControls.Scroll("control-center-settings");
            DeucarianControlCenterSettings.Build(settingsPage);
            workspace.Content.Add(settingsPage);
            ConfigureFooter();
            pageRoot.RegisterCallback<GeometryChangedEvent>(
                OnGeometryChanged);
            pageRoot.RegisterCallback<KeyDownEvent>(OnSearchKeyDown);
            periodicRefresh = pageRoot.schedule
                .Execute(() => Refresh(false))
                .Every(SnapshotIntervalMilliseconds);
            Subscribe();
            Refresh(false);
        }

        private void OnEnable()
        {
            selectedArea = (DeucarianControlCenterArea)DeucarianEditorProjectPreferences.GetInt("control-center.area", (int)selectedArea);
            searchQuery = DeucarianEditorProjectPreferences.GetString("control-center.search", searchQuery);
            Subscribe();
        }

        private void OnDisable()
        {
            navigation?.Dispose();
            navigation = null;
            DeucarianEditorProjectPreferences.SetInt("control-center.area", (int)selectedArea);
            DeucarianEditorProjectPreferences.SetString("control-center.search", searchQuery);
            Unsubscribe();
            DisposeVisualTree();
        }

        private void Subscribe()
        {
            if (subscribed)
            {
                return;
            }

            DeucarianControlCenterRegistry.Changed += QueueRefresh;
            DeucarianToolRegistry.Changed += QueueRefresh;
            DeucarianProjectValidationRegistry.Changed += QueueRefresh;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            DeucarianControlCenterRegistry.Changed -= QueueRefresh;
            DeucarianToolRegistry.Changed -= QueueRefresh;
            DeucarianProjectValidationRegistry.Changed -= QueueRefresh;
            subscribed = false;
        }

        private void QueueRefresh()
        {
            if (refreshQueued || pageRoot == null)
            {
                return;
            }

            refreshQueued = true;
            pageRoot.schedule.Execute(() =>
            {
                refreshQueued = false;
                Refresh(false);
            }).StartingIn(100);
        }

        private void Refresh(bool explicitRefresh)
        {
            snapshot = DeucarianControlCenterSnapshotBuilder.Capture(
                explicitRefresh);
            if (!ContainsArea(selectedArea))
            {
                selectedArea = DeucarianControlCenterArea.Overview;
                focusedTargetId = null;
            }

            Render();
        }

        private bool ContainsArea(DeucarianControlCenterArea area)
        {
            if (snapshot == null)
            {
                return false;
            }

            foreach (DeucarianControlCenterArea candidate in snapshot.Areas)
            {
                if (candidate == area)
                {
                    return true;
                }
            }

            return false;
        }

        private void Navigate(
            DeucarianControlCenterArea area,
            string targetId)
        {
            showSettings = false;
            selectedArea = area;
            focusedTargetId = targetId;
            SetSearch(string.Empty);
            Render();
        }

        private void SetSearch(string value)
        {
            searchQuery = value ?? string.Empty;
            if (searchField != null)
            {
                searchField.SetValueWithoutNotify(searchQuery);
            }

            Render();
        }

        private void Render()
        {
            if (view == null || snapshot == null)
            {
                return;
            }

            DeucarianEditorWorkspaceControls.Show(view.Root, !showSettings);
            DeucarianEditorWorkspaceControls.Show(settingsPage, showSettings);
            workspace.Root.Q("workspace-project-settings")?.EnableInClassList("dw-selected", showSettings);
            if (showSettings)
            {
                workspace.Root.EnableInClassList("dw-overview-page", false);
                workspace.Title.text = "Project settings";
                workspace.Subtitle.text = "Set editor appearance, scale and project preferences.";
                workspace.SelectNavigation("settings");
                DeucarianEditorWorkspaceControls.Show(workspace.PageActions, false);
                DeucarianEditorWorkspaceControls.Show(workspace.Footer, false);
                return;
            }

            view.Render(
                snapshot,
                selectedArea,
                focusedTargetId,
                searchQuery);
            workspace.Root.EnableInClassList("dw-overview-page", selectedArea == DeucarianControlCenterArea.Overview && string.IsNullOrWhiteSpace(searchQuery));
            bool overview = selectedArea == DeucarianControlCenterArea.Overview && string.IsNullOrWhiteSpace(searchQuery);
            bool advanced = selectedArea == DeucarianControlCenterArea.Project || selectedArea == DeucarianControlCenterArea.Developer;
            workspace.Title.text = advanced ? "Advanced" : "Control Center";
            workspace.Subtitle.text = advanced ? "Review package checks and find advanced project tools." : "Review project readiness and open installed Deucarian tools.";
            workspace.SelectNavigation(advanced ? "advanced" : DeucarianToolIds.ControlCenter);
            DeucarianEditorWorkspaceControls.Show(workspace.PageActions, !overview && !advanced);
            DeucarianEditorWorkspaceControls.Show(workspace.Footer, !overview && !advanced);
            if (summary != null)
            {
                summary.text = snapshot.CapturedAtUtc.ToLocalTime()
                    .ToString("'Updated' HH:mm:ss");
            }
        }

        private void ConfigureFooter()
        {
            workspace.FooterLeading.text = "Local project · refreshes automatically";
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            view?.SetLayoutMode(DeucarianEditorResponsiveLayout.ResolveMode(workspace?.Content.resolvedStyle.width ?? evt.newRect.width));
        }

        private void DisposeVisualTree()
        {
            periodicRefresh?.Pause();
            periodicRefresh = null;
            pageRoot?.UnregisterCallback<GeometryChangedEvent>(
                OnGeometryChanged);
            pageRoot?.UnregisterCallback<KeyDownEvent>(OnSearchKeyDown);
            view?.Dispose();
            view = null;
            workspace?.Dispose();
            workspace = null;
        }

        private void OnSearchKeyDown(KeyDownEvent evt)
        {
            if ((evt.ctrlKey || evt.commandKey) && (evt.keyCode == KeyCode.K || evt.keyCode == KeyCode.F))
            {
                searchField?.Focus();
                searchField?.SelectAll();
            }
            else if (string.IsNullOrWhiteSpace(searchQuery)) return;
            else if (!(pageRoot.focusController?.focusedElement is VisualElement focused) ||
                (focused != searchField && !searchField.Contains(focused))) return;
            else if (evt.keyCode == KeyCode.DownArrow) view?.MoveSearchSelection(1);
            else if (evt.keyCode == KeyCode.UpArrow) view?.MoveSearchSelection(-1);
            else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) view?.OpenSelectedSearchResult();
            else if (evt.keyCode == KeyCode.Escape) SetSearch(string.Empty);
            else return;
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }
}
