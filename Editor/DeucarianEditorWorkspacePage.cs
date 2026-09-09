using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Hosts a window controller's domain content inside the shared workspace.</summary>
    public static class DeucarianEditorWorkspacePage
    {
        public static IDeucarianEditorPage Create<T>(string toolId, Action<T, VisualElement> build,
            Action<T, string> activate = null, Action<T> deactivate = null,
            Action<T> update = null) where T : EditorWindow
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            DeucarianEditorWorkspace workspace = null;
            try
            {
                var page = DeucarianEditorWindowPages.Create<T>((controller, root) =>
                {
                    workspace = new DeucarianEditorWorkspace(root, Application.productName);
                    if (DeucarianToolRegistry.TryGet(toolId, out var tool))
                    {
                        workspace.Title.text = tool.DisplayName;
                        workspace.Subtitle.text = tool.Description;
                    }
                    DeucarianEditorWorkspaceNavigation.Populate(workspace, toolId);
                    DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);
                    DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
                    build(controller, workspace.Content);
                }, activate, deactivate, update);
                return new DeucarianEditorPage(page.Root, page.Activate, page.Deactivate, page.Update,
                    () => { try { page.Dispose(); } finally { workspace?.Dispose(); } });
            }
            catch
            {
                workspace?.Dispose();
                throw;
            }
        }
    }
}
