using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Compact Project Settings presentation. Does not create Control Center navigation or a second scale control.</summary>
    public sealed class DeucarianEditorProjectSettingsPage : IDisposable
    {
        public VisualElement Root { get; }
        public VisualElement Content { get; }

        public DeucarianEditorProjectSettingsPage(VisualElement host, string title, string description)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            Root = DeucarianEditorInspector.CreateToolkit();
#if UNITY_6000_0_OR_NEWER
            // Unity 6 ATG can resolve custom fonts on a worker thread in the native Settings host.
            Root.style.unityTextGenerator = UnityEngine.TextGeneratorType.Standard;
#endif
            Root.name = "deucarian-project-settings";
            Root.AddToClassList("dw-project-settings");
            DeucarianEditorUIResources.TryAddStyleSheet(Root,
                DeucarianEditorUIResources.StylesPath + "/DeucarianProjectSettings.uss");
            host.Add(Root);
            var scroll = DeucarianEditorWorkspaceControls.Scroll("project-settings-scroll");
            Root.Add(scroll);
            var page = DeucarianEditorWorkspaceControls.Region("project-settings-page", "dw-project-settings-page");
            scroll.Add(page);
            page.Add(DeucarianEditorWorkspaceControls.Label(title, "dw-project-settings-title"));
            if (!string.IsNullOrWhiteSpace(description))
                page.Add(DeucarianEditorWorkspaceControls.Label(description, "dw-project-settings-description"));
            Content = DeucarianEditorWorkspaceControls.Region("project-settings-content", "dw-project-settings-content");
            page.Add(Content);
        }

        public void Dispose() => Root.RemoveFromHierarchy();
    }
}
