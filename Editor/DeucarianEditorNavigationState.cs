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
            return selected;
        }

        internal void SetExpanded(string path, bool value) => expanded[path] = value;

        internal DeucarianEditorReloadSnapshot.Group[] CaptureGroups()
        {
            var result = new List<DeucarianEditorReloadSnapshot.Group>();
            foreach (var pair in expanded)
                result.Add(new DeucarianEditorReloadSnapshot.Group { path = pair.Key, expanded = pair.Value });
            return result.ToArray();
        }

        internal void Restore(DeucarianEditorReloadSnapshot snapshot)
        {
            ScrollOffset = snapshot.navigationScroll;
            foreach (var group in snapshot.groups) expanded[group.path] = group.expanded;
        }
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
