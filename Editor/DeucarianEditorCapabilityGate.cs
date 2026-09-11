using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>A visible explanation outside the disabled controls; the owning package supplies policy.</summary>
    public sealed class DeucarianEditorCapabilityGate
    {
        private readonly VisualElement controls;
        private readonly VisualElement notice;
        private readonly Func<bool> isEnabled;
        private readonly Action whenDisabled;
        private bool? previousState;
        public VisualElement Root { get; }

        public DeucarianEditorCapabilityGate(VisualElement controls, Func<bool> isEnabled,
            string title, string explanation, string actionLabel, Action openSettings, Action whenDisabled = null, string iconId = null)
        {
            this.controls = controls ?? throw new ArgumentNullException(nameof(controls));
            this.isEnabled = isEnabled ?? throw new ArgumentNullException(nameof(isEnabled));
            this.whenDisabled = whenDisabled;
            Root = DeucarianEditorWorkspaceControls.Region(null, "dw-capability-gate");
            var summary = new DeucarianEditorStatusSummary("capability-disabled");
            summary.Set(title, explanation, DeucarianEditorStatus.Info, iconId);
            notice = summary.Root;
            notice.AddToClassList("dw-capability-notice");
            var action = DeucarianEditorWorkspaceControls.Button(actionLabel, openSettings, true);
            action.name = "capability-open-settings";
            summary.Actions.Add(action);
            Root.Add(notice);
            Root.Add(controls);
            Refresh();
        }

        public void Refresh()
        {
            bool enabled = isEnabled();
            controls.SetEnabled(enabled);
            controls.EnableInClassList("dw-capability-muted", !enabled);
            DeucarianEditorWorkspaceControls.Show(notice, !enabled);
            if (!enabled && previousState != false) whenDisabled?.Invoke();
            previousState = enabled;
        }
    }
}
