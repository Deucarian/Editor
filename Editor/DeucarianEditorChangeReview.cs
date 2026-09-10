using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Shared review presentation inside an existing workspace. The caller owns commands and state.</summary>
    public sealed class DeucarianEditorChangeReview : IDisposable
    {
        public static int MaximumVisibleChanges => DeucarianEditorChangeRows.MaximumVisibleItems;

        private readonly DeucarianEditorChangeRows changes;
        private readonly DeucarianEditorChangeDiff diff;
        private readonly DeucarianEditorChangeHistory history;
        private readonly Label summary;
        private bool disposed;

        public DeucarianEditorChangeReview(VisualElement parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Root = DeucarianEditorWorkspaceControls.Scroll("review-scroll");
            Root.AddToClassList("dw-review");
            parent.Add(Root);
            Context = new DeucarianEditorWorkspaceForm(AddRegion("review-context", "dw-review-context"));
            Actions = new DeucarianEditorWorkspaceForm(AddRegion("review-actions", "dw-actions"));
            summary = DeucarianEditorWorkspaceControls.Label("Select changes, then choose an action.", "dw-muted");
            summary.name = "review-summary";
            summary.enableRichText = false;
            Root.Add(summary);
            var list = DeucarianEditorWorkspaceControls.Region("review-list", "dw-review-list");
            list.Add(DeucarianEditorWorkspaceControls.Label("Changes", "dw-section-title"));
            var listScroll = DeucarianEditorWorkspaceControls.Scroll("review-list-scroll");
            list.Add(listScroll);
            changes = new DeucarianEditorChangeRows(listScroll);
            var details = DeucarianEditorWorkspaceControls.Region("review-diff", "dw-review-diff");
            diff = new DeucarianEditorChangeDiff(details);
            var split = DeucarianEditorWorkspaceControls.Split(list, details);
            split.name = "review-split";
            split.AddToClassList("dw-review-split");
            Root.Add(split);
            Commit = new DeucarianEditorWorkspaceForm(AddRegion("review-commit", "dw-review-commit"));
            history = new DeucarianEditorChangeHistory(Root);
        }

        public VisualElement Root { get; }
        public DeucarianEditorWorkspaceForm Context { get; }
        public DeucarianEditorWorkspaceForm Actions { get; }
        public DeucarianEditorWorkspaceForm Commit { get; }

        public void SetChanges(IReadOnlyList<DeucarianEditorChangeItem> items, string inspectedId)
        {
            ThrowIfDisposed();
            changes.Set(items, inspectedId);
        }

        public void SetSummary(string text)
        {
            ThrowIfDisposed();
            summary.text = text ?? string.Empty;
        }

        public void SetDiff(string title, string content, bool binary = false, bool truncated = false)
        {
            ThrowIfDisposed();
            diff.Set(title, content, binary, truncated);
        }

        public void SetHistory(IReadOnlyList<DeucarianEditorHistoryItem> items)
        {
            ThrowIfDisposed();
            history.Set(items);
        }

        public void RefreshForms()
        {
            ThrowIfDisposed();
            Context.Refresh(); Actions.Refresh(); Commit.Refresh();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Root.SetEnabled(false);
            Root.RemoveFromHierarchy();
            changes.Clear();
        }

        private VisualElement AddRegion(string name, string style)
        {
            var region = DeucarianEditorWorkspaceControls.Region(name, style);
            Root.Add(region);
            return region;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(DeucarianEditorChangeReview));
        }
    }
}
