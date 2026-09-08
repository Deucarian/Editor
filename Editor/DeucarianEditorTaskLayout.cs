using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Composes a focused workflow into the shared workbench; domain state stays with the caller.</summary>
    public sealed class DeucarianEditorTaskLayout : IDisposable
    {
        private readonly DeucarianEditorWorkbench workbench;
        public VisualElement Context { get; }
        public VisualElement Actions => workbench.Toolbar;
        public ScrollView Body { get; }
        public VisualElement Content { get; }
        public Foldout Preview { get; }
        public Foldout Advanced { get; }
        public Label Status { get; }

        public DeucarianEditorTaskLayout(VisualElement root, string packageKey, string title, string purpose)
        {
            workbench = DeucarianEditorWorkbench.Create(root, new DeucarianEditorWorkbenchOptions
            {
                IncludeHeader = true, IncludeToolbar = true, IncludeFooter = true,
                HeaderPackageKey = packageKey, HeaderTitle = title, HeaderSubtitle = purpose
            });
            Context = new VisualElement { name = "task-context" };
            Context.style.flexShrink = 0;
            Context.style.paddingLeft = DeucarianEditorLayoutMetrics.PageHorizontalPadding;
            Context.style.paddingRight = DeucarianEditorLayoutMetrics.PageHorizontalPadding;
            workbench.Content.Add(Context);
            Body = new ScrollView { name = "task-body" };
            Body.style.flexGrow = 1;
            Body.style.minHeight = 0;
            workbench.Content.Add(Body);
            Content = new VisualElement { name = "task-content" };
            Body.Add(Content);
            Preview = new Foldout { name = "task-preview", text = "Preview", value = true };
            Advanced = new Foldout { name = "task-advanced", text = "Advanced", value = false };
            Body.Add(Preview);
            Body.Add(Advanced);
            Status = new Label { name = "task-status" };
            Status.style.whiteSpace = WhiteSpace.Normal;
            workbench.Footer.Add(Status);
        }

        public void Dispose() => workbench.Dispose();
    }
}
