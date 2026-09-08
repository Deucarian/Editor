using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorWorkbenchDrawer
    {
        internal DeucarianEditorWorkbenchDrawer(VisualElement root, ScrollView scrollView, VisualElement content)
        {
            Root = root;
            ScrollView = scrollView;
            Content = content;
        }

        public VisualElement Root { get; }
        public ScrollView ScrollView { get; }
        public VisualElement Content { get; }
    }
}
