using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>
    /// Canonical three-lane command-bar composition. The leading and trailing lanes
    /// own controls while the summary remains the flexible middle label.
    /// </summary>
    public sealed class DeucarianEditorCommandBarLanes
    {
        internal DeucarianEditorCommandBarLanes(
            VisualElement root,
            VisualElement leading,
            Label summary,
            VisualElement trailing)
        {
            Root = root;
            Leading = leading;
            Summary = summary;
            Trailing = trailing;
        }

        public VisualElement Root { get; }
        public VisualElement Leading { get; }
        public Label Summary { get; }
        public VisualElement Trailing { get; }
    }
}
