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
                Assert.That(view.Root.Q("control-center-card-deucarian.overview.build-packages").Query<Label>().ToList().Count, Is.EqualTo(2));
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
                Assert.That(area, Is.EqualTo(DeucarianControlCenterArea.Diagnostics));
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
    }
}
