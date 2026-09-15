using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal static class DeucarianEditorNativeControlStyling
    {
        private static readonly ConditionalWeakTable<VisualElement, DeucarianEditorSliderFill> Fills =
            new ConditionalWeakTable<VisualElement, DeucarianEditorSliderFill>();
        internal static void Bind(VisualElement root, SerializedObject serialized)
        {
            var sliders = new Dictionary<VisualElement, DeucarianEditorSliderFill>();
            void Style()
            {
                foreach (var removed in new List<VisualElement>(sliders.Keys))
                    if (!root.Contains(removed)) sliders.Remove(removed);
                root.Query<Slider>().ForEach(slider =>
                {
                    if (slider is DeucarianEditorSlider) return;
                    if (!sliders.TryGetValue(slider, out var fill))
                    {
                        fill = Fills.GetValue(slider, _ => {
                            var created = new DeucarianEditorSliderFill(slider);
                            slider.RegisterValueChangedCallback(evt => created.Set(evt.newValue, slider.lowValue, slider.highValue, slider.direction, slider.inverted));
                            return created;
                        });
                        sliders.Add(slider, fill);
                    }
                    fill.Set(slider.value, slider.lowValue, slider.highValue, slider.direction, slider.inverted);
                });
                root.Query<SliderInt>().ForEach(slider =>
                {
                    if (slider is DeucarianEditorIntegerSlider) return;
                    if (!sliders.TryGetValue(slider, out var fill))
                    {
                        fill = Fills.GetValue(slider, _ => {
                            var created = new DeucarianEditorSliderFill(slider);
                            slider.RegisterValueChangedCallback(evt => created.Set(evt.newValue, slider.lowValue, slider.highValue, slider.direction, slider.inverted));
                            return created;
                        });
                        sliders.Add(slider, fill);
                    }
                    fill.Set(slider.value, slider.lowValue, slider.highValue, slider.direction, slider.inverted);
                });
                root.Query<Toggle>().ForEach(toggle =>
                {
                    if (toggle is DeucarianEditorSwitch || string.IsNullOrEmpty(toggle.bindingPath)) return;
                    if (!toggle.ClassListContains("dw-switch"))
                    {
                        toggle.AddToClassList("dw-switch");
                        toggle.RegisterValueChangedCallback(evt => toggle.EnableInClassList("dw-switch-on", evt.newValue));
                    }
                    toggle.EnableInClassList("dw-switch-on", toggle.value);
                });
            }
            var refresh = root.schedule.Execute(Style);
            void Queue() => refresh.ExecuteLater(0);
            root.RegisterCallback<AttachToPanelEvent>(_ => Queue());
            root.RegisterCallback<SerializedPropertyChangeEvent>(_ => Queue());
            root.Query<PropertyField>().ForEach(field => field.RegisterCallback<GeometryChangedEvent>(_ => Queue()));
            root.TrackSerializedObjectValue(serialized, _ => Queue());
            Queue();
        }
    }
}
