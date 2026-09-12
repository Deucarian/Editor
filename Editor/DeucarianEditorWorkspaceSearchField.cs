using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorWorkspaceSearchField : TextField
    {
        private readonly Label placeholder;

        internal DeucarianEditorWorkspaceSearchField(Label placeholder)
        {
            this.placeholder = placeholder;
            this.RegisterValueChangedCallback(_ => RefreshPlaceholder());
            RegisterCallback<AttachToPanelEvent>(_ => RefreshPlaceholder());
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RefreshPlaceholder();
        }

        private void RefreshPlaceholder() =>
            DeucarianEditorWorkspaceControls.Show(placeholder, string.IsNullOrEmpty(value));
    }
}
