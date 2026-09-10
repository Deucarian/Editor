using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Composes shared presentation around an inspector without replacing native property editing.</summary>
    public static class DeucarianEditorInspector
    {
        public static VisualElement CreateToolkit(string title = null)
        {
            var root = new VisualElement { name = "deucarian-inspector" };
            root.AddToClassList("deucarian-editor");
            root.AddToClassList("deucarian-workspace-host");
            root.AddToClassList("deucarian-workspace");
            root.AddToClassList("dw-inspector");
            root.AddToClassList(DeucarianEditorTheme.CurrentClass);
            DeucarianEditorAppearance.Bind(root);
            DeucarianEditorUIResources.TryAddSharedStyleSheet(root);
            DeucarianEditorUIResources.TryAddStyleSheet(root, DeucarianEditorWorkspace.StyleSheetPath);
            DeucarianEditorUIResources.TryAddStyleSheet(root, DeucarianEditorUIResources.StylesPath + "/DeucarianFeatures.uss");
            if (!string.IsNullOrWhiteSpace(title))
                root.Add(DeucarianEditorWorkspaceControls.Label(title, "dw-section-title"));
            return root;
        }

        public static VisualElement Create(Action draw)
        {
            if (draw == null) throw new ArgumentNullException(nameof(draw));
            var root = new VisualElement { name = "deucarian-inspector" };
            root.AddToClassList("deucarian-editor");
            root.AddToClassList("deucarian-inspector");
            root.AddToClassList(DeucarianEditorTheme.CurrentClass);
            DeucarianEditorUIResources.TryAddSharedStyleSheet(root);
            root.Add(new IMGUIContainer(draw) { name = "deucarian-inspector-content" });
            return root;
        }
    }
}
