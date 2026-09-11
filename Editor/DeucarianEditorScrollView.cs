using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorScrollView : ScrollView
    {
        private IVisualElementScheduledItem reveal;

        public DeucarianEditorScrollView() : base(ScrollViewMode.Vertical)
        {
            RegisterCallback<FocusInEvent>(OnFocusIn, TrickleDown.TrickleDown);
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            var focused = evt.target as VisualElement;
            if (focused == null || !contentContainer.Contains(focused)) return;
            reveal?.Pause();
            int remaining = 4;
            reveal = schedule.Execute(() =>
            {
                if (focusController?.focusedElement != focused) { reveal.Pause(); return; }
                RevealFocused(focused);
                if (--remaining == 0) reveal.Pause();
            }).Every(0);
        }

        private void RevealFocused(VisualElement focused)
        {
            if (panel == null || focused.panel != panel || !contentContainer.Contains(focused)) return;
            var bounds = contentViewport.WorldToLocal(focused.worldBound);
            float height = contentViewport.layout.height;
            if (height <= 0 || float.IsNaN(height)) return;
            float delta = bounds.yMin < 0 ? bounds.yMin : Mathf.Max(0, bounds.yMax - height);
            if (Mathf.Abs(delta) > 0.01f)
                scrollOffset = new Vector2(scrollOffset.x, Mathf.Clamp(scrollOffset.y + delta, 0, verticalScroller.highValue));
        }
    }
}
