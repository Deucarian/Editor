using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Composes shared presentation around an inspector without replacing native property editing.</summary>
    public static class DeucarianEditorInspector
    {
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
