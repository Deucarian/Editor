namespace Deucarian.Editor
{
    public readonly struct DeucarianEditorStatusChip
    {
        public DeucarianEditorStatusChip(
            string label,
            DeucarianEditorStatus status,
            string tooltip = null)
        {
            Label = label ?? string.Empty;
            Status = status;
            Tooltip = tooltip ?? string.Empty;
        }

        public string Label { get; }
        public DeucarianEditorStatus Status { get; }
        public string Tooltip { get; }
    }

    public readonly struct DeucarianEditorTimelineEvent
    {
        public DeucarianEditorTimelineEvent(
            string label,
            string detail,
            bool visualAssigned,
            bool audioAssigned,
            bool enabled = true)
        {
            Label = label ?? string.Empty;
            Detail = detail ?? string.Empty;
            VisualAssigned = visualAssigned;
            AudioAssigned = audioAssigned;
            Enabled = enabled;
        }

        public string Label { get; }
        public string Detail { get; }
        public bool VisualAssigned { get; }
        public bool AudioAssigned { get; }
        public bool Enabled { get; }
    }

    public readonly struct DeucarianEditorSplitPaneWidths
    {
        public DeucarianEditorSplitPaneWidths(float left, float center, float right)
        {
            Left = left;
            Center = center;
            Right = right;
        }

        public float Left { get; }
        public float Center { get; }
        public float Right { get; }
    }
}
