using System;
using UnityEngine.UIElements;
using Controls = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorWorkspaceSpecimen : IDisposable
    {
        private readonly DeucarianEditorWorkspace workspace;
        internal DeucarianEditorWorkspaceSpecimen(DeucarianEditorWorkspace workspace) =>
            this.workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

        internal void Build()
        {
            workspace.Title.text = "Editor Component Gallery";
            workspace.Subtitle.text = "Preview the shared controls used to build Deucarian editor UIs.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, "deucarian.editor.workspace-preview");
            var page = Controls.Scroll("workspace-specimen-scroll"); workspace.Content.Add(page);
            page.Add(Controls.Divider());
            page.Add(Controls.Label("Buttons", "dw-section-title"));
            int clicks = 0;
            var feedback = Controls.Label(string.Empty, "dw-note"); Controls.Show(feedback, false);
            Action clicked = () => { feedback.text = "Example action · " + ++clicks; Controls.Show(feedback, true); };
            var primary = Controls.Button("Primary", clicked, true); primary.name = "specimen-add";
            var disabled = Controls.Button("Disabled", () => { }); disabled.SetEnabled(false);
            var buttons = Controls.Actions(primary, Controls.Button("Secondary", clicked),
                Controls.Button("Quiet text", clicked, DeucarianEditorButtonRole.Quiet), disabled,
                Controls.IconButton(string.Empty, "cog", clicked));
            buttons[4].tooltip = "Example icon action";
            buttons.AddToClassList("dw-gallery-buttons"); page.Add(buttons); page.Add(feedback);
            page.Add(Controls.Divider()); page.Add(Controls.Label("Fields", "dw-section-title"));
            var left = new VisualElement(); var right = new VisualElement();
            var fields = Controls.Split(left, right); fields.AddToClassList("dw-gallery-fields"); page.Add(fields);
            int choice = 0; string input = "Example"; bool on = true, off = false;
            var form = new DeucarianEditorWorkspaceForm(left);
            form.Choice("specimen-choice", "Dropdown", new[] { "Default", "Alternate" }, () => choice, value => choice = value);
            form.Text("specimen-title", "Input", () => input, value => input = value);
            var switches = new DeucarianEditorWorkspaceForm(right);
            switches.Toggle("specimen-switch-on", "Switch (On)", () => on, value => on = value);
            switches.Toggle("specimen-switch-off", "Switch (Off)", () => off, value => off = value);
            page.Add(Controls.Divider()); page.Add(Controls.Label("Sliders", "dw-section-title"));
            var sliderRoot = Controls.Region(null, "dw-gallery-sliders"); page.Add(sliderRoot);
            var sliders = new DeucarianEditorWorkspaceForm(sliderRoot);
            int volume = 50; float duration = .25f;
            sliders.IntegerSlider("specimen-number", "Volume (%)", 0, 100, () => volume, value => volume = value);
            sliders.Slider("specimen-duration", "Duration (s)", 0, 1, () => duration, value => duration = value);
            page.Add(Controls.Divider()); page.Add(Controls.Label("Statuses", "dw-section-title"));
            var statuses = Controls.Region("specimen-message-rows", "dw-gallery-statuses"); page.Add(statuses);
            AddStatus(statuses, "Ready", DeucarianEditorStatus.Success, DeucarianEditorIconIds.Success);
            AddStatus(statuses, "Attention", DeucarianEditorStatus.Warning, DeucarianEditorIconIds.Warning);
            AddStatus(statuses, "Error", DeucarianEditorStatus.Error, DeucarianEditorIconIds.Error);
            AddStatus(statuses, "Inactive", DeucarianEditorStatus.Disabled, "circle");
            var more = new DeucarianEditorWorkspaceForm(page).Section("More examples", true);
            more.Root.Add(new DeucarianEditorSteps("Choose", "Configure", "Review").Root);
            more.Root.Add(new DeucarianEditorControlSpecimen());
            workspace.FooterLeading.text = string.Empty;
            workspace.FooterTrailing.text = string.Empty;
        }
        private static void AddStatus(VisualElement root, string label, DeucarianEditorStatus status, string icon)
        {
            var row = Controls.Region(null, "dw-gallery-status");
            var symbol = Controls.Icon(icon); symbol.AddToClassList("dw-status-" + status.ToString().ToLowerInvariant());
            row.Add(symbol); row.Add(Controls.Label(label)); root.Add(row);
        }
        public void Dispose() { }
    }
}
