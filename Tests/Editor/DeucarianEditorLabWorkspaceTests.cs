using System;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorLabWorkspaceTests
    {
        [Test]
        public void MessageRowsKeepIdentityDuringCountdownAndMoveBetweenLanes()
        {
            using (var view = new DeucarianEditorLabWorkspace(new VisualElement(), "Test", "Lab", "Test", () => { }, _ => { }))
            {
                var first = Item("one", "3 seconds", 0.75f);
                var second = Item("two", "Until resolved", null);
                view.SetMessages(new[] { first }, new[] { second }, 0);
                var row = view.Workspace.Root.Q<DeucarianEditorMessageRow>("one");
                view.SetMessages(new[] { Item("one", "2 seconds", 0.5f) }, new[] { second }, 0);
                Assert.That(view.Workspace.Root.Q<DeucarianEditorMessageRow>("one"), Is.SameAs(row));
                Assert.That(row.Q<Label>(className: "dw-message-state").text, Is.EqualTo("2 seconds"));
                view.SetMessages(new[] { second }, new[] { first }, 0);
                Assert.That(row.parent, Is.SameAs(view.Workspace.Root.Q<Foldout>("lab-overflow").contentContainer));
                view.SetMessages(Array.Empty<DeucarianEditorMessageData>(), Array.Empty<DeucarianEditorMessageData>(), 0);
                Assert.That(view.Workspace.Root.Q<DeucarianEditorMessageRow>(), Is.Null);
            }
        }

        [Test]
        public void TargetRefreshIsSilentAndRuntimeChoiceRequiresARealTarget()
        {
            int commands = 0;
            using (var view = new DeucarianEditorLabWorkspace(new VisualElement(), "Test", "Lab", "Test", () => { }, _ => commands++))
            {
                view.SetTargets(new[] { "a", "b" }, new[] { "First", "Second" }, "b", "Connected");
                view.SetTargets(Array.Empty<string>(), Array.Empty<string>(), null, "Editor preview");
                Assert.That(commands, Is.Zero);
                Assert.That(view.Workspace.Scope.Q<Button>("choice-1").enabledSelf, Is.False);
                Assert.Throws<ArgumentException>(() => view.SetTargets(new[] { "a" }, Array.Empty<string>(), null, ""));
            }
        }

        [Test]
        public void TestAndAppearanceKeepTheSameRowsAndProgress()
        {
            using var view = new DeucarianEditorLabWorkspace(new VisualElement(), "Test", "Lab", "Test", () => { }, _ => { });
            view.SetMessages(new[] { Item("one", "2 seconds", .5f) }, Array.Empty<DeucarianEditorMessageData>(), 0);
            var preview = view.PreviewRoot;
            var row = view.VisibleRows.Q<DeucarianEditorMessageRow>("one");
            var testParent = preview.parent;
            view.SelectTab(1);
            Assert.That(preview.parent, Is.Not.SameAs(testParent));
            Assert.That(view.VisibleRows.Q<DeucarianEditorMessageRow>("one"), Is.SameAs(row));
            Assert.That(row.Q<Label>(className: "dw-message-state").text, Is.EqualTo("2 seconds"));
            view.SelectTab(0);
            Assert.That(preview.parent, Is.SameAs(testParent));
        }

        [Test]
        public void ConsumerCanFinishAnExitBeforeRemovingTheRow()
        {
            using var view = new DeucarianEditorLabWorkspace(new VisualElement(), "Test", "Lab", "Test", () => { }, _ => { });
            Action complete = null;
            view.DismissRow = (_, done) => complete = done;
            view.SetMessages(new[] { Item("one", "", null) }, Array.Empty<DeucarianEditorMessageData>(), 0);
            var row = view.VisibleRows.Q<DeucarianEditorMessageRow>();
            view.SetMessages(Array.Empty<DeucarianEditorMessageData>(), Array.Empty<DeucarianEditorMessageData>(), 0);
            Assert.That(row.parent, Is.Not.Null);
            complete();
            Assert.That(row.parent, Is.Null);
        }

        [Test]
        public void ConsumerCanDeferMovingARowBetweenVisibleAndQueuedLanes()
        {
            using var view = new DeucarianEditorLabWorkspace(new VisualElement(), "Test", "Lab", "Test", () => { }, _ => { });
            view.SetMessages(new[] { Item("one", "", null) }, Array.Empty<DeucarianEditorMessageData>(), 0);
            var row = view.VisibleRows.Q<DeucarianEditorMessageRow>();
            VisualElement requestedDestination = null;
            view.PlaceRow = (_, destination, __) => requestedDestination = destination;
            view.SetMessages(Array.Empty<DeucarianEditorMessageData>(), new[] { Item("one", "", null) }, 0);
            Assert.That(row.parent, Is.SameAs(view.VisibleRows));
            Assert.That(requestedDestination, Is.SameAs(view.Workspace.Root.Q<Foldout>("lab-overflow").contentContainer));
            requestedDestination.Add(row);
            Assert.That(row.parent, Is.SameAs(requestedDestination));
        }

        private static DeucarianEditorMessageData Item(string id, string state, float? remaining)
            => new DeucarianEditorMessageData(id, id, "Body", DeucarianEditorStatus.Warning, state, remaining);
    }
}
