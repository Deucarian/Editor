using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorChangeHistory
    {
        internal const int MaximumVisibleItems = 20;
        private readonly Foldout foldout;
        private readonly VisualElement rows;
        private readonly Label note;
        private readonly Dictionary<string, VisualElement> entries = new Dictionary<string, VisualElement>(StringComparer.Ordinal);
        private bool panel;

        internal DeucarianEditorChangeHistory(VisualElement parent)
        {
            foldout = new Foldout { name = "review-history", text = "Recent history", value = false };
            foldout.AddToClassList("dw-foldout");
            rows = DeucarianEditorWorkspaceControls.Region("review-history-rows", "dw-review-history");
            note = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            note.name = "review-history-note";
            foldout.Add(rows);
            foldout.Add(note);
            parent.Add(foldout);
            DeucarianEditorWorkspaceControls.Show(foldout, false);
        }

        internal void UsePanel()
        {
            panel = true; foldout.value = true;
            foldout.AddToClassList("dw-review-history-panel");
            foldout.Insert(0, DeucarianEditorWorkspaceControls.Label("Recent history", "dw-section-title"));
            DeucarianEditorWorkspaceControls.Show(foldout, true);
            note.text = "Load recent history to review this repository’s commits.";
        }

        internal void Set(IReadOnlyList<DeucarianEditorHistoryItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            int count = Math.Min(items.Count, MaximumVisibleItems);
            for (int i = 0; i < count; i++)
                if (items[i] == null || string.IsNullOrWhiteSpace(items[i].Id) || !ids.Add(items[i].Id))
                    throw new ArgumentException("Visible history items require unique, non-empty IDs.", nameof(items));
            foreach (string id in new List<string>(entries.Keys))
                if (!ids.Contains(id)) { entries[id].RemoveFromHierarchy(); entries.Remove(id); }
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                if (!entries.TryGetValue(item.Id, out var row))
                {
                    row = DeucarianEditorWorkspaceControls.Region(null, "dw-review-history-row");
                    row.name = "review-history-" + item.Id;
                    row.Add(DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.GitBranch));
                    var title = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-history-title"); title.enableRichText = false; row.Add(title);
                    var detail = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted"); detail.enableRichText = false; row.Add(detail);
                    entries.Add(item.Id, row);
                }
                row.Q<Label>(className: "dw-history-title").text = Clip(item.Title);
                var detailLabel = row.Q<Label>(className: "dw-muted"); detailLabel.text = Clip(item.Detail);
                DeucarianEditorWorkspaceControls.Show(detailLabel, !string.IsNullOrWhiteSpace(item.Detail));
                if (row.parent != rows || rows.IndexOf(row) != i) rows.Insert(i, row);
            }
            foldout.text = "Recent history (" + count + ")";
            note.text = count == 0 ? "Load recent history to review this repository’s commits." : items.Count > count ? "Showing " + count + " of " + items.Count + " entries. Open the external client for full history." : string.Empty;
            DeucarianEditorWorkspaceControls.Show(note, items.Count > count || panel && count == 0);
            DeucarianEditorWorkspaceControls.Show(foldout, panel || count > 0);
        }
        private static string Clip(string value) => string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 2048 ? value : value.Substring(0, 2048) + "…";
    }
}
