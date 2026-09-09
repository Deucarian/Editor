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
            var window = GetWindow<DeucarianEditorWorkspacePreviewWindow>("Editor UI Preview");
            window.minSize = new Vector2(420, 420);
            window.Show();
            window.Focus();
        }

        public void CreateGUI()
        {
            workspace?.Dispose();
            workspace = new DeucarianEditorWorkspace(rootVisualElement,
                System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(Application.dataPath)));
            new DeucarianEditorWorkspaceSpecimen(workspace).Build();
        }

        private void OnDisable()
        {
            workspace?.Dispose();
            workspace = null;
        }
    }
}
