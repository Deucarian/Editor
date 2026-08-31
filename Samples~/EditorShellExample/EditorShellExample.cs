using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Samples
{
    /// <summary>Minimal package tool built from the shared editor shell.</summary>
    public sealed class EditorShellExampleWindow : EditorWindow
    {
        public static void Open()
        {
            EditorShellExampleWindow window =
                GetWindow<EditorShellExampleWindow>();
            window.titleContent = new GUIContent("Editor Shell Example");
            window.minSize = new Vector2(420f, 260f);
            window.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Add(EditorShellExampleView.Create());
        }
    }

    /// <summary>Builds the sample view as a testable composition.</summary>
    public static class EditorShellExampleView
    {
        public static VisualElement Create()
        {
            VisualElement root = new VisualElement
            {
                name = "editor-shell-example"
            };
            VisualElement content =
                DeucarianEditorVisualShell.CreateWindowShell(root);
            content.Add(DeucarianEditorVisualShell.CreateHeader(
                "Editor Shell Example",
                "Shared chrome with package-owned content"));

            VisualElement panel =
                DeucarianEditorVisualShell.CreatePanel();
            panel.name = "editor-shell-example-panel";
            panel.Add(new Label(
                "Ready to add package-specific controls.")
            {
                name = "editor-shell-example-status"
            });
            content.Add(panel);
            return root;
        }
    }

    [InitializeOnLoad]
    internal static class EditorShellExampleRegistration
    {
        private static readonly IDisposable Registration;

        static EditorShellExampleRegistration()
        {
            Registration = DeucarianToolRegistry.Register(
                new DeucarianToolDescriptor(
                    "deucarian.samples.editor-shell-example",
                    "Editor Shell Example",
                    "Open the imported shared editor-shell sample.",
                    DeucarianControlCenterArea.Developer,
                    EditorShellExampleWindow.Open,
                    "com.deucarian.editor",
                    DeucarianEditorIconIds.Sample,
                    new[] { "sample", "shell", "example" },
                    1000));
        }
    }
}
