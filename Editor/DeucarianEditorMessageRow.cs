using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>A read-only presentation row. Timing and resolution remain owned by the caller.</summary>
    public sealed class DeucarianEditorMessageRow : VisualElement
    {
        private readonly Label state;
        private readonly VisualElement progress;
        private readonly VisualElement fill;
        private readonly Button actionButton;
        private readonly bool hasAction;

        public DeucarianEditorMessageRow(string title, string body, DeucarianEditorStatus status,
            string stateLabel, string actionLabel = null, Action action = null)
        {
            AddToClassList("dw-message");
            AddToClassList("dw-message--" + status.ToString().ToLowerInvariant());
            var marker = DeucarianEditorWorkspaceControls.Region(null, "dw-message-marker");
            Add(marker);
            string iconId = status == DeucarianEditorStatus.Warning ? DeucarianEditorIconIds.Warning :
                status == DeucarianEditorStatus.Error ? DeucarianEditorIconIds.Error :
                status == DeucarianEditorStatus.Success ? DeucarianEditorIconIds.Success : DeucarianEditorIconIds.Info;
            Add(DeucarianEditorWorkspaceControls.Icon(iconId));
            var text = DeucarianEditorWorkspaceControls.Region(null, "dw-message-text");
            text.Add(DeucarianEditorWorkspaceControls.Label(title, "dw-message-title"));
            if (!string.IsNullOrEmpty(body)) text.Add(DeucarianEditorWorkspaceControls.Label(body, "dw-muted"));
            Add(text);
            var trailing = DeucarianEditorWorkspaceControls.Region(null, "dw-message-trailing");
            state = DeucarianEditorWorkspaceControls.Label(stateLabel, "dw-message-state");
            trailing.Add(state);
            progress = DeucarianEditorWorkspaceControls.Region(null, "dw-progress");
            fill = DeucarianEditorWorkspaceControls.Region(null, "dw-progress-fill");
            progress.Add(fill);
            progress.style.display = DisplayStyle.None;
            trailing.Add(progress);
            Add(trailing);
            if (!string.IsNullOrEmpty(actionLabel))
            {
                hasAction = action != null;
                actionButton = DeucarianEditorWorkspaceControls.Button(actionLabel, action);
                actionButton.SetEnabled(hasAction);
                Add(actionButton);
            }
        }

        public void SetActionEnabled(bool enabled) => actionButton?.SetEnabled(enabled && hasAction);

        public void SetProgress(string label, float? remaining)
        {
            state.text = label ?? string.Empty;
            bool valid = remaining.HasValue && !float.IsNaN(remaining.Value) && !float.IsInfinity(remaining.Value);
            progress.style.display = valid ? DisplayStyle.Flex : DisplayStyle.None;
            if (valid) fill.style.width = Length.Percent(Mathf.Clamp01(remaining.Value) * 100f);
        }
    }
}
