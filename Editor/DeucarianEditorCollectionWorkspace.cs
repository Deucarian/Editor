using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>A shared searchable list and selection details surface, without domain operations.</summary>
    public sealed class DeucarianEditorCollectionWorkspace : IDisposable
    {
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly Label empty;
        private readonly VisualElement rows;

        public DeucarianEditorCollectionWorkspace(VisualElement root, string context, string title,
            string description, string selectedTool, string searchPrompt)
        {
            Workspace = new DeucarianEditorWorkspace(root, context);
            Workspace.Title.text = title;
            Workspace.Subtitle.text = description;
            Workspace.SetSearchPrompt(searchPrompt);
            DeucarianEditorWorkspaceNavigation.Populate(Workspace, selectedTool, filterNavigation: false);
            var list = DeucarianEditorWorkspaceControls.Scroll("workspace-collection");
            rows = DeucarianEditorWorkspaceControls.Region("workspace-collection-rows", "dw-collection-rows");
            list.Add(rows);
            empty = DeucarianEditorWorkspaceControls.Label("Nothing to show.", "dw-empty");
            list.Add(empty);
            Details = DeucarianEditorWorkspaceControls.Scroll("workspace-details");
            var split = DeucarianEditorWorkspaceControls.Split(list, Details);
            Collection = split;
            split.AddToClassList("dw-collection-split");
            Workspace.Content.Add(split);
        }

        public DeucarianEditorWorkspace Workspace { get; }
        public VisualElement Details { get; }
        public VisualElement Collection { get; }

        public void UsePanels(bool filtersInList = false)
        {
            Collection.AddToClassList("dw-collection-panels");
            if (filtersInList)
            {
                var list = Collection.Q<ScrollView>("workspace-collection");
                list.Insert(0, Workspace.Scope);
                Collection.AddToClassList("dw-collection-local-scope");
            }
        }

        public void SetItems(IReadOnlyList<DeucarianEditorCollectionItem> items, string selectedId, string emptyText)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in items)
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || !ids.Add(item.Id))
                    throw new ArgumentException("Items require unique non-empty IDs.", nameof(items));

            var removed = new List<string>();
            foreach (var entry in entries)
                if (!ids.Contains(entry.Key)) removed.Add(entry.Key);
            foreach (string id in removed) { entries[id].Root.RemoveFromHierarchy(); entries.Remove(id); }
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!entries.TryGetValue(item.Id, out var entry))
                {
                    entry = new Entry();
                    entries.Add(item.Id, entry);
                }
                entry.Update(item, item.Id == selectedId);
                if (entry.Root.parent != rows || rows.IndexOf(entry.Root) != i) rows.Insert(i, entry.Root);
            }
            empty.text = emptyText ?? "Nothing to show.";
            DeucarianEditorWorkspaceControls.Show(empty, items.Count == 0);
        }

        public void Dispose() { entries.Clear(); Workspace.Dispose(); }

        private sealed class Entry
        {
            internal readonly VisualElement Root = DeucarianEditorWorkspaceControls.Region(null, "dw-collection-row");
            private readonly Button select;
            private readonly Button action;
            private readonly Label title;
            private readonly Label subtitle;
            private readonly Label status;
            private readonly VisualElement icon;
            private readonly VisualElement glyph;
            private readonly VisualElement actionIcon;
            private DeucarianEditorCollectionItem current;

            internal Entry()
            {
                select = DeucarianEditorWorkspaceControls.Button(string.Empty, () => current.Select?.Invoke());
                select.AddToClassList("dw-collection-select");
                icon = DeucarianEditorWorkspaceControls.Region(null, "dw-collection-icon");
                glyph = DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Document);
                icon.Add(glyph);
                select.Add(icon);
                var copy = DeucarianEditorWorkspaceControls.Region(null, "dw-collection-copy");
                title = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-message-title");
                subtitle = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
                status = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-collection-status");
                copy.Add(title);
                copy.Add(subtitle);
                select.Add(copy);
                Root.Add(select);
                Root.Add(status);
                action = DeucarianEditorWorkspaceControls.Button(string.Empty, () => current.Execute?.Invoke());
                action.AddToClassList("dw-collection-action");
                actionIcon = DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Play);
                action.Add(actionIcon);
                Root.Add(action);
            }

            internal void Update(DeucarianEditorCollectionItem item, bool selected)
            {
                current = item;
                Root.name = "workspace-item-" + item.Id;
                Root.EnableInClassList("dw-selected", selected);
                title.text = item.Title;
                subtitle.text = item.Description;
                DeucarianEditorWorkspaceControls.Show(subtitle, !string.IsNullOrWhiteSpace(item.Description));
                status.text = item.Status;
                bool iconAction = !string.IsNullOrEmpty(item.ActionIconId);
                action.text = iconAction ? string.Empty : item.ActionLabel;
                action.tooltip = item.ActionLabel + " · " + item.Title;
                action.EnableInClassList("dw-round-action", iconAction);
                if (iconAction) actionIcon.style.backgroundImage = new StyleBackground(DeucarianEditorIcons.GetIcon(item.ActionIconId));
                DeucarianEditorWorkspaceControls.Show(actionIcon, iconAction);
                DeucarianEditorWorkspaceControls.Show(icon, !string.IsNullOrEmpty(item.IconId));
                if (!string.IsNullOrEmpty(item.IconId)) glyph.style.backgroundImage = new StyleBackground(DeucarianEditorIcons.GetIcon(item.IconId));
                DeucarianEditorWorkspaceControls.Show(status, !string.IsNullOrEmpty(item.Status));
                action.SetEnabled(item.ActionEnabled);
                DeucarianEditorWorkspaceControls.Show(action, item.Execute != null);
                select.tooltip = item.Title + " · " + item.Description;
            }
        }
    }
}
