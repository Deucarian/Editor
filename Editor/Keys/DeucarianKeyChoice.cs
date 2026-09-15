using System;

namespace Deucarian.Editor
{
    /// <summary>A domain-owned definition projected into the shared Inspector picker.</summary>
    public sealed class DeucarianKeyChoice
    {
        public DeucarianKeyChoice(string id, string label)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A definition needs a non-empty identity.", nameof(id));
            Id = id;
            Label = string.IsNullOrWhiteSpace(label) ? id : label;
        }

        public string Id { get; }
        public string Label { get; }
    }
}
