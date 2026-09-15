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
        private readonly VisualElement marker, icon;
        public Label Title { get; }
        public Label Body { get; }

        public DeucarianEditorMessageRow(string title, string body, DeucarianEditorStatus status,
            string stateLabel, string actionLabel = null, Action action = null)
        {
            AddToClassList("dw-message");
            AddToClassList("dw-message--" + status.ToString().ToLowerInvariant());
            marker = DeucarianEditorWorkspaceControls.Region(null, "dw-message-marker");
            Add(marker);
            string iconId = status == DeucarianEditorStatus.Warning ? DeucarianEditorIconIds.Warning :
                status == DeucarianEditorStatus.Error ? DeucarianEditorIconIds.Error :
                status == DeucarianEditorStatus.Success ? DeucarianEditorIconIds.Success : DeucarianEditorIconIds.Info;
            icon = DeucarianEditorWorkspaceControls.Icon(iconId);
            Add(icon);
            var text = DeucarianEditorWorkspaceControls.Region(null, "dw-message-text");
            Title = DeucarianEditorWorkspaceControls.Label(title, "dw-message-title");
            text.Add(Title);
            Body = DeucarianEditorWorkspaceControls.Label(body, "dw-muted");
            if (!string.IsNullOrEmpty(body)) text.Add(Body);
            Add(text);
            var trailing = DeucarianEditorWorkspaceControls.Region(null, "dw-message-trailing");
            state = DeucarianEditorWorkspaceControls.Label(stateLabel, "dw-message-state");
            trailing.Add(state);
            progress = DeucarianEditorWorkspaceControls.Region(null, "dw-progress");
            fill = DeucarianEditorWorkspaceControls.Region(null, "dw-progress-fill");
            progress.Add(fill);
            progress.style.display = DisplayStyle.None;
            trailing.Add(progress);
            text.Add(trailing);
            if (!string.IsNullOrEmpty(actionLabel))
            {
                hasAction = action != null;
                actionButton = DeucarianEditorWorkspaceControls.Button(actionLabel, action);
                actionButton.SetEnabled(hasAction);
                Add(actionButton);
            }
        }

        public void SetActionEnabled(bool enabled) => actionButton?.SetEnabled(enabled && hasAction);

        /// <summary>Colors a domain specimen. Editor chrome and controls remain Editor-owned.</summary>
        public void SetColors(Color surface, Color title, Color body, Color severity)
        {
            style.backgroundColor = surface;
            Title.style.color = title;
            Body.style.color = body;
            marker.style.backgroundColor = severity;
            icon.style.unityBackgroundImageTintColor = severity;
            icon.style.color = severity;
        }

        public void SetProgress(string label, float? remaining)
        {
            state.text = label ?? string.Empty;
            bool valid = remaining.HasValue && !float.IsNaN(remaining.Value) && !float.IsInfinity(remaining.Value);
            progress.style.display = valid ? DisplayStyle.Flex : DisplayStyle.None;
            if (valid) fill.style.width = Length.Percent(Mathf.Clamp01(remaining.Value) * 100f);
        }
    }
}
