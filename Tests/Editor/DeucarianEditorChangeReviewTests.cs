using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorChangeReviewTests
    {
        [Test]
        public void RefreshKeepsControlsAndCallerSelectionWithoutRunningActions()
        {
            int calls = 0;
            using (var view = new DeucarianEditorChangeReview(new VisualElement()))
            {
                var first = Item("first", true, () => calls++, _ => calls++);
                var second = Item("second", false, () => calls++, _ => calls++);
                view.SetChanges(new[] { first, second }, "first");
                var row = view.Root.Q("review-change-first");
                var toggle = row.Q<Toggle>();
                view.SetChanges(new[] { second, first }, "second");
                Assert.That(view.Root.Q("review-change-first"), Is.SameAs(row));
                Assert.That(row.Q<Toggle>(), Is.SameAs(toggle));
                Assert.That(toggle.value, Is.True);
                Assert.That(row.ClassListContains("dw-selected"), Is.False);
                Assert.That(view.Root.Q("review-change-second").ClassListContains("dw-selected"), Is.True);
                Assert.That(calls, Is.Zero);
                view.SetChanges(Array.Empty<DeucarianEditorChangeItem>(), null);
                Assert.That(view.Root.Q("review-change-first"), Is.Null);
                Assert.That(view.Root.Q<Label>("review-change-note").text, Does.Contain("No changes"));
            }
        }

        [Test]
        public void SelectionInvokesOnlyTheCurrentCallerCallbackAndNeverStagesImplicitly()
        {
            int oldCalls = 0, newCalls = 0, inspectCalls = 0;
            using (var view = new DeucarianEditorChangeReview(new VisualElement()))
            {
                view.SetChanges(new[] { Item("one", false, () => inspectCalls++, _ => oldCalls++) }, null);
                var toggle = view.Root.Q<Toggle>();
                view.SetChanges(new[] { Item("one", false, () => inspectCalls++, _ => newCalls++) }, null);
                toggle.value = true;
                Assert.That(oldCalls, Is.Zero);
                Assert.That(newCalls, Is.EqualTo(1));
                Assert.That(inspectCalls, Is.Zero);
                view.SetChanges(new[] { Item("one", false, null, null) }, null);
                Assert.That(toggle.value, Is.False, "The caller remains authoritative.");
                Assert.That(toggle.enabledSelf, Is.False);
            }
        }

        [Test]
        public void InvalidVisibleIdsDoNotPartiallyReplaceThePreviousSnapshot()
        {
            using (var view = new DeucarianEditorChangeReview(new VisualElement()))
            {
                var item = Item("one", false, null, null);
                view.SetChanges(new[] { item }, null);
                var row = view.Root.Q("review-change-one");
                Assert.Throws<ArgumentException>(() => view.SetChanges(new[] { item, item }, null));
                Assert.Throws<ArgumentException>(() => view.SetChanges(new[] { item, Item("", false, null, null) }, null));
                Assert.Throws<ArgumentNullException>(() => view.SetChanges(null, null));
                Assert.That(view.Root.Q("review-change-one"), Is.SameAs(row));
            }
        }

        [Test]
        public void LargeListsAndDiffsRemainBoundedAndDiscloseIncompleteOutput()
        {
            using (var view = new DeucarianEditorChangeReview(new VisualElement()))
            {
                view.SetChanges(Enumerable.Range(0, 550).Select(i => Item("file-" + i, false, null, null)).ToArray(), null);
                Assert.That(view.Root.Q("review-change-rows").childCount, Is.EqualTo(500));
                Assert.That(view.Root.Q<Label>("review-change-note").text, Does.Contain("500 of 550"));
                view.SetDiff("Large diff", new string('x', 70000));
                Assert.That(view.Root.Q<TextField>("review-diff-text").value.Length, Is.EqualTo(65536));
                Assert.That(view.Root.Q<Label>("review-diff-note").text, Does.Contain("Partial"));
                view.SetHistory(Enumerable.Range(0, 25).Select(i => new DeucarianEditorHistoryItem("revision-" + i, "Title", "Branch")).ToArray());
                Assert.That(view.Root.Q("review-history-rows").childCount, Is.EqualTo(20));
                Assert.That(view.Root.Q<Label>("review-history-note").text, Does.Contain("20 of 25"));
                Assert.That(view.Root.Q<Foldout>("review-history").value, Is.False);
            }
        }

        [Test]
        public void BinaryAndPartialDiffsAreExplicitAndRowTextIsLiteral()
        {
            using (var view = new DeucarianEditorChangeReview(new VisualElement()))
            {
                view.SetChanges(new[] { new DeucarianEditorChangeItem("file", "<b>file.cs</b>", "Renamed", true, false,
                    null, null, "Previous: old.cs · Related: file.cs.meta") }, "file");
                Assert.That(view.Root.Q<Label>(className: "dw-review-path").enableRichText, Is.False);
                Assert.That(view.Root.Query<Label>().ToList().Any(label => label.text.Contains("file.cs.meta")), Is.True);
                view.SetDiff("Image", "Binary data should not render", binary: true);
                var field = view.Root.Q<TextField>("review-diff-text");
                Assert.That(field.value, Is.Empty);
                Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(view.Root.Q<Label>("review-diff-note").text, Does.Contain("Binary"));
                view.SetDiff("Text", "-old\n+new", truncated: true);
                Assert.That(field.isReadOnly, Is.True);
                Assert.That(view.Root.Q<Label>("review-diff-note").text, Does.Contain("Partial"));
            }
        }

        [Test]
        public void HistoryRefreshPreservesDisclosureAndRejectsDuplicatesAtomically()
        {
            using (var view = new DeucarianEditorChangeReview(new VisualElement()))
            {
                var item = new DeucarianEditorHistoryItem("one", "Change title", "abc123 · feature/example");
                view.SetHistory(new[] { item });
                var foldout = view.Root.Q<Foldout>("review-history");
                var row = view.Root.Q("review-history-one");
                foldout.value = true;
                view.SetHistory(new[] { item });
                Assert.That(foldout.value, Is.True);
                Assert.That(view.Root.Q("review-history-one"), Is.SameAs(row));
                Assert.Throws<ArgumentException>(() => view.SetHistory(new[] { item, item }));
                Assert.That(view.Root.Q("review-history-one"), Is.SameAs(row));
                view.SetHistory(Array.Empty<DeucarianEditorHistoryItem>());
                Assert.That(foldout.style.display.value, Is.EqualTo(DisplayStyle.None));
            }
        }

        [Test]
        public void DisposeDetachesOnlyItsReviewAndIsIdempotent()
        {
            var parent = new VisualElement();
            var sibling = new Label("Keep");
            parent.Add(sibling);
            var view = new DeucarianEditorChangeReview(parent);
            view.Dispose();
            view.Dispose();
            Assert.That(parent.childCount, Is.EqualTo(1));
            Assert.That(parent[0], Is.SameAs(sibling));
            Assert.Throws<ObjectDisposedException>(() => view.SetSummary("No longer active"));
        }

        private static DeucarianEditorChangeItem Item(string id, bool selected, Action inspect, Action<bool> select)
            => new DeucarianEditorChangeItem(id, id + ".cs", "Modified", false, selected, inspect, select);
    }
}
