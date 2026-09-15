using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Isolated visual specimen of shared controls, not a notification service.</summary>
    public sealed class DeucarianEditorWorkspacePreviewWindow : EditorWindow
    {
        private DeucarianEditorWorkspace workspace;
        private DeucarianEditorWorkspaceSpecimen specimen;

        public static void Open()
        {
            var window = DeucarianEditorWindowPages.GetStandalone<DeucarianEditorWorkspacePreviewWindow>("Editor Component Gallery");
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
            specimen?.Dispose();
            workspace?.Dispose();
            workspace = new DeucarianEditorWorkspace(root,
                System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(Application.dataPath)));
            specimen = new DeucarianEditorWorkspaceSpecimen(workspace);
            specimen.Build();
        }

        private void OnDisable()
        {
            specimen?.Dispose(); specimen = null;
            navigation?.Dispose();
            navigation = null;
            workspace?.Dispose();
            workspace = null;
        }
    }
}
