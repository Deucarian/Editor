using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>A single, prominent state and optional next action.</summary>
    public sealed class DeucarianEditorStatusSummary
    {
        public VisualElement Root { get; }
        public Label Title { get; }
        public Label Description { get; }
        public VisualElement Actions { get; }
        private readonly VisualElement icon;

        public DeucarianEditorStatusSummary(string id)
        {
            Root = DeucarianEditorWorkspaceControls.Region(id, "dw-overview-focus");
            icon = DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Info);
            Root.Add(icon);
            var copy = DeucarianEditorWorkspaceControls.Region(null, "dw-focus-text");
            Title = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-focus-title");
            Description = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-focus-description");
            copy.Add(Title); copy.Add(Description); Root.Add(copy);
            Actions = DeucarianEditorWorkspaceControls.Region(null, "dw-focus-actions");
            Root.Add(Actions);
            DeucarianEditorResponsiveLayout.AdaptToWidth(Root, "dw-focus-stacked", 920);
        }

        public void Set(string title, string description, DeucarianEditorStatus status, string iconId = null)
        {
            Title.text = title; Description.text = description;
            foreach (string value in new[] { "success", "warning", "error", "info" })
                Root.EnableInClassList("dw-focus--" + value, value == status.ToString().ToLowerInvariant());
            string id = status == DeucarianEditorStatus.Success ? DeucarianEditorIconIds.Success
                : status == DeucarianEditorStatus.Error ? DeucarianEditorIconIds.Error
                : status == DeucarianEditorStatus.Warning ? DeucarianEditorIconIds.Warning : DeucarianEditorIconIds.Info;
            icon.style.backgroundImage = new StyleBackground(DeucarianEditorIcons.GetIcon(iconId ?? id));
        }
    }
}
