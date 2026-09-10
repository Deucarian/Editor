using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorSwitch : Toggle
    {
        public DeucarianEditorSwitch()
        {
            AddToClassList("dw-switch");
            this.RegisterValueChangedCallback(evt => EnableInClassList("dw-switch-on", evt.newValue));
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            EnableInClassList("dw-switch-on", newValue);
        }
    }
}
