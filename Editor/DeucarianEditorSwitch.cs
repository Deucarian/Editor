using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorSwitch : Toggle
    {
        private readonly Label state;
        public DeucarianEditorSwitch()
        {
            AddToClassList("dw-switch");
            state = new Label("Off") { pickingMode = PickingMode.Ignore };
            state.AddToClassList("dw-switch-label");
            hierarchy.Add(state);
            this.RegisterValueChangedCallback(evt => ShowState(evt.newValue));
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            ShowState(newValue);
        }

        private void ShowState(bool enabled)
        {
            EnableInClassList("dw-switch-on", enabled);
            if (state != null) state.text = enabled ? "On" : "Off";
        }
    }
}
