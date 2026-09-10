using System;

namespace Deucarian.Editor
{
    /// <summary>A caller-owned snapshot. Selection is a review choice, never an implicit stage command.</summary>
    public sealed class DeucarianEditorChangeItem
    {
        public DeucarianEditorChangeItem(string id, string path, string status, bool staged, bool selected,
            Action inspect, Action<bool> select, string relatedPath = null, bool selectionEnabled = true)
        {
            Id = id; Path = path; Status = status; Staged = staged; Selected = selected;
            Inspect = inspect; Select = select; RelatedPath = relatedPath; SelectionEnabled = selectionEnabled;
        }

        public string Id { get; }
        public string Path { get; }
        public string Status { get; }
        public bool Staged { get; }
        public bool Selected { get; }
        public Action Inspect { get; }
        public Action<bool> Select { get; }
        public string RelatedPath { get; }
        public bool SelectionEnabled { get; }
    }

    /// <summary>Compact, display-only history. The owner supplies sanitized revision and branch text.</summary>
    public sealed class DeucarianEditorHistoryItem
    {
        public DeucarianEditorHistoryItem(string id, string title, string detail)
        {
            Id = id; Title = title; Detail = detail;
        }

        public string Id { get; }
        public string Title { get; }
        public string Detail { get; }
    }
}
