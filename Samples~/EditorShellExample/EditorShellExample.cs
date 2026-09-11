using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Samples
{
    /// <summary>Minimal package tool built from the shared editor shell.</summary>
    public sealed class EditorShellExampleWindow : EditorWindow
    {
        private DeucarianEditorPageSession session;
        public static void Open()
        {
            EditorShellExampleWindow window =
                GetWindow<EditorShellExampleWindow>();
            window.titleContent = new GUIContent("Editor Shell Example");
            DeucarianEditorWorkspace.ConfigureWindow(window);
            window.Show();
        }

        public void CreateGUI()
        {
            session?.Dispose();
            session = new DeucarianEditorPageSession(this, EditorShellExampleView.ToolId, EditorShellExampleView.CreatePage());
        }

        private void OnDisable() { session?.Dispose(); session = null; }
    }

    /// <summary>Builds the sample view as a testable composition.</summary>
    public static class EditorShellExampleView
    {
        public const string ToolId = "deucarian.samples.editor-shell-example";

        public static VisualElement Create()
        {
            var page = CreatePage();
            page.Root.RegisterCallback<DetachFromPanelEvent>(_ => page.Dispose());
            return page.Root;
        }

        public static IDeucarianEditorPage CreatePage()
        {
            VisualElement root = new VisualElement
            {
                name = "editor-shell-example"
            };
            var workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "Editor shell example";
            workspace.Subtitle.text = "One action, with a preview you can change.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, ToolId);
            var scroll = DeucarianEditorWorkspaceControls.Scroll("example-content");
            workspace.Content.Add(scroll);
            var section = new DeucarianEditorFeatureSection("editor-shell-example-panel", "Try a label",
                "Your draft stays here when you switch tools.", DeucarianEditorIconIds.Sample);
            scroll.Add(section.Root);
            string draft = "Hello";
            var form = new DeucarianEditorWorkspaceForm(section.Details);
            form.Text("example-label", "Label", () => draft, value => draft = value);
            var preview = DeucarianEditorWorkspaceControls.Label("Hello", "dw-section-title");
            preview.name = "editor-shell-example-status";
            section.Details.Add(preview);
            section.Actions.Add(DeucarianEditorWorkspaceControls.Button("Update preview", () => preview.text = draft, true));
            return new DeucarianEditorPage(root, dispose: workspace.Dispose);
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
                    EditorShellExampleView.ToolId,
                    "Editor Shell Example",
                    "Open the imported shared editor-shell sample.",
                    DeucarianControlCenterArea.Developer,
                    EditorShellExampleWindow.Open,
                    "com.deucarian.editor",
                    DeucarianEditorIconIds.Sample,
                    new[] { "sample", "shell", "example" },
                    1000, createPage: EditorShellExampleView.CreatePage,
                    navigationPath: "Developer", navigationLabel: "Shell example"));
        }
    }
}
