using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorChangeRows
    {
        internal const int MaximumVisibleItems = 500;
        private readonly VisualElement rows;
        private readonly Label note;
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);

        internal DeucarianEditorChangeRows(VisualElement parent)
        {
            rows = DeucarianEditorWorkspaceControls.Region("review-change-rows", "dw-review-rows");
            note = DeucarianEditorWorkspaceControls.Label("No changes to review.", "dw-muted");
            note.name = "review-change-note";
            parent.Add(rows);
            parent.Add(note);
        }

        internal void Set(IReadOnlyList<DeucarianEditorChangeItem> items, string inspectedId)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            var visible = new HashSet<string>(StringComparer.Ordinal);
            int count = Math.Min(items.Count, MaximumVisibleItems);
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || !visible.Add(item.Id))
                    throw new ArgumentException("Visible changes require unique, non-empty IDs.", nameof(items));
            }
            var focused = rows.panel?.focusController?.focusedElement as VisualElement;
            foreach (string id in new List<string>(entries.Keys))
                if (!visible.Contains(id)) { entries[id].Root.RemoveFromHierarchy(); entries.Remove(id); }
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                if (!entries.TryGetValue(item.Id, out var entry))
                {
                    entry = new Entry();
                    entries.Add(item.Id, entry);
                }
                entry.Update(item, item.Id == inspectedId);
                if (entry.Root.parent != rows || rows.IndexOf(entry.Root) != i) rows.Insert(i, entry.Root);
            }
            // Reordering a surviving row may detach it briefly; retain keyboard review focus.
            if (focused != null && rows.Contains(focused) && focused.panel != null
                && focused.panel.focusController.focusedElement != focused) focused.Focus();
            note.text = count == 0 ? "No changes to review." : items.Count > count
                ? "Showing " + count + " of " + items.Count + " changes. Narrow the selection or use an external client for the remaining files."
                : string.Empty;
            DeucarianEditorWorkspaceControls.Show(note, count == 0 || items.Count > count);
        }

        internal void Clear() { rows.Clear(); entries.Clear(); }

        private sealed class Entry
        {
            internal readonly VisualElement Root = DeucarianEditorWorkspaceControls.Region(null, "dw-review-row");
            private readonly Toggle selected;
            private readonly Button inspect;
            private readonly Label path;
            private readonly Label state;
            private readonly Label related;
            private DeucarianEditorChangeItem current;

            internal Entry()
            {
                selected = new Toggle { name = "review-select", tooltip = "Select this change for an explicit action." };
                selected.AddToClassList("dw-review-select");
                selected.RegisterValueChangedCallback(evt => current.Select?.Invoke(evt.newValue));
                Root.Add(selected);
                inspect = DeucarianEditorWorkspaceControls.Button(string.Empty, () => current.Inspect?.Invoke());
                inspect.name = "review-inspect";
                inspect.AddToClassList("dw-collection-select");
                path = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-review-path");
                state = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
                related = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
                path.enableRichText = state.enableRichText = related.enableRichText = false;
                inspect.Add(path);
                inspect.Add(state);
                inspect.Add(related);
                Root.Add(inspect);
            }

            internal void Update(DeucarianEditorChangeItem item, bool inspected)
            {
                current = item;
                Root.name = "review-change-" + item.Id;
                Root.EnableInClassList("dw-selected", inspected);
                selected.SetValueWithoutNotify(item.Selected);
                selected.SetEnabled(item.SelectionEnabled && item.Select != null);
                selected.tooltip = "Select " + Clip(item.Path) + (item.Staged ? " (staged)" : " (unstaged)");
                inspect.SetEnabled(item.Inspect != null);
                path.text = Clip(item.Path);
                state.text = (item.Staged ? "Staged" : "Unstaged") + " · " + Clip(item.Status);
                related.text = Clip(item.RelatedPath);
                DeucarianEditorWorkspaceControls.Show(related, !string.IsNullOrEmpty(item.RelatedPath));
                inspect.tooltip = path.text + " · " + state.text;
            }

            private static string Clip(string text) => string.IsNullOrEmpty(text) ? string.Empty
                : text.Length <= 1024 ? text : text.Substring(0, 1024) + "…";
        }
    }
}
