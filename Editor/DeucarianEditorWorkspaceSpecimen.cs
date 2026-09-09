using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Fixed presentation examples and local widget events. No application messages are created.</summary>
    internal sealed class DeucarianEditorWorkspaceSpecimen
    {
        private readonly DeucarianEditorWorkspace workspace;
        private VisualElement examples;
        private Label count;
        private TextField title;
        private TextField body;
        private PopupField<string> severity;
        private DeucarianEditorChoiceBar dismissal;
        private FloatField duration;
        private Label feedback;
        private VisualElement overflowExample;
        private int added;

        internal DeucarianEditorWorkspaceSpecimen(DeucarianEditorWorkspace workspace)
        {
            this.workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        }

        internal void Build()
        {
            workspace.Title.text = "Notifications";
            workspace.Subtitle.text = "Create a test message and see what happens.";
            workspace.Root.Insert(0, DeucarianEditorWorkspaceControls.Label(
                "EDITOR COMPONENT PREVIEW · SAMPLE DATA · NOT CONNECTED TO YOUR APP", "dw-preview-label"));
            AddNavigation("overview", "Overview", DeucarianEditorIconIds.Dashboard, DeucarianToolIds.ControlCenter);
            AddNavigation("packages", "Packages", DeucarianEditorIconIds.Package, DeucarianToolIds.PackageInstaller);
            AddNavigation("appearance", "Appearance", DeucarianEditorIconIds.Palette, DeucarianToolIds.ThemeManager);
            AddNavigation("audio", "Audio", "headset", DeucarianToolIds.ThemeManager);
            workspace.AddNavigation("notifications", "Notifications", DeucarianEditorIconIds.Sample, () => workspace.SelectNavigation("notifications"));
            AddNavigation("diagnostics", "Diagnostics", DeucarianEditorIconIds.Activity, DeucarianToolIds.Diagnostics);
            workspace.AddNavigation("advanced", "Advanced", DeucarianEditorIconIds.Settings,
                () => DeucarianControlCenterWindow.Open(DeucarianControlCenterArea.Developer), true);
            workspace.SelectNavigation("notifications");
            workspace.ContextButton.SetEnabled(false);
            workspace.ContextButton.tooltip = "Current project. Project switching is not part of this visual preview.";
            var standalone = DeucarianEditorWorkspaceControls.Button("Open standalone", () => DeucarianToolRegistry.TryOpen("deucarian.notifications.lab"));
            standalone.SetEnabled(DeucarianToolRegistry.TryGet("deucarian.notifications.lab", out _));
            standalone.tooltip = "Open the existing package-owned Notification Lab. This preview does not replace it.";
            workspace.PageActions.Add(standalone);
            var tabs = new DeucarianEditorChoiceBar(new[] { "Test", "Appearance", "Audio" }, tabs: true);
            workspace.Tabs.Add(tabs);
            workspace.Scope.Add(DeucarianEditorWorkspaceControls.Label("Destination"));
            var destination = new DeucarianEditorChoiceBar(new[] { "Editor preview", "Running app" });
            destination.SetChoiceEnabled(1, false, "A domain-package adapter is required to connect an application. This is an Editor-only component preview.");
            workspace.Scope.Add(destination);
            workspace.Scope.Add(DeucarianEditorWorkspaceControls.Label("Sandbox · Does not affect your app", "dw-muted"));
            var page = DeucarianEditorWorkspaceControls.Scroll("workspace-specimen-scroll");
            var form = DeucarianEditorWorkspaceControls.Region(null, "dw-specimen-form");
            var preview = DeucarianEditorWorkspaceControls.Region(null, "dw-specimen-preview");
            BuildForm(form);
            BuildExamples(preview);
            page.Add(DeucarianEditorWorkspaceControls.Split(form, preview));
            workspace.Content.Add(page);
            var alternate = DeucarianEditorWorkspaceControls.Region("workspace-specimen-alternate", "dw-alternate");
            alternate.style.display = DisplayStyle.None;
            workspace.Content.Add(alternate);
            tabs.Changed += index =>
            {
                page.style.display = index == 0 ? DisplayStyle.Flex : DisplayStyle.None;
                alternate.style.display = index == 0 ? DisplayStyle.None : DisplayStyle.Flex;
                alternate.Clear();
                if (index != 0)
                {
                    alternate.Add(DeucarianEditorWorkspaceControls.Label(index == 1 ? "Appearance" : "Audio", "dw-section-title"));
                    alternate.Add(DeucarianEditorWorkspaceControls.Label(
                        "This is a visual component preview. Application settings remain in their owning package.", "dw-muted"));
                }
            };
            workspace.SearchField.RegisterValueChangedCallback(evt =>
            {
                string query = evt.newValue ?? string.Empty;
                foreach (VisualElement item in workspace.Navigation.Children())
                    item.style.display = (item.Q<Label>()?.text ?? string.Empty).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            });
            workspace.FooterLeading.text = "Preview only · sample timers are frozen";
            workspace.FooterTrailing.text = "Sample layout · Maximum 5 · Fade · Lazy follow off";
        }

        private void AddNavigation(string id, string label, string icon, string toolId)
        {
            var button = workspace.AddNavigation(id, label, icon, () => DeucarianToolRegistry.TryOpen(toolId));
            button.tooltip = DeucarianToolRegistry.TryGet(toolId, out _) ? "Open " + label : "This tool is not installed in the current project.";
            button.SetEnabled(DeucarianToolRegistry.TryGet(toolId, out _));
        }

        private void BuildForm(VisualElement form)
        {
            form.Add(DeucarianEditorWorkspaceControls.Label("New test message", "dw-section-title"));
            severity = new PopupField<string>(new List<string> { "Warning", "Error", "Info", "Success" }, 0) { name = "specimen-type" };
            form.Add(DeucarianEditorWorkspaceControls.Field("Type", severity));
            title = new TextField { value = "Example warning", name = "specimen-title" };
            form.Add(DeucarianEditorWorkspaceControls.Field("Title", title));
            body = new TextField { value = "This is a test message.", multiline = true, name = "specimen-body" };
            body.AddToClassList("dw-multiline");
            form.Add(DeucarianEditorWorkspaceControls.Field("Message", body));
            dismissal = new DeucarianEditorChoiceBar(new[] { "Resolve manually", "After a delay" });
            form.Add(DeucarianEditorWorkspaceControls.Field("Dismissal", dismissal));
            duration = new FloatField { value = 5, name = "specimen-duration" };
            var durationRow = DeucarianEditorWorkspaceControls.Field("Seconds", duration);
            durationRow.style.display = DisplayStyle.None;
            form.Add(durationRow);
            dismissal.Changed += index => durationRow.style.display = index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            var add = DeucarianEditorWorkspaceControls.Button("Add test message", AddExample, true);
            add.name = "specimen-add";
            add.tooltip = "Add a local presentation row. No notification store, audio or runtime is involved.";
            title.RegisterValueChangedCallback(evt => add.SetEnabled(!string.IsNullOrWhiteSpace(evt.newValue)));
            form.Add(add);
            var scenarios = new Foldout { text = "Test scenarios", value = false };
            scenarios.AddToClassList("dw-foldout");
            scenarios.Add(DeucarianEditorWorkspaceControls.Button("Restore sample rows", ResetExamples));
            form.Add(scenarios);
            feedback = DeucarianEditorWorkspaceControls.Label("Test messages are temporary.", "dw-muted");
            feedback.AddToClassList("dw-note");
            form.Add(feedback);
        }

        private void BuildExamples(VisualElement preview)
        {
            var toolbar = DeucarianEditorWorkspaceControls.Region(null, "dw-preview-toolbar");
            toolbar.Add(DeucarianEditorWorkspaceControls.Label("Message preview", "dw-section-title"));
            count = DeucarianEditorWorkspaceControls.Label("5 shown · 2 in overflow", "dw-muted");
            toolbar.Add(count);
            var clear = DeucarianEditorWorkspaceControls.Button("Clear test messages", () => { examples.Clear(); UpdateCount(); });
            clear.AddToClassList("dw-link");
            toolbar.Add(clear);
            preview.Add(toolbar);
            examples = DeucarianEditorWorkspaceControls.Region("specimen-message-rows", "dw-message-rows");
            preview.Add(examples);
            ResetExamples();
            overflowExample = DeucarianEditorWorkspaceControls.Region("specimen-overflow", "dw-overflow-example");
            var overflow = new Foldout { text = "Overflow (2)", value = false };
            overflow.AddToClassList("dw-foldout");
            overflow.Add(DeucarianEditorWorkspaceControls.Label("Two hidden rows in this fixed design specimen. No live queue is connected.", "dw-muted"));
            overflowExample.Add(overflow);
            overflowExample.Add(DeucarianEditorWorkspaceControls.Label("Overflow messages remain active.", "dw-muted"));
            preview.Add(overflowExample);
        }

        private void ResetExamples()
        {
            examples.Clear();
            AddRow("Example warning", "This is a test message.", DeucarianEditorStatus.Warning, "Until resolved");
            AddRow("Example error", "This is a test message.", DeucarianEditorStatus.Error, "Until resolved");
            AddRow("Timed information", "This is a test message.", DeucarianEditorStatus.Info, "Expires in 3.2 s", 0.53f);
            AddRow("Connection warning", "This is a test message.", DeucarianEditorStatus.Warning, "Until resolved");
            AddRow("Saved successfully", "This is a test message.", DeucarianEditorStatus.Disabled, "Expires in 1.8 s", 0.3f);
            added = 0;
            count.text = "5 shown · 2 in overflow";
            if (overflowExample != null) overflowExample.style.display = DisplayStyle.Flex;
            workspace.FooterTrailing.text = "Sample layout · Maximum 5 · Fade · Lazy follow off";
        }

        private void AddExample()
        {
            if (string.IsNullOrWhiteSpace(title.value)) return;
            Enum.TryParse(severity.value, out DeucarianEditorStatus status);
            float seconds = float.IsNaN(duration.value) || float.IsInfinity(duration.value) ? 5 : Math.Max(0.1f, duration.value);
            AddRow(title.value.Trim(), body.value, status, dismissal.Value == 0 ? "Until resolved" : $"Sample: {seconds:0.#} s (frozen)", dismissal.Value == 0 ? (float?)null : 1f);
            added++;
            feedback.text = "Added " + added + " local sample(s). The running app is unchanged.";
            UpdateCount();
        }

        private void AddRow(string heading, string text, DeucarianEditorStatus status, string lifetime, float? progress = null)
        {
            DeucarianEditorMessageRow row = null;
            row = new DeucarianEditorMessageRow(heading, text, status, lifetime,
                progress.HasValue ? null : "Resolve", () => { row.RemoveFromHierarchy(); UpdateCount(); });
            row.SetProgress(lifetime, progress);
            examples.Add(row);
        }

        private void UpdateCount()
        {
            count.text = examples.childCount + " sample rows · not a live list";
            if (overflowExample != null) overflowExample.style.display = DisplayStyle.None;
            workspace.FooterTrailing.text = "Presentation rows only · No queue, animation or audio";
        }
    }
}
