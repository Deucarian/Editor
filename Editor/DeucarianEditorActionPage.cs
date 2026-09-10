using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>An explicit command or reference page; opening it never executes its action.</summary>
    public static class DeucarianEditorActionPage
    {
        public static IDeucarianEditorPage Create(string toolId, string text,
            string actionLabel = null, Action action = null, bool requiresConfirmation = false)
        {
            var root = new VisualElement();
            var workspace = new DeucarianEditorWorkspace(root, Application.productName);
            if (DeucarianToolRegistry.TryGet(toolId, out var tool)) workspace.Title.text = tool.DisplayName;
            DeucarianEditorWorkspaceNavigation.Populate(workspace, toolId);
            DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);
            DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
            var scroll = DeucarianEditorWorkspaceControls.Scroll("workspace-action-content");
            var description = DeucarianEditorWorkspaceControls.Label(text ?? string.Empty, "dw-muted");
            scroll.Add(description);
            if (action != null)
                scroll.Add(DeucarianEditorWorkspaceControls.Button(actionLabel, () =>
                {
                    if (requiresConfirmation && !UnityEditor.EditorUtility.DisplayDialog(workspace.Title.text,
                        "Run '" + actionLabel + "'?", "Run", "Cancel")) return;
                    try { action(); }
                    catch (Exception error) { DeucarianEditorActionErrors.Show(actionLabel, error); }
                }));
            workspace.Content.Add(scroll);
            return new DeucarianEditorPage(root, dispose: workspace.Dispose);
        }
    }
}
