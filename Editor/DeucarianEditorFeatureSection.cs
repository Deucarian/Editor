using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>A shared feature opt-in surface; the owner supplies state and commands.</summary>
    public sealed class DeucarianEditorFeatureSection
    {
        public VisualElement Root { get; }
        public VisualElement Details { get; }
        public VisualElement Actions { get; }
        public Toggle Switch { get; }
        public Label Status { get; }
        public Label Description { get; }
        private readonly Label switchLabel;

        public DeucarianEditorFeatureSection(string id, string title, string description,
            string icon, Action<bool> changed)
        {
            Root = DeucarianEditorWorkspaceControls.Region(id, "dw-feature");
            var header = DeucarianEditorWorkspaceControls.Region(null, "dw-feature-header");
            var symbol = DeucarianEditorWorkspaceControls.Icon(icon);
            symbol.AddToClassList("dw-feature-icon");
            header.Add(symbol);
            var heading = DeucarianEditorWorkspaceControls.Region(null, "dw-feature-heading");
            heading.Add(DeucarianEditorWorkspaceControls.Label(title, "dw-feature-title"));
            Description = DeucarianEditorWorkspaceControls.Label(description, "dw-feature-description");
            heading.Add(Description);
            Status = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-feature-status");
            heading.Add(Status);
            header.Add(heading);
            var switchRow = DeucarianEditorWorkspaceControls.Region(null, "dw-feature-switch-row");
            Switch = new Toggle { name = id + "-enabled", tooltip = "Use " + title.ToLowerInvariant() + " in this app" };
            Switch.AddToClassList("dw-feature-switch");
            switchLabel = DeucarianEditorWorkspaceControls.Label("Off", "dw-feature-switch-label");
            switchRow.Add(Switch);
            switchRow.Add(switchLabel);
            header.Add(switchRow);
            Root.Add(header);
            Details = DeucarianEditorWorkspaceControls.Region(id + "-details", "dw-feature-details");
            Actions = DeucarianEditorWorkspaceControls.Region(id + "-actions", "dw-feature-actions");
            Root.Add(Details);
            Root.Add(Actions);
            Switch.RegisterValueChangedCallback(evt => changed?.Invoke(evt.newValue));
            DeucarianEditorResponsiveLayout.AdaptToWidth(Root, "dw-feature-compact", 670);
        }

        public void SetState(bool enabled, string status = null)
        {
            Switch.SetValueWithoutNotify(enabled);
            switchLabel.text = enabled ? "On" : "Off";
            Switch.EnableInClassList("dw-feature-switch-on", enabled);
            Root.EnableInClassList("dw-feature-on", enabled);
            Status.text = status ?? string.Empty;
            DeucarianEditorWorkspaceControls.Show(Status, !string.IsNullOrEmpty(status));
            DeucarianEditorWorkspaceControls.Show(Details, enabled && Details.childCount > 0);
            DeucarianEditorWorkspaceControls.Show(Actions, Actions.childCount > 0);
        }

        public static VisualElement Connection(string id, string label, bool connected, string explanation)
        {
            var row = DeucarianEditorWorkspaceControls.Region(id, "dw-feature-connection");
            var icon = DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Check);
            if (!connected) icon.style.backgroundImage = StyleKeyword.None;
            icon.AddToClassList("dw-feature-connection-icon");
            row.Add(icon);
            row.Add(DeucarianEditorWorkspaceControls.Label(label));
            row.EnableInClassList("dw-connected", connected);
            row.tooltip = explanation;
            return row;
        }

        public static VisualElement Information(string text)
        {
            var row = DeucarianEditorWorkspaceControls.Region(null, "dw-feature-information");
            row.Add(DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Info));
            row.Add(DeucarianEditorWorkspaceControls.Label(text, "dw-muted"));
            return row;
        }
    }
}
