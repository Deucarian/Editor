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
        private bool disposed;

        public DeucarianEditorPageSession(EditorWindow window, string homeId,
            Action<VisualElement> buildHome, Action<string> activateHome = null,
            Action deactivateHome = null)
        {
            this.window = window != null ? window : throw new ArgumentNullException(nameof(window));
            this.homeId = homeId ?? throw new ArgumentNullException(nameof(homeId));
            if (buildHome == null) throw new ArgumentNullException(nameof(buildHome));
            root = window.rootVisualElement;
            root.Clear();
            var homeRoot = new VisualElement { name = "deucarian-page-" + homeId };
            homeRoot.style.flexGrow = 1;
            homeRoot.style.minHeight = 0;
            buildHome(homeRoot);
            pages.Add(homeId, new DeucarianEditorPage(homeRoot, activateHome, deactivateHome));
            ActiveToolId = homeId;
            root.Add(homeRoot);
            root.RegisterCallback<DeucarianEditorNavigateEvent>(OnNavigate);
            refresh = root.schedule.Execute(Update).Every(100);
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
        }

        public string ActiveToolId { get; private set; }
        public int PageCount => pages.Count;

        public bool Navigate(string toolId, string route = null)
        {
            if (disposed || string.IsNullOrEmpty(toolId)) return false;
            bool created = false;
            if (!pages.TryGetValue(toolId, out var next))
            {
                if (!DeucarianToolRegistry.TryGet(toolId, out var descriptor) ||
                    descriptor.CreatePage == null) return false;
                next = descriptor.CreatePage();
                if (next == null) throw new InvalidOperationException("The tool did not create a page: " + toolId);
                created = true;
            }
            try
            {
                next.Update(window.position);
                next.Activate(route);
            }
            catch
            {
                if (created) next.Dispose();
                throw;
            }
            if (toolId == ActiveToolId) return true;
            var previous = pages[ActiveToolId];
            try { previous.Deactivate(); }
            catch
            {
                if (created) next.Dispose();
                throw;
            }
            if (created) pages.Add(toolId, next);
            previous.Root.RemoveFromHierarchy();
            next.Root.style.flexGrow = 1;
            next.Root.style.minHeight = 0;
            root.Add(next.Root);
            ActiveToolId = toolId;
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
            pages[ActiveToolId].Update(window.position);
            if (ActiveToolId != homeId) window.Repaint();
        }

        public void Dispose()
        {
            if (disposed) return;
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
