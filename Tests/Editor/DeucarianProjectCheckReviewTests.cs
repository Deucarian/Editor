using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianProjectCheckReviewTests
    {
        [Test]
        public void ReviewIncludesDomainAndSectionIssuesWithoutDuplicateSummaries()
        {
            var error = Card("domain-error", DeucarianControlCenterArea.Diagnostics, DeucarianControlCenterStatus.Error);
            var warning = Card("domain-warning", DeucarianControlCenterArea.Experience, DeucarianControlCenterStatus.Warning);
            var ready = Card("deucarian.readiness.project", DeucarianControlCenterArea.Project, DeucarianControlCenterStatus.Success);
            var summary = Card("deucarian.overview.diagnostics", DeucarianControlCenterArea.Overview, DeucarianControlCenterStatus.Error);
            var section = new DeucarianControlCenterSection("health", DeucarianControlCenterArea.Diagnostics, "Health", new[] { error });
            var snapshot = new DeucarianControlCenterSnapshot(DateTime.UtcNow, new[] { ready, summary, warning, error },
                new[] { section }, Array.Empty<DeucarianToolDescriptor>());
            Assert.That(DeucarianProjectCheckReview.Collect(snapshot).Select(card => card.Id),
                Is.EqualTo(new[] { error.Id, warning.Id }));
        }

        [UnityTest]
        public IEnumerator SeverityTabsFilterInPlaceAndDoNotInvokeNavigation()
        {
            var error = Card("error", DeucarianControlCenterArea.Diagnostics, DeucarianControlCenterStatus.Error);
            var warning = Card("warning", DeucarianControlCenterArea.Experience, DeucarianControlCenterStatus.Warning);
            var snapshot = Snapshot(error, warning);
            int navigations = 0;
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            using (var view = new DeucarianControlCenterView((_, __) => navigations++, () => { }))
            {
                try
                {
                    window.Show();
                    window.rootVisualElement.Add(view.Root);
                    view.Render(snapshot, DeucarianControlCenterArea.Project, error.Id, "");
                    var scroll = view.Root.Q<ScrollView>("control-center-content");
                    Assert.That(view.Root.Q("control-center-card-error").ClassListContains("dw-check-expanded"), Is.True);
                    var filters = view.Root.Q<DeucarianEditorChoiceBar>("control-center-check-filters");
                    Assert.That(filters.Q<Button>("choice-1").text, Is.EqualTo("Errors (1)"));
                    yield return Submit(filters.Q<Button>("choice-2"));
                    Assert.That(view.Root.Q("control-center-card-error"), Is.Null);
                    Assert.That(view.Root.Q("control-center-card-warning"), Is.Not.Null);
                    Assert.That(view.Root.Q<ScrollView>("control-center-content"), Is.SameAs(scroll));
                    view.Render(snapshot, DeucarianControlCenterArea.Project, null, "");
                    Assert.That(view.Root.Q("control-center-card-error"), Is.Null, "Refresh keeps the filter.");
                    filters = view.Root.Q<DeucarianEditorChoiceBar>("control-center-check-filters");
                    var warningTab = filters.Q<Button>("choice-2");
                    warningTab.Focus();
                    using (var evt = KeyDownEvent.GetPooled(new Event { type = EventType.KeyDown, keyCode = KeyCode.RightArrow }))
                    { evt.target = warningTab; warningTab.SendEvent(evt); }
                    for (int frame = 0; frame < 3; frame++) yield return null;
                    var informationTab = view.Root.Q<DeucarianEditorChoiceBar>("control-center-check-filters").Q<Button>("choice-3");
                    Assert.That(window.rootVisualElement.focusController.focusedElement, Is.SameAs(informationTab));
                    Assert.That(view.Root.Query<Label>().ToList().Any(label => label.text == "No checks match this filter."), Is.True);
                    Assert.That(navigations, Is.Zero);
                    view.Render(snapshot, DeucarianControlCenterArea.Project, error.Id, "");
                    Assert.That(view.Root.Q("control-center-card-error"), Is.Not.Null, "Explicit issue navigation reveals a filtered-out issue.");
                    Assert.That(view.Root.Q<DeucarianEditorChoiceBar>("control-center-check-filters").Value, Is.Zero);
                }
                finally { window.Close(); }
            }
        }

        private static IEnumerator Submit(Button button)
        {
            button.Focus();
            yield return null;
            using (var evt = NavigationSubmitEvent.GetPooled()) { evt.target = button; button.SendEvent(evt); }
            yield return null;
        }

        private static DeucarianControlCenterCard Card(string id, DeucarianControlCenterArea area, DeucarianControlCenterStatus status) =>
            new DeucarianControlCenterCard(id, area, id, "An actionable explanation.", "com.deucarian.editor", status, status.ToString());

        private static DeucarianControlCenterSnapshot Snapshot(params DeucarianControlCenterCard[] cards) =>
            new DeucarianControlCenterSnapshot(DateTime.UtcNow, cards, Array.Empty<DeucarianControlCenterSection>(), Array.Empty<DeucarianToolDescriptor>());
    }
}
