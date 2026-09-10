using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Navigation preferences owned by one window, never by a package or global singleton.</summary>
    internal sealed class DeucarianEditorNavigationState
    {
        private readonly Dictionary<string, bool> expanded = new Dictionary<string, bool>(StringComparer.Ordinal);
        internal float ScrollOffset { get; set; }

        internal bool IsExpanded(string path, bool selected)
        {
            if (expanded.TryGetValue(path, out bool value)) return value;
            if (selected) expanded[path] = true;
            return selected;
        }

        internal void SetExpanded(string path, bool value) => expanded[path] = value;
    }

    internal sealed class DeucarianEditorPageHost : VisualElement
    {
        internal DeucarianEditorNavigationState NavigationState { get; } = new DeucarianEditorNavigationState();
        internal DeucarianEditorPageHost()
        {
            name = "deucarian-page-host";
            style.flexGrow = 1;
            style.minHeight = 0;
        }
    }
}
