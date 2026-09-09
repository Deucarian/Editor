using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Isolated visual specimen of shared controls, not a notification service.</summary>
    public sealed class DeucarianEditorWorkspacePreviewWindow : EditorWindow
    {
        private DeucarianEditorWorkspace workspace;

        public static void Open()
        {
            var window = DeucarianEditorWindowPages.GetStandalone<DeucarianEditorWorkspacePreviewWindow>("Editor UI Preview");
            window.minSize = new Vector2(420, 420);
            window.Show();
            window.Focus();
        }

        private DeucarianEditorPageSession navigation;
        public void CreateGUI()
        {
            navigation?.Dispose();
            navigation = new DeucarianEditorPageSession(this, "deucarian.editor.workspace-preview", BuildPage);
        }

        internal static IDeucarianEditorPage CreatePage() =>
            DeucarianEditorWindowPages.Create<DeucarianEditorWorkspacePreviewWindow>((window, root) => window.BuildPage(root));

        private void BuildPage(UnityEngine.UIElements.VisualElement root)
        {
            workspace?.Dispose();
            workspace = new DeucarianEditorWorkspace(root,
                System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(Application.dataPath)));
            new DeucarianEditorWorkspaceSpecimen(workspace).Build();
        }

        private void OnDisable()
        {
            navigation?.Dispose();
            navigation = null;
            workspace?.Dispose();
            workspace = null;
        }
    }
}
