using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Routes from the owning visual tree; no global active-window lookup.</summary>
    public static class DeucarianEditorNavigation
    {
        public static void Open(VisualElement source, string toolId, string route = null)
        {
            if (source == null) { DeucarianEditorToolWindow.Open(toolId, route); return; }
            DeucarianEditorNavigateEvent.Send(source, toolId, route);
        }
    }
}
