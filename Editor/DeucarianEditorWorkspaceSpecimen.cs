using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Interactive examples of shared editor components, with no domain commands or settings.</summary>
    internal sealed class DeucarianEditorWorkspaceSpecimen
    {
        private readonly DeucarianEditorWorkspace workspace;

        internal DeucarianEditorWorkspaceSpecimen(DeucarianEditorWorkspace workspace)
        {
            this.workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        }

        internal void Build()
        {
            workspace.Title.text = "Editor Component Gallery";
            workspace.Subtitle.text = "Explore the shared controls used by every package editor.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, "deucarian.editor.workspace-preview");
            DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
            var tabs = new DeucarianEditorChoiceBar(new[] { "Controls", "Status & empty states" }, tabs: true);
            workspace.Tabs.Add(tabs);
            var page = DeucarianEditorWorkspaceControls.Scroll("workspace-specimen-scroll");
            var formRoot = DeucarianEditorWorkspaceControls.Region(null, "dw-specimen-form");
            var preview = DeucarianEditorWorkspaceControls.Region(null, "dw-specimen-preview");
            BuildControls(formRoot, preview);
            page.Add(DeucarianEditorWorkspaceControls.Split(formRoot, preview));
            workspace.Content.Add(page);
            var statuses = DeucarianEditorWorkspaceControls.Scroll("specimen-statuses");
            statuses.Add(DeucarianEditorWorkspaceControls.Label("Status styles", "dw-section-title"));
            var examples = DeucarianEditorWorkspaceControls.Region("specimen-message-rows", "dw-message-rows");
            examples.Add(new DeucarianEditorMessageRow("Information", "Supporting context for a task.", DeucarianEditorStatus.Info, "Sample"));
            examples.Add(new DeucarianEditorMessageRow("Ready", "The action completed successfully.", DeucarianEditorStatus.Success, "Sample"));
            examples.Add(new DeucarianEditorMessageRow("Needs attention", "Explain what needs checking and why.", DeucarianEditorStatus.Warning, "Sample"));
            examples.Add(new DeucarianEditorMessageRow("Action failed", "Explain what happened and the next useful step.", DeucarianEditorStatus.Error, "Sample"));
            statuses.Add(examples);
            statuses.Add(DeucarianEditorWorkspaceControls.Label("Empty state", "dw-section-title"));
            statuses.Add(DeucarianEditorWorkspaceControls.Label("Nothing selected. Choose an item to see its details.", "dw-empty"));
            DeucarianEditorWorkspaceControls.Show(statuses, false);
            workspace.Content.Add(statuses);
            tabs.Changed += index =>
            {
                DeucarianEditorWorkspaceControls.Show(page, index == 0);
                DeucarianEditorWorkspaceControls.Show(statuses, index == 1);
            };
            workspace.FooterLeading.text = "Component examples only · Nothing is saved";
            workspace.FooterTrailing.text = "Editor-owned design system";
        }

        private static void BuildControls(VisualElement root, VisualElement preview)
        {
            var form = new DeucarianEditorWorkspaceForm(root);
            string title = "Example title", description = "Supporting text that explains the next step.";
            bool enabled = true;
            float amount = 0.5f;
            int choice = 0, clicks = 0;
            var heading = DeucarianEditorWorkspaceControls.Label(title, "dw-section-title");
            var copy = DeucarianEditorWorkspaceControls.Label(description, "dw-muted");
            var feedback = DeucarianEditorWorkspaceControls.Label("Try a control on the left.", "dw-note");
            preview.Add(DeucarianEditorWorkspaceControls.Label("Live component preview", "dw-muted"));
            preview.Add(heading);
            preview.Add(copy);
            preview.Add(feedback);
            form.Text("specimen-title", "Heading", () => title, value => { title = value; heading.text = value; });
            form.Text("specimen-body", "Supporting text", () => description, value => { description = value; copy.text = value; }, true);
            form.Segments("specimen-choice", "Choice", new[] { "First", "Second" }, () => choice, value => choice = value);
            form.Slider("specimen-number", "Amount", 0, 1, () => amount, value => amount = value);
            form.Toggle("specimen-enabled", "Enable action", () => enabled, value => { enabled = value; form.Refresh(); });
            form.Action("specimen-add", "Try primary action", () => feedback.text = "Action clicked " + ++clicks + " time(s).", () => enabled, true);
            form.Action("specimen-disabled", "Unavailable action", () => { }, () => false);
            var section = form.Section("Optional details", true);
            section.Note(() => "Use a disclosure for less frequently needed settings.");
            form.Refresh();
        }
    }
}
