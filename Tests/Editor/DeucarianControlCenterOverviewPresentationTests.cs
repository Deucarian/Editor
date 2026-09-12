using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianControlCenterOverviewPresentationTests
    {
        [Test]
        public void OverviewHasOneFocusWithoutRepeatedCountsOrToolDescriptions()
        {
            var ready = Card("deucarian.readiness.overview", DeucarianControlCenterArea.Overview, DeucarianControlCenterStatus.Success);
            var summary = Card("deucarian.overview.build-packages", DeucarianControlCenterArea.Overview, DeucarianControlCenterStatus.Success);
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                view.Render(Snapshot(ready, summary), DeucarianControlCenterArea.Overview, null, "");
                Assert.That(view.Root.Query(className: "dw-focus-title").ToList().Count, Is.EqualTo(1));
                Assert.That(view.Root.Q<Label>(className: "dw-focus-title").text, Is.EqualTo("No issues reported"));
                Assert.That(view.Root.Q("control-center-card-deucarian.readiness.overview"), Is.Null);
                Assert.That(view.Root.Q("control-center-card-deucarian.overview.build-packages"), Is.Null,
                    "The overview shows focused tool destinations, not a second row of package summaries.");
                Assert.That(view.Root.Query<Label>().ToList().Any(label => label.text.Contains("Verbose source")), Is.False);
                view.Render(Snapshot(ready, summary), DeucarianControlCenterArea.Overview, null, "Verbose source");
                Assert.That(view.Root.Q("control-center-card-deucarian.overview.build-packages"), Is.Null);
                Assert.That(view.Root.Q("control-center-search-result-deucarian.overview.build-packages"), Is.Not.Null, "Detail remains searchable.");
            }
        }

        [UnityTest]
        public IEnumerator AttentionPrioritizesErrorsIncludingSectionsAndNavigatesWithoutExecutingAFix()
        {
            var warning = Card("warning", DeucarianControlCenterArea.Project, DeucarianControlCenterStatus.Warning);
            var error = Card("error", DeucarianControlCenterArea.Diagnostics, DeucarianControlCenterStatus.Error);
            var section = new DeucarianControlCenterSection("health", DeucarianControlCenterArea.Diagnostics, "Health", new[] { error });
            var snapshot = new DeucarianControlCenterSnapshot(DateTime.UtcNow, new[] { warning }, new[] { section }, Array.Empty<DeucarianToolDescriptor>());
            Assert.That(DeucarianControlCenterOverviewPresentation.FindAttention(snapshot), Is.SameAs(error));
            DeucarianControlCenterArea? area = null;
            string target = null;
            var focus = DeucarianControlCenterOverviewPresentation.CreateFocus(snapshot, (value, id) => { area = value; target = id; });
            Assert.That(focus.Q<Label>(className: "dw-focus-title").text, Is.EqualTo("Needs your attention"));
            Assert.That(focus.Q<Label>(className: "dw-focus-description").text, Is.EqualTo(error.Description), "Explain the issue rather than repeating an internal code or severity.");
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            try
            {
                window.Show();
                window.rootVisualElement.Add(focus);
                focus.Q<Button>().Focus();
                yield return null;
                using (var evt = NavigationSubmitEvent.GetPooled()) { evt.target = focus.Q<Button>(); focus.Q<Button>().SendEvent(evt); }
                Assert.That(area, Is.EqualTo(DeucarianControlCenterArea.Project), "Review always opens the unified check list, never an arbitrary domain page.");
                Assert.That(target, Is.EqualTo("error"));
            }
            finally { window.Close(); }
        }

        private static DeucarianControlCenterCard Card(string id, DeucarianControlCenterArea area, DeucarianControlCenterStatus status) =>
            new DeucarianControlCenterCard(id, area, "Build & packages", "Verbose source description for search.", "com.deucarian.editor", status, status == DeucarianControlCenterStatus.Success ? "Ready" : status.ToString(), details: new[] { "Errors: 0", "Warnings: 0" });

        [Test]
        public void AConcreteStatusTakesPrecedenceOverAGenericToolDescription()
        {
            var card = new DeucarianControlCenterCard("theming", DeucarianControlCenterArea.Experience, "Theming",
                "Project-local active theme selection and authoring workflow.", "com.deucarian.theming",
                DeucarianControlCenterStatus.Warning, "No active theme");
            Assert.That(DeucarianControlCenterOverviewPresentation.DescribeAttention(card), Is.EqualTo("Theming · No active theme"));
        }

        private static DeucarianControlCenterSnapshot Snapshot(params DeucarianControlCenterCard[] cards) =>
            new DeucarianControlCenterSnapshot(DateTime.UtcNow, cards, Array.Empty<DeucarianControlCenterSection>(), Array.Empty<DeucarianToolDescriptor>());

        [UnityTest]
        public IEnumerator SearchResultsUseWorkspaceTypographyAtEverySupportedScale()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>(); window.Show();
            int previous = DeucarianEditorAppearance.WorkspaceScalePercent;
            using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Search test"))
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                try
                {
                    workspace.Content.Add(view.Root);
                    view.Render(Snapshot(Card("search-sizing", DeucarianControlCenterArea.Overview, DeucarianControlCenterStatus.Success)),
                        DeucarianControlCenterArea.Overview, null, "Build");
                    var reference = DeucarianEditorWorkspaceControls.Label("Reference row", "dw-message-title"); workspace.Content.Add(reference);
                    foreach (int width in new[] { 1586, 1000, 820 })
                    foreach (int scale in new[] { 75, 100, 150 })
                    {
                        window.position = new Rect(30, 30, width, 800);
                        DeucarianEditorAppearance.WorkspaceScalePercent = scale;
                        for (int frame = 0; frame < 10; frame++) yield return null;
                        var row = view.Root.Q<Button>("control-center-search-result-search-sizing");
                        var title = row.Q<Label>(className: "dw-message-title");
                        Assert.That(title.resolvedStyle.fontSize, Is.GreaterThanOrEqualTo(reference.resolvedStyle.fontSize));
                        Assert.That(row.layout.height, Is.GreaterThanOrEqualTo(88));
                        Assert.That(title.worldBound.xMax, Is.LessThanOrEqualTo(row.worldBound.xMax + 1));
                    }
                }
                finally { DeucarianEditorAppearance.WorkspaceScalePercent = previous; window.Close(); }
            }
        }
    }
}
