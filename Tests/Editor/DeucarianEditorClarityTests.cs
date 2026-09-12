using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorClarityTests
    {
        [Test]
        public void AdvancedUsesActionableCheckNamesAndRespectsNavigationOwnership()
        {
            var issue = new DeucarianControlCenterCard("deucarian.readiness.issue.TEST-01",
                DeucarianControlCenterArea.Project, "TEST-01", "Assign the project camera.", "com.deucarian.editor");
            var diagnostic = new DeucarianToolDescriptor("test.diagnostics", "Diagnostic settings", "Settings",
                DeucarianControlCenterArea.Developer, () => { }, "com.deucarian.editor", navigationPath: "Diagnostics");
            var developer = new DeucarianToolDescriptor("test.developer", "Developer tool", "Tool",
                DeucarianControlCenterArea.Developer, () => { }, "com.deucarian.editor", navigationPath: "Developer/Testing");
            var snapshot = new DeucarianControlCenterSnapshot(System.DateTime.UtcNow, new[] { issue },
                System.Array.Empty<DeucarianControlCenterSection>(), new[] { diagnostic, developer });
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                view.Render(snapshot, DeucarianControlCenterArea.Project, null, "");
                var row = view.Root.Q<Button>("control-center-check-" + issue.Id);
                Assert.That(row.Q<Label>(className: "dw-navigation-title").text, Is.EqualTo(issue.Description));
                Assert.That(row.tooltip, Is.EqualTo(issue.Title));
                Assert.That(view.Root.Q("control-center-open-test.diagnostics"), Is.Null);
                Assert.That(view.Root.Q("control-center-open-test.developer"), Is.Null, "Project checks do not mix in developer tool destinations.");
                view.Render(snapshot, DeucarianControlCenterArea.Developer, null, "");
                Assert.That(view.Root.Q("control-center-open-test.developer"), Is.Not.Null);
                Assert.That(view.Root.Q("control-center-open-test.diagnostics"), Is.Null);
            }
        }

        [Test]
        public void SearchDoesNotOfferToOpenTheControlCenterFromInsideItself()
        {
            var snapshot = DeucarianControlCenterSnapshotBuilder.Capture();
            foreach (var result in DeucarianControlCenterSearch.Search(snapshot, "Control Center"))
                Assert.That(result.Kind != DeucarianControlCenterSearchResultKind.Tool ||
                    result.TargetId != DeucarianToolIds.ControlCenter, Is.True);
        }

        [UnityTest]
        public IEnumerator LongCheckExplanationsWrapWithoutOverlappingStatus()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Test"))
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                var issue = new DeucarianControlCenterCard("deucarian.readiness.issue.TEST-LONG",
                    DeucarianControlCenterArea.Project, "TEST-LONG",
                    "No project-default development context is assigned. Choose a context before starting your preview.",
                    "com.deucarian.editor", DeucarianControlCenterStatus.Error, "Error");
                var snapshot = new DeucarianControlCenterSnapshot(System.DateTime.UtcNow, new[] { issue },
                    System.Array.Empty<DeucarianControlCenterSection>(), System.Array.Empty<DeucarianToolDescriptor>());
                workspace.Content.Add(view.Root);
                view.Render(snapshot, DeucarianControlCenterArea.Project, null, "");
                try
                {
                    window.Show();
                    foreach (float width in new[] { 620f, 1586f })
                    {
                        window.position = new Rect(40, 40, width, 940);
                        for (int frame = 0; frame < 12; frame++) yield return null;
                        var row = view.Root.Q<Button>("control-center-check-" + issue.Id);
                        var title = row.Q<Label>(className: "dw-navigation-title");
                        var status = row.Q<Label>(className: "dw-navigation-status");
                        Assert.That(title.worldBound.xMax, Is.LessThanOrEqualTo(status.worldBound.xMin));
                        Assert.That(title.worldBound.yMax, Is.LessThanOrEqualTo(row.worldBound.yMax));
                        Assert.That(title.resolvedStyle.whiteSpace, Is.EqualTo(WhiteSpace.Normal));
                    }
                }
                finally { window.Close(); }
            }
        }

        [Test]
        public void RefreshPreservesTheScrollableSurface()
        {
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                var snapshot = DeucarianControlCenterSnapshotBuilder.Capture();
                view.Render(snapshot, DeucarianControlCenterArea.Overview, null, "");
                var original = view.Root.Q<ScrollView>("control-center-content");
                view.Render(snapshot, DeucarianControlCenterArea.Overview, null, "");
                Assert.That(view.Root.Q<ScrollView>("control-center-content"), Is.SameAs(original));
            }
        }

        [Test]
        public void TaskLayoutPinsContextAndActionsOutsideTheScrollingBody()
        {
            using (var layout = new DeucarianEditorTaskLayout(new VisualElement(), "editor", "Example", "Do one thing"))
            {
                Assert.That(layout.Body.Contains(layout.Context), Is.False);
                Assert.That(layout.Body.Contains(layout.Actions), Is.False);
                Assert.That(layout.Body.Contains(layout.Content), Is.True);
                Assert.That(layout.Advanced.value, Is.False);
                Assert.That(layout.Preview.value, Is.True);
            }
        }

        [Test]
        public void PreferenceKeysCannotCollideAcrossProjects()
        {
            string first = DeucarianEditorProjectPreferences.PrefixFor(Application.dataPath);
            string other = DeucarianEditorProjectPreferences.PrefixFor(Application.dataPath + "/OtherProject");
            Assert.That(first, Is.Not.EqualTo(other));
            Assert.That(first, Does.Not.Contain(Application.dataPath));
        }

        [Test]
        public void NarrowCardsUseContentHeightInsteadOfTheirWideColumnWidth()
        {
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                var card = new DeucarianControlCenterCard("deucarian.project.example",
                    DeucarianControlCenterArea.Project, "Project check", "Review this project check.",
                    "com.deucarian.editor", DeucarianControlCenterStatus.Success, "Ready");
                var snapshot = new DeucarianControlCenterSnapshot(System.DateTime.UtcNow, new[] { card },
                    System.Array.Empty<DeucarianControlCenterSection>(), System.Array.Empty<DeucarianToolDescriptor>());
                view.Render(snapshot, DeucarianControlCenterArea.Project, null, "");
                view.SetLayoutMode(DeucarianEditorLayoutMode.Narrow);
                int checkedCards = 0;
                view.Root.Query<VisualElement>().ForEach(element =>
                {
                    if (element.name == null || !element.name.StartsWith("control-center-card-")) return;
                    Assert.That(element.style.flexBasis.keyword, Is.EqualTo(StyleKeyword.Auto));
                    Assert.That(element.style.flexGrow.value, Is.Zero);
                    checkedCards++;
                });
                Assert.That(checkedCards, Is.GreaterThan(0));
            }
        }

        [Test]
        public void SearchHintTracksProgrammaticAndTypedValues()
        {
            var field = DeucarianEditorSearchField.Create("Find a tool", _ => { });
            var label = field.Q<Label>("deucarian-search-placeholder");
            var hint = label.parent;
            Assert.That(label.text, Is.EqualTo("Find a tool"));
            field.SetValueWithoutNotify("notifications");
            Assert.That(hint.style.display.value, Is.EqualTo(DisplayStyle.None));
            field.SetValueWithoutNotify("");
            Assert.That(hint.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        }

        [UnityTest]
        public IEnumerator PrimaryAndSecondaryButtonsHaveVisibleKeyboardFocus()
        {
            var window = EditorWindow.CreateWindow<FocusTestWindow>();
            try
            {
                window.Show();
                window.Repaint();
                for (int i = 0; i < 30 && !window.Rendered; i++) yield return null;
                Assert.That(window.Rendered, Is.True);
                Assert.That(window.DistinctPrimary, Is.True);
                Assert.That(window.DistinctSecondary, Is.True);
            }
            finally { window.Close(); }
        }

        private sealed class FocusTestWindow : EditorWindow
        {
            public bool Rendered;
            public bool DistinctPrimary;
            public bool DistinctSecondary;
            private void OnGUI()
            {
                DistinctPrimary = DeucarianEditorButtons.PrimaryStyle.focused.background != DeucarianEditorButtons.PrimaryStyle.normal.background;
                DistinctSecondary = DeucarianEditorButtons.SecondaryStyle.focused.background != DeucarianEditorButtons.SecondaryStyle.normal.background;
                Rendered = true;
            }
        }

        [UnityTest]
        public IEnumerator QuietAppearanceUpdatesAnAlreadyAttachedWindowAndSurvivesRebuild()
        {
            bool previous = DeucarianEditorAppearance.DecorativeBackgrounds;
            var window = EditorWindow.CreateWindow<AppearanceTestWindow>();
            try
            {
                window.Show();
                DeucarianEditorAppearance.DecorativeBackgrounds = false;
                window.Rebuild();
                yield return null;
                var root = window.rootVisualElement;
                Assert.That(root.ClassListContains("deucarian-quiet"), Is.True);
                Assert.That(root.Q("deucarian-window-background").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
                DeucarianEditorAppearance.DecorativeBackgrounds = true;
                yield return null;
                Assert.That(root.ClassListContains("deucarian-quiet"), Is.False);
                Assert.That(root.Q("deucarian-window-background").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
                window.Rebuild();
                DeucarianEditorAppearance.DecorativeBackgrounds = false;
                yield return null;
                Assert.That(root.ClassListContains("deucarian-quiet"), Is.True);
            }
            finally { window.Close(); DeucarianEditorAppearance.DecorativeBackgrounds = previous; }
        }

        private sealed class AppearanceTestWindow : EditorWindow
        {
            public void CreateGUI() => Rebuild();
            public void Rebuild()
            {
                DeucarianEditorVisualShell.CreateWindowShell(rootVisualElement);
                DeucarianEditorWindowChrome.ConfigureFixedWallpaper(rootVisualElement);
            }
        }
    }
}
