using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorStepper : BaseField<int>
    {
        private readonly int minimum;
        private readonly int maximum;
        private readonly IntegerField input;
        private readonly Button decrease;
        private readonly Button increase;

        public DeucarianEditorStepper(int minimum, int maximum) : base(null, new VisualElement())
        {
            this.minimum = minimum;
            this.maximum = Mathf.Max(minimum, maximum);
            AddToClassList("dw-stepper");
            var controls = this.Q(className: inputUssClassName);
            decrease = DeucarianEditorWorkspaceControls.Button("−", () => value--);
            decrease.tooltip = "Decrease";
            input = new IntegerField { isDelayed = true };
            input.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                value = evt.newValue;
                input.SetValueWithoutNotify(value);
            });
            increase = DeucarianEditorWorkspaceControls.Button("+", () => value++);
            increase.tooltip = "Increase";
            controls.Add(decrease); controls.Add(input); controls.Add(increase);
            SetValueWithoutNotify(minimum);
        }

        public override int value { get => base.value; set => base.value = Mathf.Clamp(value, minimum, maximum); }
        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(Mathf.Clamp(newValue, minimum, maximum));
            input?.SetValueWithoutNotify(base.value);
            decrease?.SetEnabled(base.value > minimum);
            increase?.SetEnabled(base.value < maximum);
        }
    }
}
