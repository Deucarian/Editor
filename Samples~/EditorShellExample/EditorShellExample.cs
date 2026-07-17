using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Samples
{
    /// <summary>
    /// Opens a minimal package tool built from the shared Deucarian editor shell.
    /// </summary>
    public sealed class EditorShellExampleWindow : EditorWindow
    {
        [MenuItem("Tools/Deucarian/Samples/Editor Shell Example")]
        private static void Open()
        {
            EditorShellExampleWindow window = GetWindow<EditorShellExampleWindow>();
            window.titleContent = new GUIContent("Editor Shell Example");
            window.minSize = new Vector2(420f, 260f);
            window.Show();
        }

        /// <summary>
        /// Composes the example when Unity creates the window UI.
        /// </summary>
        public void CreateGUI()
        {
            rootVisualElement.Add(EditorShellExampleView.Create());
        }
    }

    /// <summary>
    /// Builds the sample view separately so packages can follow the same testable composition pattern.
    /// </summary>
    public static class EditorShellExampleView
    {
        /// <summary>
        /// Creates a complete shell with a shared package header and one content panel.
        /// </summary>
        public static VisualElement Create()
        {
            VisualElement root = new VisualElement { name = "editor-shell-example" };
            VisualElement content = DeucarianEditorVisualShell.CreateWindowShell(root);
            content.Add(DeucarianEditorVisualShell.CreateHeader(
                "Editor Shell Example",
                "Shared chrome with package-owned content"));

            VisualElement panel = DeucarianEditorVisualShell.CreatePanel();
            panel.name = "editor-shell-example-panel";
            panel.Add(new Label("Ready to add package-specific controls.")
            {
                name = "editor-shell-example-status"
            });
            content.Add(panel);

            return root;
        }
    }
}
