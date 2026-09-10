using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorSlider : Slider
    {
        private readonly DeucarianEditorSliderFill fill;

        public DeucarianEditorSlider(float minimum, float maximum) : base(minimum, maximum)
        {
            fill = new DeucarianEditorSliderFill(this);
            RegisterValueChangedCallback(_ => RefreshFill());
            RegisterCallback<GeometryChangedEvent>(_ => RefreshFill());
            RefreshFill();
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RefreshFill();
        }

        private void RefreshFill() => fill?.Set(value, lowValue, highValue, direction, inverted);
    }

    public sealed class DeucarianEditorIntegerSlider : SliderInt
    {
        private readonly DeucarianEditorSliderFill fill;

        public DeucarianEditorIntegerSlider(int minimum, int maximum) : base(minimum, maximum)
        {
            fill = new DeucarianEditorSliderFill(this);
            RegisterValueChangedCallback(_ => RefreshFill());
            RegisterCallback<GeometryChangedEvent>(_ => RefreshFill());
            RefreshFill();
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RefreshFill();
        }

        private void RefreshFill() => fill?.Set(value, lowValue, highValue, direction, inverted);
    }

    internal sealed class DeucarianEditorSliderFill
    {
        private readonly VisualElement fill;

        internal DeucarianEditorSliderFill(VisualElement slider)
        {
            slider.AddToClassList("dw-slider");
            fill = new VisualElement { name = "slider-fill", pickingMode = PickingMode.Ignore };
            fill.AddToClassList("dw-slider-fill");
            slider.Q(className: "unity-base-slider__tracker")?.Add(fill);
        }

        internal void Set(float value, float minimum, float maximum, SliderDirection direction, bool inverted)
        {
            float percent = Mathf.InverseLerp(minimum, maximum, value) * 100;
            bool vertical = direction == SliderDirection.Vertical;
            fill.style.width = Length.Percent(vertical ? 100 : percent);
            fill.style.height = Length.Percent(vertical ? percent : 100);
            fill.style.left = !vertical && inverted ? StyleKeyword.Auto : 0;
            fill.style.right = !vertical && inverted ? 0 : StyleKeyword.Auto;
            fill.style.top = vertical && !inverted ? StyleKeyword.Auto : 0;
            fill.style.bottom = vertical && !inverted ? 0 : StyleKeyword.Auto;
        }
    }
}
