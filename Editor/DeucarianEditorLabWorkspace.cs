using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Shared test workspace. The consumer owns targeting, lifetime, queue selection and actions.</summary>
    public sealed class DeucarianEditorLabWorkspace : IDisposable
    {
        private readonly List<VisualElement> pages = new List<VisualElement>();
        private readonly DeucarianEditorChoiceBar destinations;
        private readonly PopupField<string> targets;
        private readonly Label destinationNote;
        private readonly Label count;
        private readonly Label empty;
        private readonly Foldout overflow;
        private readonly VisualElement rows;
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
        private string[] targetIds = Array.Empty<string>();
        private readonly Action<string> selectTarget;
        private readonly Button clear;
        private readonly DeucarianEditorChoiceBar tabs;
        private bool syncing;
        public DeucarianEditorWorkspace Workspace { get; }
        public DeucarianEditorWorkspaceForm Composer { get; }
        public DeucarianEditorWorkspaceForm Appearance { get; }
        public DeucarianEditorWorkspaceForm Audio { get; }
        public VisualElement MotionPreviewRoot { get; }
        public event Action<int> TabChanged;

        public DeucarianEditorLabWorkspace(VisualElement root, string context, string title, string subtitle,
            Action clearMessages, Action<string> selectTarget)
        {
            this.selectTarget = selectTarget ?? throw new ArgumentNullException(nameof(selectTarget));
            Workspace = new DeucarianEditorWorkspace(root, context);
            Workspace.Title.text = title;
            Workspace.Subtitle.text = subtitle;
            Workspace.ContextButton.SetEnabled(false);
            tabs = new DeucarianEditorChoiceBar(new[] { "Test", "Appearance", "Audio" }, tabs: true);
            Workspace.Tabs.Add(tabs);
            tabs.Changed += SelectTab;
            Workspace.Scope.AddToClassList("dw-lab-destination");
            Workspace.Scope.Add(DeucarianEditorWorkspaceControls.Label("Destination"));
            destinations = new DeucarianEditorChoiceBar(new[] { "Editor preview", "Running app" });
            destinations.Changed += index => { if (!syncing && (index == 0 || targetIds.Length > 0)) selectTarget(index == 0 ? null : targetIds[0]); };
            Workspace.Scope.Add(destinations);
            targets = new PopupField<string>(new List<string> { "No active list" }, 0) { name = "lab-target" };
            targets.AddToClassList("dw-target-picker");
            targets.RegisterValueChangedCallback(_ => { if (!syncing && targets.index >= 0 && targets.index < targetIds.Length) selectTarget(targetIds[targets.index]); });
            Workspace.Scope.Add(targets);
            destinationNote = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            Workspace.Scope.Add(destinationNote);
            var test = AddPage();
            var form = DeucarianEditorWorkspaceControls.Region("lab-composer-scroll", "dw-lab-composer");
            form.AddToClassList("dw-lab-composer");
            var preview = DeucarianEditorWorkspaceControls.Panel("lab-preview-scroll");
            preview.AddToClassList("dw-lab-preview");
            var composerPane = DeucarianEditorWorkspaceControls.Panel("lab-composer-pane");
            composerPane.Add(form);
            var split = DeucarianEditorWorkspaceControls.Split(composerPane, preview, stackBelow: 1120);
            split.AddToClassList("dw-lab-split");
            test.Add(split);
            form.Add(DeucarianEditorWorkspaceControls.Label("New message", "dw-section-title"));
            Composer = new DeucarianEditorWorkspaceForm(form);
            var toolbar = DeucarianEditorWorkspaceControls.Region(null, "dw-preview-toolbar");
            toolbar.Add(DeucarianEditorWorkspaceControls.Label("Preview", "dw-section-title"));
            count = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            toolbar.Add(count);
            clear = DeucarianEditorWorkspaceControls.IconButton("Clear", DeucarianEditorIconIds.Remove, clearMessages);
            clear.name = "lab-clear";
            toolbar.Add(clear);
            preview.Add(toolbar);
            empty = DeucarianEditorWorkspaceControls.Label("All clear. Add a test message to begin.", "dw-empty");
            preview.Add(empty);
            rows = DeucarianEditorWorkspaceControls.Region("lab-visible-rows", "dw-message-rows");
            preview.Add(rows);
            overflow = new Foldout { text = "Overflow", value = false, name = "lab-overflow" };
            overflow.AddToClassList("dw-foldout");
            preview.Add(overflow);
            var appearance = AddPage();
            var appearanceForm = new VisualElement();
            MotionPreviewRoot = new VisualElement();
            var appearanceSplit = DeucarianEditorWorkspaceControls.Split(appearanceForm, MotionPreviewRoot);
            appearanceSplit.AddToClassList("dw-motion-split");
            appearance.Add(appearanceSplit);
            Appearance = new DeucarianEditorWorkspaceForm(appearanceForm);
            Audio = new DeucarianEditorWorkspaceForm(AddPage());
            pages[1].AddToClassList("dw-settings-page");
            pages[2].AddToClassList("dw-settings-page");
            DeucarianEditorWorkspaceControls.Show(Workspace.Footer, false);
            SelectTab(0);
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= pages.Count) return;
            tabs.SetValueWithoutNotify(index);
            for (int i = 0; i < pages.Count; i++) DeucarianEditorWorkspaceControls.Show(pages[i], i == index);
            DeucarianEditorWorkspaceControls.Show(Workspace.Scope, index == 0);
            Workspace.Subtitle.text = index == 0 ? "Create a message. See how it feels." : index == 1
                ? "Set how messages appear." : "A small sound when attention is needed.";
            TabChanged?.Invoke(index);
        }

        public void SetTargets(IReadOnlyList<string> ids, IReadOnlyList<string> labels, string selected, string note)
        {
            if (ids.Count != labels.Count) throw new ArgumentException("Target IDs and labels must correspond.");
            syncing = true;
            try
            {
                bool changed = targetIds.Length != ids.Count;
                for (int i = 0; !changed && i < ids.Count; i++)
                    changed = targetIds[i] != ids[i] || targets.choices[i] != labels[i];
                if (changed)
                {
                    targetIds = new List<string>(ids).ToArray();
                    targets.choices = labels.Count == 0 ? new List<string> { "No active list" } : new List<string>(labels);
                }
                destinations.SetChoiceEnabled(1, ids.Count > 0, ids.Count == 0 ? "Start Play Mode and activate an application notification list." : "");
                destinations.SetValueWithoutNotify(selected == null ? 0 : 1);
                int index = Array.IndexOf(targetIds, selected);
                targets.SetValueWithoutNotify(targets.choices[Math.Max(0, index)]);
                targets.style.display = selected != null && ids.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
                destinationNote.text = note ?? string.Empty;
                destinations.tooltip = destinationNote.text;
                DeucarianEditorWorkspaceControls.Show(destinationNote, selected != null);
            }
            finally { syncing = false; }
        }

        public void SetMessages(IReadOnlyList<DeucarianEditorMessageData> visible, IReadOnlyList<DeucarianEditorMessageData> hidden, int pending)
        {
            var retained = new HashSet<string>();
            Synchronize(rows, visible, retained);
            Synchronize(overflow, hidden, retained);
            foreach (string id in new List<string>(entries.Keys))
                if (!retained.Contains(id)) { entries[id].Row.RemoveFromHierarchy(); entries.Remove(id); }
            count.text = visible.Count + " visible · " + hidden.Count + " queued" + (pending > 0 ? " · " + pending + " waiting" : "");
            overflow.text = "Overflow (" + hidden.Count + ") · still active";
            overflow.style.display = hidden.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            empty.text = pending > 0 ? "Waiting for the show delay…" : "All clear. Add a test message to begin.";
            empty.style.display = visible.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            clear.SetEnabled(visible.Count + hidden.Count + pending > 0);
        }

        public void RefreshForms() { Composer.Refresh(); Appearance.Refresh(); Audio.Refresh(); }
        public void Dispose() { TabChanged = null; Workspace.Dispose(); entries.Clear(); }

        private VisualElement AddPage(bool scrollable = true)
        {
            VisualElement page = scrollable
                ? DeucarianEditorWorkspaceControls.Scroll("lab-page-" + pages.Count)
                : DeucarianEditorWorkspaceControls.Region("lab-page-" + pages.Count, "dw-lab-test");
            page.AddToClassList("dw-lab-page");
            Workspace.Content.Add(page);
            pages.Add(page);
            return page;
        }

        private void Synchronize(VisualElement container, IReadOnlyList<DeucarianEditorMessageData> values, HashSet<string> retained)
        {
            container = container.contentContainer;
            for (int i = 0; i < values.Count; i++)
            {
                var data = values[i];
                if (!retained.Add(data.Id)) throw new ArgumentException("Displayed message IDs must be unique.");
                if (!entries.TryGetValue(data.Id, out var entry) || !entry.Matches(data))
                {
                    entry?.Row.RemoveFromHierarchy();
                    entry = new Entry(data);
                    entries[data.Id] = entry;
                }
                entry.Row.SetProgress(data.State, data.Remaining);
                entry.Update(data);
                entry.Row.SetActionEnabled(data.ActionEnabled);
                if (entry.Row.parent != container || container.IndexOf(entry.Row) != i)
                    container.Insert(i, entry.Row);
            }
        }

        private sealed class Entry
        {
            private DeucarianEditorMessageData data;
            public DeucarianEditorMessageRow Row { get; }
            public Entry(DeucarianEditorMessageData data)
            {
                this.data = data;
                Row = new DeucarianEditorMessageRow(data.Title, data.Body, data.Status, data.State, data.ActionLabel,
                    data.Action == null ? (Action)null : () => this.data.Action?.Invoke()) { name = data.Id };
            }
            public void Update(DeucarianEditorMessageData value) => data = value;
            public bool Matches(DeucarianEditorMessageData value) => data.Title == value.Title && data.Body == value.Body && data.Status == value.Status && data.ActionLabel == value.ActionLabel && (data.Action == null) == (value.Action == null);
        }
    }
}
