using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>An explicitly opened, independent workspace.</summary>
    public sealed class DeucarianEditorToolWindow : EditorWindow
    {
        [SerializeField] private string toolId;
        [SerializeField] private string route;
        private IDeucarianEditorPage initialPage;
        private DeucarianEditorPageSession session;

        public static DeucarianEditorToolWindow Open(string toolId, string route = null)
        {
            if (!DeucarianToolRegistry.TryGet(toolId, out var tool) || tool.CreatePage == null)
                return null;
            var window = CreateInstance<DeucarianEditorToolWindow>();
            window.toolId = toolId;
            window.route = route;
            window.titleContent = new GUIContent(tool.DisplayName);
            DeucarianEditorWorkspace.ConfigureWindow(window);
            window.Show();
            window.Focus();
            return window;
        }

        public void CreateGUI()
        {
            ReleasePages();
            if (!DeucarianToolRegistry.TryGet(toolId, out var tool) || tool.CreatePage == null) return;
            initialPage = tool.CreatePage();
            session = new DeucarianEditorPageSession(this, toolId,
                root => root.Add(initialPage.Root), initialPage.Activate, initialPage.Deactivate);
            initialPage.Root.style.flexGrow = 1;
            initialPage.Update(position);
            initialPage.Activate(route);
        }

        private void OnInspectorUpdate()
        {
            if (session?.ActiveToolId == toolId) { initialPage?.Update(position); Repaint(); }
        }

        private void OnDisable() => ReleasePages();
        private void ReleasePages()
        {
            try { session?.Dispose(); }
            finally
            {
                session = null;
                initialPage?.Dispose();
                initialPage = null;
            }
        }
    }
}
