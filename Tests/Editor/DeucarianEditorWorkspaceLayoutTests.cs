using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorWorkspaceLayoutTests
    {
        [UnityTest]
        public IEnumerator LabControlsAdaptToTheirColumnAndRemainReachableAfterResizing()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            using (var lab = new DeucarianEditorLabWorkspace(window.rootVisualElement, "Test", "Notifications", "Create a test message.", () => { }, _ => { }))
            {
                try
                {
                    string title = "Example warning";
                    int lifetime = 0;
                    lab.Composer.Choice("layout-type", "Type", new[] { "Warning", "Error", "Information" }, () => 0, _ => { });
                    lab.Composer.Text("layout-title", "Title", () => title, value => title = value);
                    lab.Composer.Text("layout-body", "Message", () => "A long message that wraps across several lines.", _ => { }, true);
                    var choice = lab.Composer.Segments("layout-dismissal", "Dismissal", new[] { "Resolve manually", "After a delay" }, () => lifetime, value => lifetime = value);
                    var add = lab.Composer.Action("layout-add", "Add test message", () => { }, primary: true);
                    lab.SetMessages(new[] { new DeucarianEditorMessageData("layout-message", "A longer warning title", "Please resolve this test message to simulate recovery.", DeucarianEditorStatus.Warning, "Until resolved", null, "Resolve", () => { }) }, Array.Empty<DeucarianEditorMessageData>(), 0);
                    foreach (var size in Sizes())
                    {
                        Resize(window, size);
                        for (int i = 0; i < 8; i++) yield return null;
                        var composer = lab.Workspace.Root.Q(className: "dw-lab-composer");
                        foreach (var field in composer.Query(className: "dw-field").ToList())
                        {
                            var input = field.Q(className: "dw-field-input");
                            AssertInside(input, field, size.ToString());
                            AssertInside(field, composer, size.ToString());
                            Assert.That(field.ClassListContains("dw-field-stacked"), Is.EqualTo(field.resolvedStyle.width < 520));
                        }
                        foreach (var button in choice.Query<Button>().ToList()) AssertInside(button, choice, size.ToString());
                        AssertInside(add, composer, size.ToString());
                        var composerScroll = (ScrollView)composer;
                        var previewScroll = lab.Workspace.Root.Q<ScrollView>("lab-preview-scroll");
                        Assert.That(composerScroll.resolvedStyle.height, Is.GreaterThan(70));
                        Assert.That(previewScroll.resolvedStyle.height, Is.GreaterThan(70));
                        var previousPreviewOffset = previewScroll.scrollOffset;
                        composerScroll.ScrollTo(add);
                        yield return null;
                        Assert.That(previewScroll.scrollOffset, Is.EqualTo(previousPreviewOffset));
                        Assert.That(add.worldBound.yMax, Is.LessThanOrEqualTo(composerScroll.contentViewport.worldBound.yMax + 1), "The primary action must be reachable by scrolling its own pane.");
                        var message = lab.Workspace.Root.Q<DeucarianEditorMessageRow>("layout-message");
                        AssertInside(message.Q<Button>(), message, size.ToString());
                        Assert.That(message.Q(className: "dw-message-state").parent.parent.ClassListContains("dw-message-text"), Is.True);
                        Assert.That(lab.Workspace.Content.resolvedStyle.height, Is.GreaterThan(100), size.ToString());
                        Assert.That(lab.Workspace.Footer.worldBound.yMax, Is.LessThanOrEqualTo(window.rootVisualElement.worldBound.yMax + 1));
                        Assert.That(title, Is.EqualTo("Example warning"));
                        Assert.That(lifetime, Is.Zero, "Resizing must not change the selected dismissal policy.");
                    }
                    choice.Q<Button>("choice-1").Focus();
                    yield return null;
                    using (var evt = NavigationSubmitEvent.GetPooled())
                    {
                        evt.target = choice.Q<Button>("choice-1");
                        choice.Q<Button>("choice-1").SendEvent(evt);
                    }
                    Assert.That(lifetime, Is.EqualTo(1));
                }
                finally { window.Close(); }
            }
        }

        [UnityTest]
        public IEnumerator CollectionSelectionAndDetailActionsDoNotCreateNestedOrStretchedBoxes()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            using (var collection = new DeucarianEditorCollectionWorkspace(window.rootVisualElement, "Test", "Audio", "Preview project sounds.", "audio", "Find a role"))
            {
                try
                {
                    collection.SetItems(new[] { new DeucarianEditorCollectionItem("key", "Special Key", "Button_22_click", "Experience palette", () => { }, "Play", () => { }) }, "key", "Empty");
                    var form = new DeucarianEditorWorkspaceForm(collection.Details);
                    form.ReadOnly("layout-volume", "Volume / pitch", () => "0.32 · 0.95–1.05");
                    var play = form.Action("layout-play", "Play processed", () => { }, primary: true);
                    foreach (var size in Sizes())
                    {
                        Resize(window, size);
                        for (int i = 0; i < 8; i++) yield return null;
                        var row = collection.Collection.Q("workspace-item-key");
                        var select = row.Q<Button>(className: "dw-collection-select");
                        select.Focus();
                        yield return null;
                        Assert.That(select.resolvedStyle.borderLeftWidth, Is.Zero);
                        Assert.That(select.resolvedStyle.borderTopWidth, Is.Zero);
                        Assert.That(select.resolvedStyle.borderBottomWidth, Is.EqualTo(2), "Keyboard focus keeps a visible underline, without a second enclosing border.");
                        AssertInside(row, collection.Collection, size.ToString());
                        AssertInside(play, collection.Details, size.ToString());
                        Assert.That(play.resolvedStyle.width, Is.LessThan(260), size.ToString());
                        Assert.That(collection.Details.resolvedStyle.height, Is.GreaterThan(70), size.ToString());
                    }
                }
                finally { window.Close(); }
            }
        }

        [UnityTest]
        public IEnumerator ControlCenterUsesFlatToolRowsAndContentAlignedWrappingCards()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Test"))
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                try
                {
                    DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);
                    DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
                    workspace.Content.Add(view.Root);
                    var tools = new[] { new DeucarianToolDescriptor("layout-tool", "Audio Palette Lab", "Audition project audio by semantic role and experience.", DeucarianControlCenterArea.Developer, () => { }, "com.deucarian.editor") };
                    var cards = Enumerable.Range(0, 3).Select(i => new DeucarianControlCenterCard("layout-card-" + i, DeucarianControlCenterArea.Overview, "Project readiness", "All installed package checks pass.", "com.deucarian.editor", statusText: "Ready")).ToArray();
                    var snapshot = new DeucarianControlCenterSnapshot(DateTime.UtcNow, cards, Array.Empty<DeucarianControlCenterSection>(), tools);
                    foreach (var size in Sizes())
                    {
                        Resize(window, size);
                        view.Render(snapshot, DeucarianControlCenterArea.Overview, null, "");
                        for (int i = 0; i < 8; i++) yield return null;
                        var content = view.Root.Q<ScrollView>("control-center-content");
                        Assert.That(content.Query<Label>().ToList().Any(label => label.text == "Overview"), Is.False);
                        Assert.That(content.worldBound.xMin, Is.EqualTo(workspace.Content.worldBound.xMin).Within(1));
                        foreach (var card in content.Query(className: "dw-summary-card").ToList()) AssertInside(card, content, size.ToString());
                        view.Render(snapshot, DeucarianControlCenterArea.Developer, null, "");
                        for (int i = 0; i < 8; i++) yield return null;
                        var row = view.Root.Q("control-center-tool-layout-tool");
                        Assert.That(row.ClassListContains("dw-summary-card"), Is.False);
                        foreach (var button in row.Query<Button>().ToList())
                        {
                            AssertInside(button, row, size.ToString());
                            Assert.That(button.resolvedStyle.width, Is.LessThan(120));
                        }
                    }
                }
                finally { window.Close(); }
            }
        }

        private static Vector2[] Sizes() => new[] { new Vector2(1908, 950), new Vector2(1319, 697), new Vector2(1180, 700), new Vector2(820, 650), new Vector2(620, 800), new Vector2(1319, 697) };

        private static void Resize(EditorWindow window, Vector2 size)
        {
            window.rootVisualElement.style.width = size.x;
            window.rootVisualElement.style.height = size.y;
            window.Repaint();
        }

        private static void AssertInside(VisualElement child, VisualElement parent, string context)
        {
            Assert.That(child.worldBound.xMin, Is.GreaterThanOrEqualTo(parent.worldBound.xMin - 1), child.name + " left " + context);
            Assert.That(child.worldBound.xMax, Is.LessThanOrEqualTo(parent.worldBound.xMax + 1), child.name + " right " + context);
        }
    }

    internal sealed class WorkspaceLayoutTestWindow : EditorWindow { }
}
