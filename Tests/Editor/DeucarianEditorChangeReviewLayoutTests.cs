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
