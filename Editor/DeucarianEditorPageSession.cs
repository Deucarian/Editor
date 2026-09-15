using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Owns navigation and cached pages for exactly one native editor window.</summary>
    public sealed class DeucarianEditorPageSession : IDisposable
    {
        private readonly EditorWindow window;
        private readonly Dictionary<string, IDeucarianEditorPage> pages =
            new Dictionary<string, IDeucarianEditorPage>(StringComparer.Ordinal);
        private readonly VisualElement root;
        private readonly IVisualElementScheduledItem refresh;
        private readonly string homeId;
        private readonly UnityEngine.GUIContent homeTitle;
        private readonly DeucarianEditorPageHost pageHost;
        private readonly string reloadKey;
        private readonly DeucarianEditorReloadSnapshot reloadState;
        private readonly Dictionary<string, string> routes = new Dictionary<string, string>(StringComparer.Ordinal);
        private string pendingRestore;
        private bool disposed;
        private bool navigating;

        public DeucarianEditorPageSession(EditorWindow window, string homeId,
            Action<VisualElement> buildHome, Action<string> activateHome = null,
            Action deactivateHome = null)
            : this(window, homeId, () => CreateHome(homeId, buildHome, activateHome, deactivateHome)) { }

        public DeucarianEditorPageSession(EditorWindow window, string homeId, IDeucarianEditorPage homePage)
            : this(window, homeId, () => homePage ?? throw new ArgumentNullException(nameof(homePage))) { }

        private DeucarianEditorPageSession(EditorWindow window, string homeId, Func<IDeucarianEditorPage> createHome)
        {
            this.window = window != null ? window : throw new ArgumentNullException(nameof(window));
            this.homeId = homeId ?? throw new ArgumentNullException(nameof(homeId));
            reloadKey = DeucarianEditorReloadSnapshot.Key(window, homeId);
            reloadState = DeucarianEditorReloadSnapshot.Load(reloadKey);
            pendingRestore = reloadState.activeToolId;
            var savedHome = reloadState.Find(homeId);
            if (!string.IsNullOrEmpty(savedHome?.ownerState) && window is IDeucarianEditorReloadState owner)
                owner.RestoreReloadState(savedHome.ownerState);
            homeTitle = new UnityEngine.GUIContent(window.titleContent);
            if (DeucarianToolRegistry.TryGet(homeId, out var homeTool)) homeTitle.text = homeTool.DisplayName;
            root = window.rootVisualElement;
            root.Clear();
            pageHost = new DeucarianEditorPageHost();
            pageHost.NavigationState.Restore(reloadState);
            root.Add(pageHost);
            var home = createHome();
            try
            {
                if (!(window is IDeucarianEditorReloadState) && !string.IsNullOrEmpty(savedHome?.ownerState))
                    (home as IDeucarianEditorReloadState)?.RestoreReloadState(savedHome.ownerState);
            }
            catch
            {
                try { home.Dispose(); }
                finally { pageHost.RemoveFromHierarchy(); }
                throw;
            }
            routes[homeId] = savedHome?.route;
            home.Root.style.flexGrow = 1;
            home.Root.style.minHeight = 0;
            pages.Add(homeId, home);
            ActiveToolId = homeId;
            window.titleContent = new UnityEngine.GUIContent(homeTitle);
            pageHost.Add(home.Root);
            DeucarianEditorReloadSnapshot.RestoreScroll(savedHome, home.Root);
            root.RegisterCallback<DeucarianEditorNavigateEvent>(OnNavigate);
            refresh = root.schedule.Execute(Update).Every(100);
            root.schedule.Execute(RestoreSelection);
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
        }

        private static IDeucarianEditorPage CreateHome(string id, Action<VisualElement> build,
            Action<string> activate, Action deactivate)
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            var home = new VisualElement { name = "deucarian-page-" + id };
            build(home);
            return new DeucarianEditorPage(home, activate, deactivate);
        }

        public string ActiveToolId { get; private set; }
        public int PageCount => pages.Count;
        internal bool HasRestoredHomeState => !string.IsNullOrEmpty(reloadState.Find(homeId)?.ownerState);

        public bool Navigate(string toolId, string route = null)
        {
            if (disposed || string.IsNullOrEmpty(toolId)) return false;
            pendingRestore = null;
            if (navigating) throw new InvalidOperationException("A page transition is already in progress.");
            if (toolId == ActiveToolId && string.IsNullOrEmpty(route)) return true;
            navigating = true;
            try { return NavigateCore(toolId, route); }
            finally { navigating = false; }
        }

        private bool NavigateCore(string toolId, string route)
        {
            if (toolId == ActiveToolId)
            {
                pages[toolId].Activate(route);
                routes[toolId] = route;
                return true;
            }
            bool created = false;
            if (!pages.TryGetValue(toolId, out var next))
            {
                if (!DeucarianToolRegistry.TryGet(toolId, out var descriptor) ||
                    descriptor.CreatePage == null) return false;
                next = descriptor.CreatePage();
                if (next == null) throw new InvalidOperationException("The tool did not create a page: " + toolId);
                created = true;
                var saved = reloadState.Find(toolId);
                try
                {
                    if (!string.IsNullOrEmpty(saved?.ownerState))
                        (next as IDeucarianEditorReloadState)?.RestoreReloadState(saved.ownerState);
                }
                catch { next.Dispose(); throw; }
            }
            var previous = pages[ActiveToolId];
            try { previous.Deactivate(); }
            catch
            {
                if (created) next.Dispose();
                throw;
            }
            try
            {
                next.Update(window.position);
                next.Activate(route);
            }
            catch (Exception error)
            {
                var failures = new List<Exception> { error };
                try { next.Deactivate(); } catch (Exception cleanup) { failures.Add(cleanup); }
                if (created)
                    try { next.Dispose(); } catch (Exception cleanup) { failures.Add(cleanup); }
                try { previous.Activate(null); } catch (Exception restore) { failures.Add(restore); }
                if (failures.Count > 1) throw new AggregateException("Page activation and recovery failed.", failures);
                throw;
            }
            if (created) pages.Add(toolId, next);
            previous.Root.RemoveFromHierarchy();
            next.Root.style.flexGrow = 1;
            next.Root.style.minHeight = 0;
            pageHost.Add(next.Root);
            ActiveToolId = toolId;
            routes[toolId] = route;
            if (created) DeucarianEditorReloadSnapshot.RestoreScroll(reloadState.Find(toolId), next.Root);
            if (toolId == homeId || homeId == DeucarianToolIds.ControlCenter)
                window.titleContent = new UnityEngine.GUIContent(homeTitle);
            else if (DeucarianToolRegistry.TryGet(toolId, out var tool))
                window.titleContent = new UnityEngine.GUIContent(tool.DisplayName, homeTitle.image);
            DeucarianToolHistory.RecordOpened(toolId);
            window.Repaint();
            return true;
        }

        private void OnNavigate(DeucarianEditorNavigateEvent evt)
        {
            evt.StopPropagation();
            try
            {
                if (!Navigate(evt.ToolId, evt.Route))
                    window.ShowNotification(new UnityEngine.GUIContent("Update this package to enable in-window navigation."));
            }
            catch (Exception error)
            {
                window.ShowNotification(new UnityEngine.GUIContent("Could not open this page: " + error.Message));
            }
        }

        private void Update()
        {
            if (disposed || window == null) return;
            RestoreSelection();
            pages[ActiveToolId].Update(window.position);
            if (ActiveToolId != homeId) window.Repaint();
        }

        internal void RestoreSelection()
        {
            if (disposed || string.IsNullOrEmpty(pendingRestore)) return;
            string destination = pendingRestore;
            if (destination != homeId && (!DeucarianToolRegistry.TryGet(destination, out var tool) || tool.CreatePage == null)) return;
            pendingRestore = null;
            var saved = reloadState.Find(destination);
            // A navigation route is a command, not the owner's current selection. A
            // restored draft takes precedence over an old "select asset A" route.
            try { Navigate(destination, string.IsNullOrEmpty(saved?.ownerState) ? saved?.route : null); }
            catch { window.ShowNotification(new UnityEngine.GUIContent("Could not restore this page. Its saved draft is retained for another attempt.")); }
        }

        private void SaveReloadState()
        {
            reloadState.activeToolId = pendingRestore ?? ActiveToolId;
            reloadState.navigationScroll = pageHost.NavigationState.ScrollOffset;
            reloadState.groups = pageHost.NavigationState.CaptureGroups();
            foreach (var pair in pages)
            {
                routes.TryGetValue(pair.Key, out string route);
                try
                {
                    reloadState.Capture(pair.Key, route, pair.Value,
                        pair.Key == homeId ? window as IDeucarianEditorReloadState : null);
                }
                catch { window.ShowNotification(new UnityEngine.GUIContent("A page could not save its draft. Other pages were preserved.")); }
            }
            reloadState.Save(reloadKey);
        }

        public void Dispose()
        {
            if (disposed) return;
            try { SaveReloadState(); }
            catch { window.ShowNotification(new UnityEngine.GUIContent("Could not save the workspace state for reload.")); }
            disposed = true;
            AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
            refresh?.Pause();
            root.UnregisterCallback<DeucarianEditorNavigateEvent>(OnNavigate);
            try { pages[ActiveToolId].Deactivate(); }
            finally
            {
                var failures = new List<Exception>();
                foreach (var page in pages.Values)
                {
                    try { page.Dispose(); }
                    catch (Exception error) { failures.Add(error); }
                }
                pages.Clear();
                pageHost.RemoveFromHierarchy();
                if (failures.Count != 0) throw new AggregateException("Could not release all editor pages.", failures);
            }
        }
    }

    internal sealed class DeucarianEditorNavigateEvent : EventBase<DeucarianEditorNavigateEvent>
    {
        internal string ToolId { get; private set; }
        internal string Route { get; private set; }
        protected override void Init()
        {
            base.Init();
            bubbles = true;
            ToolId = null;
            Route = null;
        }

        internal static void Send(VisualElement source, string toolId, string route)
        {
            using (var evt = GetPooled())
            {
                evt.ToolId = toolId;
                evt.Route = route;
                evt.target = source;
                source.SendEvent(evt);
            }
        }
    }
}
