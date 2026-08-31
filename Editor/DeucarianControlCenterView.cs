using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed partial class DeucarianControlCenterView : IDisposable
    {
        private readonly Action<DeucarianControlCenterArea, string> navigate;
        private readonly Action refresh;
        private readonly List<VisualElement> cardHosts =
            new List<VisualElement>();
        private VisualElement layout;
        private VisualElement sidebar;
        private DeucarianEditorLayoutMode layoutMode;
    }
}
