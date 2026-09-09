using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Composes an existing IMGUI workflow into the shared, window-local workspace.</summary>
    public static class DeucarianEditorImGuiPage
    {
        public static IDeucarianEditorPage Create(string toolId, Action draw, Action dispose = null)
        {
            var root = new VisualElement();
            var workspace = new DeucarianEditorWorkspace(root, Application.productName);
            if (DeucarianToolRegistry.TryGet(toolId, out var tool))
            {
                workspace.Title.text = tool.DisplayName;
                workspace.Subtitle.text = tool.Description;
            }
            DeucarianEditorWorkspaceNavigation.Populate(workspace, toolId);
            DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);
            DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
            var container = new IMGUIContainer(draw) { name = "workspace-imgui-content" };
            container.style.flexGrow = 1;
            container.style.minHeight = 0;
            workspace.Content.Add(container);
            return new DeucarianEditorPage(root, update: _ => container.MarkDirtyRepaint(),
                dispose: () => { try { dispose?.Invoke(); } finally { workspace.Dispose(); } });
        }

        public static IDeucarianEditorPage Create<T>(string toolId, Action<T> draw,
            Action<T> deactivate = null, Action<T, string> activate = null,
            Action<T, VisualElement> bindNavigation = null) where T : EditorWindow
        {
            if (draw == null) throw new ArgumentNullException(nameof(draw));
            IMGUIContainer container = null;
            return DeucarianEditorWorkspacePage.Create<T>(toolId, (controller, content) =>
            {
                container = new IMGUIContainer(() =>
                {
                    if (container.resolvedStyle.width > 0 && container.resolvedStyle.height > 0)
                        controller.position = new Rect(0, 0, container.resolvedStyle.width, container.resolvedStyle.height);
                    draw(controller);
                }) { name = "workspace-imgui-content" };
                container.style.flexGrow = 1;
                container.style.minHeight = 0;
                content.Add(container);
                bindNavigation?.Invoke(controller, content);
            }, activate, deactivate, _ => container?.MarkDirtyRepaint());
        }
    }
}
