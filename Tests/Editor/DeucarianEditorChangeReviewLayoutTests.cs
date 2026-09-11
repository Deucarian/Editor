using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorChangeReviewLayoutTests
    {
        [UnityTest]
        public IEnumerator IntegratedReviewKeepsReadableRowsAndCommitBelowBothPanes()
        {
            int previous = DeucarianEditorAppearance.WorkspaceScalePercent;
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>(); window.Show();
            using var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Review geometry");
            using var review = new DeucarianEditorChangeReview(workspace.Content);
            try
            {
                review.UseSections(workspace.Tabs, true); review.SelectSection(1);
                review.Context.Root.AddToClassList("dw-local-source");
                var branch = review.Context.ReadOnly("review-branch", "Current branch", () => "codex/example");
                review.SetChanges(new[] { new DeucarianEditorChangeItem("file", "Editor/LongerNamedFolder/ExampleFile.cs", "Modified", true, true, () => { }, _ => { }) }, "file");
                review.SetDiff("ExampleFile.cs", "-old\n+new");
                var input = review.Commit.Text("commit-draft", "Commit message", () => "Retained draft", _ => { }, true);
                review.Commit.Action("commit-action", "Commit staged", () => { }, primary: true);
                foreach (var size in new[] { new Vector2(1586, 940), new Vector2(1180, 940), new Vector2(820, 650) })
                foreach (int scale in new[] { 75, 100, 125, 150 })
                {
                    window.rootVisualElement.style.width = size.x; window.rootVisualElement.style.height = size.y;
                    DeucarianEditorAppearance.WorkspaceScalePercent = scale;
                    for (int i = 0; i < 12; i++) yield return null;
                    string context = size + " at " + scale;
                    var split = review.Root.Q("review-split");
                    Assert.That(review.Commit.Root.worldBound.yMin, Is.GreaterThanOrEqualTo(split.worldBound.yMax - 1), context);
                    AssertInside(input, review.Root, context);
                    var row = review.Root.Q("review-change-file");
                    var path = row.Q<Label>(className: "dw-review-path");
                    var state = row.Q<Label>(className: "dw-muted");
                    Assert.That(state.worldBound.yMin, Is.GreaterThanOrEqualTo(path.worldBound.yMax - 1), context);
                    AssertInside(path, row, context); AssertInside(state, row, context);
                    Assert.That(row.Q<Toggle>().ClassListContains("dw-checkbox"), Is.True);
                    Assert.That(path.resolvedStyle.fontSize, Is.GreaterThanOrEqualTo(23), context);
                    Assert.That(review.Root.Q<Label>("review-diff-title").resolvedStyle.fontSize, Is.GreaterThanOrEqualTo(27), context);
                    Assert.That(input.Q(className: "unity-base-field__input").resolvedStyle.height, Is.EqualTo(82).Within(1), context);
                    Assert.That(state.resolvedStyle.unityTextAlign, Is.EqualTo(TextAnchor.MiddleLeft), context);
                    review.SelectSection(0);
                    for (int i = 0; i < 12; i++) yield return null;
                    if (!branch.parent.ClassListContains("dw-field-stacked"))
                        Assert.That(branch.worldBound.center.y, Is.EqualTo(branch.parent.Q<Label>(className: "dw-field-label").worldBound.center.y).Within(2), context);
                    review.SelectSection(1);
                }
            }
            finally { DeucarianEditorAppearance.WorkspaceScalePercent = previous; window.Close(); }
        }

        [UnityTest]
        public IEnumerator ReviewReflowsWithoutLosingFocusDraftsOrTheSharedScaleDock()
        {
            int previous = DeucarianEditorAppearance.WorkspaceScalePercent;
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Disposable review fixture"))
            using (var review = new DeucarianEditorChangeReview(workspace.Content))
            {
                try
                {
                    DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
                    DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);
                    workspace.Title.text = "Package development";
                    string draft = "Keep this commit draft";
                    review.Context.ReadOnly("review-test-path", "Repository", () => "D:/Repositories/Example-Package");
                    var action = review.Actions.Action("review-test-stage", "Stage selected changes", () => { }, primary: true);
                    var input = review.Commit.Text("review-test-message", "Commit message", () => draft, value => draft = value, true);
                    var item = new DeucarianEditorChangeItem("file", "Editor/LongerNamedFolder/ExampleFile.cs", "Modified", false, true, () => { }, _ => { });
                    var other = new DeucarianEditorChangeItem("meta", "Editor/ExampleFile.cs.meta", "New", false, false, () => { }, _ => { });
                    review.SetChanges(new[] { item }, "file");
                    review.SetDiff("Unstaged · ExampleFile.cs", "@@ -1 +1 @@\n-old line\n+new line");
                    var row = review.Root.Q("review-change-file");
                    foreach (var size in new[] { new Vector2(1480, 850), new Vector2(820, 650), new Vector2(620, 800) })
                    {
                        window.rootVisualElement.style.width = size.x;
                        window.rootVisualElement.style.height = size.y;
                        Rect? dockBounds = null;
                        foreach (int percent in new[] { 100, 150 })
                        {
                            DeucarianEditorAppearance.WorkspaceScalePercent = percent;
                            for (int frame = 0; frame < 10; frame++) yield return null;
                            string context = size + " at " + percent + "%";
                            var split = review.Root.Q("review-split");
                            Assert.That(split.ClassListContains("dw-split-stacked"), Is.EqualTo(split.resolvedStyle.width < 840), context);
                            AssertInside(review.Root.Q("review-list"), split, context);
                            AssertInside(review.Root.Q("review-diff"), split, context);
                            AssertInside(action, review.Root, context);
                            Assert.That(review.Root.Q("review-list-scroll").resolvedStyle.height, Is.GreaterThan(100), context);
                            Assert.That(review.Root.Q("review-diff-scroll").resolvedStyle.height, Is.GreaterThan(100), context);
                            var toggle = row.Q<Toggle>();
                            ((ScrollView)review.Root).ScrollTo(row);
                            toggle.Focus();
                            yield return null;
                            review.SetChanges(new[] { other, item }, "file");
                            review.RefreshForms();
                            Assert.That(review.Root.panel.focusController.focusedElement, Is.SameAs(toggle), context);
                            Assert.That(toggle.value, Is.True);
                            Assert.That(input.value, Is.EqualTo(draft));
                            var dock = window.rootVisualElement.Q("workspace-scale");
                            if (dockBounds.HasValue) Assert.That(dock.worldBound, Is.EqualTo(dockBounds.Value), context);
                            dockBounds = dock.worldBound;
                            Assert.That(workspace.Content.worldBound.yMax, Is.LessThanOrEqualTo(dock.worldBound.yMin + 1), context);
                            Assert.That(window.rootVisualElement.Query<SliderInt>("workspace-scale-slider").ToList().Count, Is.EqualTo(1));
                        }
                    }
                }
                finally { DeucarianEditorAppearance.WorkspaceScalePercent = previous; window.Close(); }
            }
        }

        private static void AssertInside(VisualElement child, VisualElement parent, string context)
        {
            Assert.That(child.worldBound.xMin, Is.GreaterThanOrEqualTo(parent.worldBound.xMin - 1), context);
            Assert.That(child.worldBound.xMax, Is.LessThanOrEqualTo(parent.worldBound.xMax + 1), context);
        }
    }
}
