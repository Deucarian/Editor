using System;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorMessageData
    {
        public string Id { get; }
        public string Title { get; }
        public string Body { get; }
        public DeucarianEditorStatus Status { get; }
        public string State { get; }
        public float? Remaining { get; }
        public string ActionLabel { get; }
        public Action Action { get; }
        public bool ActionEnabled { get; }

        public DeucarianEditorMessageData(string id, string title, string body, DeucarianEditorStatus status,
            string state, float? remaining = null, string actionLabel = null, Action action = null, bool actionEnabled = true)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A stable row ID is required.", nameof(id));
            Id = id; Title = title; Body = body; Status = status; State = state;
            Remaining = remaining; ActionLabel = actionLabel; Action = action; ActionEnabled = actionEnabled;
        }
    }
}
