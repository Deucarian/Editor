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
            var layout = new DeucarianEditorTaskLayout(root, "editor", "Editor Shell Example",
                "One clear action, visible context, optional detail.");
            root.RegisterCallback<DetachFromPanelEvent>(_ => layout.Dispose());
            layout.Context.Add(new Label("Project-local example · no runtime connection"));
            var input = new TextField("Label") { value = "Hello" };
            layout.Content.Add(input);
            var preview = new Label("Hello");
            layout.Preview.Add(preview);
            layout.Actions.Add(new Button(() => { preview.text = input.value; layout.Status.text = "Preview updated."; }) { text = "Update preview" });
            layout.Advanced.Add(new Label("Place optional package-specific controls here."));
            layout.Status.text = "Ready. Type a label and update the preview.";

            VisualElement panel =
                DeucarianEditorVisualShell.CreatePanel();
            panel.name = "editor-shell-example-panel";
            panel.Add(new Label(
                "Ready to add package-specific controls.")
            {
                name = "editor-shell-example-status"
            });
            layout.Content.Add(panel);
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
