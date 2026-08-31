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
        private DeucarianControlCenterArea selectedArea =
            DeucarianControlCenterArea.Overview;
        private string focusedTargetId;
        private string searchQuery = string.Empty;
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
            window.minSize = new Vector2(560f, 400f);
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
            searchField = new TextField
            {
                name = "control-center-search",
                tooltip = "Search areas, status, tools, and actions."
            };
            searchField.style.minWidth = 170f;
            searchField.style.flexGrow = 1f;
            searchField.SetValueWithoutNotify(searchQuery);
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchQuery = evt.newValue ?? string.Empty;
                Render();
            });
            lanes.Leading.Add(searchField);
            summary = lanes.Summary;
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
            periodicRefresh = rootVisualElement.schedule
                .Execute(() => Refresh(false))
                .Every(SnapshotIntervalMilliseconds);
            Subscribe();
            Refresh(false);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
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

            var label = new Label(
                "Status is captured on demand and at a bounded 15-second interval.");
            label.style.color = DeucarianEditorTheme.MutedText;
            label.style.marginLeft = 10f;
            workbench.Footer.Add(label);
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
            view?.Dispose();
            view = null;
            workbench?.Dispose();
            workbench = null;
        }
    }
}
