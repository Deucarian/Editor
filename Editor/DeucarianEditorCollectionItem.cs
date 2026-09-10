using System;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorCollectionItem
    {
        public DeucarianEditorCollectionItem(string id, string title, string description, string status,
            Action select, string actionLabel = null, Action execute = null, bool actionEnabled = true)
        {
            Id = id; Title = title; Description = description; Status = status; Select = select;
            ActionLabel = actionLabel; Execute = execute; ActionEnabled = actionEnabled;
        }
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Status { get; }
        public Action Select { get; }
        public string ActionLabel { get; }
        public Action Execute { get; }
        public bool ActionEnabled { get; }
    }
}
