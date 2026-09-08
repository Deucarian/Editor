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
        private DeucarianEditorWorkbench workbench;
        private DeucarianControlCenterView view;
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
                GetWindow<DeucarianControlCenterWindow>(
                    "Deucarian Control Center");
            window.selectedArea = area;
            window.focusedTargetId = null;
            window.SetSearch(string.Empty);
            window.minSize = new Vector2(420f, 360f);
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
            DisposeVisualTree();
            rootVisualElement.Clear();
            workbench = DeucarianEditorWorkbench.Create(
                rootVisualElement,
                new DeucarianEditorWorkbenchOptions
                {
                    IncludeHeader = true,
                    IncludeToolbar = true,
                    IncludeFooter = true,
                    HeaderPackageKey = "editor",
                    HeaderTitle = "Deucarian Control Center",
                    HeaderSubtitle =
                        "Project readiness, installed capabilities, and trusted shortcuts.",
                    ToolbarLayout =
                        DeucarianEditorWorkbenchToolbarLayout.Responsive
                });

            DeucarianEditorCommandBarLanes lanes =
                DeucarianEditorCommandBar.CreateLanes(workbench.Toolbar);
            searchField = DeucarianEditorSearchField.Create("Search tools · Ctrl/Cmd+K", value =>
            {
                searchQuery = value ?? string.Empty;
                Render();
            }, searchQuery);
            searchField.name = "control-center-search";
            searchField.tooltip = "Search tools and project checks. Ctrl/Cmd+K to search; arrows to choose; Enter to open.";
            searchField.style.minWidth = 200f;
            searchField.style.flexGrow = 1f;
            lanes.Leading.style.flexGrow = 1f;
            lanes.Leading.Add(searchField);
            summary = lanes.Summary;
            summary.RemoveFromHierarchy();
            Button refresh = DeucarianEditorCommandBar.CreateAction(
                DeucarianEditorIconIds.Refresh,
                "Refresh",
                () => Refresh(true),
                tooltip: "Capture a fresh bounded status snapshot.");
            refresh.name = "control-center-refresh";
            lanes.Trailing.Add(refresh);

            view = new DeucarianControlCenterView(
                Navigate,
                () => Refresh(false));
            workbench.Content.Add(view.Root);
            ConfigureFooter();
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(
                OnGeometryChanged);
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnSearchKeyDown);
            periodicRefresh = rootVisualElement.schedule
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
            if (refreshQueued || rootVisualElement == null)
            {
                return;
            }

            refreshQueued = true;
            rootVisualElement.schedule.Execute(() =>
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

            view.Render(
                snapshot,
                selectedArea,
                focusedTargetId,
                searchQuery);
            if (summary != null)
            {
                summary.text = snapshot.CapturedAtUtc.ToLocalTime()
                    .ToString("'Updated' HH:mm:ss");
            }
        }

        private void ConfigureFooter()
        {
            if (workbench.Footer == null)
            {
                return;
            }

            var label = new Label("Project status · refreshes automatically");
            label.style.color = DeucarianEditorTheme.MutedText;
            label.style.marginLeft = 10f;
            label.style.flexGrow = 1f;
            label.style.whiteSpace = WhiteSpace.Normal;
            workbench.Footer.style.flexWrap = Wrap.Wrap;
            workbench.Footer.Add(label);
            workbench.Footer.Add(summary);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            DeucarianEditorLayoutMode mode =
                workbench?.ApplyResponsiveLayout(evt.newRect.width) ??
                DeucarianEditorResponsiveLayout.ResolveMode(evt.newRect.width);
            view?.SetLayoutMode(mode);
        }

        private void DisposeVisualTree()
        {
            periodicRefresh?.Pause();
            periodicRefresh = null;
            rootVisualElement?.UnregisterCallback<GeometryChangedEvent>(
                OnGeometryChanged);
            rootVisualElement?.UnregisterCallback<KeyDownEvent>(OnSearchKeyDown);
            view?.Dispose();
            view = null;
            workbench?.Dispose();
            workbench = null;
        }

        private void OnSearchKeyDown(KeyDownEvent evt)
        {
            if ((evt.ctrlKey || evt.commandKey) && (evt.keyCode == KeyCode.K || evt.keyCode == KeyCode.F))
            {
                searchField?.Focus();
                searchField?.SelectAll();
            }
            else if (string.IsNullOrWhiteSpace(searchQuery)) return;
            else if (!(rootVisualElement.focusController?.focusedElement is VisualElement focused) ||
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
