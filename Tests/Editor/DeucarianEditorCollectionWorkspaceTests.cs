using System;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorCollectionWorkspaceTests
    {
        [Test]
        public void RefreshRetainsRowsAndDoesNotInvokeCommands()
        {
            int calls = 0;
            using (var view = Create())
            {
                var first = new DeucarianEditorCollectionItem("a", "First", "One", "Ready", () => calls++);
                var second = new DeucarianEditorCollectionItem("b", "Second", "Two", "Ready", () => calls++);
                view.SetItems(new[] { first, second }, "a", "Empty");
                var row = view.Collection.Q("workspace-item-a");
                view.SetItems(new[] { second, first }, "b", "Empty");
                Assert.AreSame(row, view.Collection.Q("workspace-item-a"));
                Assert.IsFalse(row.ClassListContains("dw-selected"));
                Assert.IsTrue(view.Collection.Q("workspace-item-b").ClassListContains("dw-selected"));
                Assert.AreEqual(0, calls);
                view.SetItems(Array.Empty<DeucarianEditorCollectionItem>(), null, "No matches");
                Assert.IsNull(view.Collection.Q("workspace-item-a"));
                Assert.AreEqual("No matches", view.Collection.Q<Label>(className: "dw-empty").text);
            }
        }

        [Test]
        public void DuplicateIdsAreRejectedBeforeChangingExistingRows()
        {
            using (var view = Create())
            {
                var item = new DeucarianEditorCollectionItem("a", "First", "One", "", null);
                view.SetItems(new[] { item }, null, "Empty");
                var row = view.Collection.Q("workspace-item-a");
                Assert.Throws<ArgumentException>(() => view.SetItems(new[] { item, item }, null, ""));
                Assert.AreSame(row, view.Collection.Q("workspace-item-a"));
            }
        }

        [Test]
        public void CollectionSearchDoesNotHideToolNavigation()
        {
            using (var view = Create())
            {
                view.Workspace.SearchField.value = "some package";
                Assert.AreNotEqual(DisplayStyle.None,
                    view.Workspace.Navigation.Q<Button>("workspace-nav-" + DeucarianToolIds.ControlCenter).style.display.value);
            }
        }

        private static DeucarianEditorCollectionWorkspace Create() => new DeucarianEditorCollectionWorkspace(
            new VisualElement(), "Test project", "Collection", "Read-only refresh", "audio", "Search items…");
    }
}
