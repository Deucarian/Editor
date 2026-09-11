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
        public IEnumerator WrappedContextNeverStarvesTheWorkingPageAtAnyScale()
        {
            int previous = DeucarianEditorAppearance.WorkspaceScalePercent;
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Review fixture"))
            {
                try
                {
                    workspace.Title.text = "Package Development";
                    workspace.Subtitle.text = "Work on a package. Test it here. Share it when ready.";
                    workspace.Tabs.Add(new DeucarianEditorChoiceBar(new[] { "Workspace", "Changes", "History" }, tabs: true));
                    var scope = new DeucarianEditorWorkspaceForm(workspace.Scope);
                    scope.Choice("layout-package", "Package", new[] { "Package with a longer display name" }, () => 0, _ => { });
                    scope.ReadOnly("layout-branch", "Branch", () => "codex/a-long-but-valid-feature-branch");
                    var body = DeucarianEditorWorkspaceControls.Scroll("layout-body"); workspace.Content.Add(body);
                    for (int i = 0; i < 20; i++) body.Add(new Label("Working page content " + i));
                    foreach (var size in new[] { new Vector2(1586, 940), new Vector2(820, 650), new Vector2(620, 650) })
                    foreach (int scale in new[] { 75, 100, 125, 150 })
                    {
                        Resize(window, size); DeucarianEditorAppearance.WorkspaceScalePercent = scale;
                        for (int i = 0; i < 12; i++) yield return null;
                        string context = size + " at " + scale + "%";
                        var heading = workspace.Root.Q<ScrollView>("workspace-context-scroll");
                        Assert.That(heading.worldBound.height, Is.GreaterThan(40), context);
                        Assert.That(body.worldBound.height, Is.GreaterThan(80), context);
                        Assert.That(body.worldBound.yMin, Is.GreaterThanOrEqualTo(heading.worldBound.yMax - 1), context);
                        Assert.That(body.worldBound.yMax, Is.LessThanOrEqualTo(window.rootVisualElement.Q("workspace-scale").worldBound.yMin + 1), context);
                        heading.ScrollTo(workspace.Scope);
                        for (int i = 0; i < 3; i++) yield return null;
                        Assert.That(workspace.Scope.worldBound.yMin, Is.LessThan(heading.worldBound.yMax), context);
                    }
                }
                finally { DeucarianEditorAppearance.WorkspaceScalePercent = previous; window.Close(); }
            }
        }

        [UnityTest]
        public IEnumerator LabControlsAdaptToTheirColumnAndRemainReachableAfterResizing()
        {
            int previousScale = DeucarianEditorAppearance.WorkspaceScalePercent;
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
                    foreach (int scale in new[] { 75, 100, 125, 150 })
                    {
                        Resize(window, size);
                        DeucarianEditorAppearance.WorkspaceScalePercent = scale;
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
                        var pageScroll = lab.Workspace.Root.Q<ScrollView>("lab-page-0");
                        var preview = lab.Workspace.Root.Q("lab-preview-scroll");
                        Assert.That(composer.resolvedStyle.height, Is.GreaterThan(70));
                        Assert.That(preview.resolvedStyle.height, Is.GreaterThan(70));
                        choice.Q<Button>("choice-0").Focus();
                        yield return null;
                        add.Focus();
                        for (int i = 0; i < 8; i++) yield return null;
                        Assert.That(composer.Contains(add), Is.True, "The action belongs to the form and follows its fields.");
                        Assert.That(add.focusController.focusedElement, Is.SameAs(add));
                        Assert.That(add.worldBound.yMax, Is.LessThanOrEqualTo(lab.Workspace.Content.worldBound.yMax + 1),
                            size + " scale=" + DeucarianEditorAppearance.WorkspaceScalePercent + " offset=" + pageScroll.scrollOffset +
                            " range=" + pageScroll.verticalScroller.highValue + " viewport=" + pageScroll.contentViewport.worldBound + " target=" + add.worldBound);
                        Assert.That(add.resolvedStyle.height, Is.GreaterThanOrEqualTo(42));
                        Assert.That(add.worldBound.width, Is.EqualTo(add.parent.worldBound.width).Within(2));
                        Assert.That(add.worldBound.yMin, Is.GreaterThanOrEqualTo(lab.Workspace.Content.worldBound.yMin));
                        var message = lab.Workspace.Root.Q<DeucarianEditorMessageRow>("layout-message");
                        AssertInside(message.Q<Button>(), message, size.ToString());
                        Assert.That(message.Q(className: "dw-message-state").parent.parent.ClassListContains("dw-message-text"), Is.True);
                        Assert.That(lab.Workspace.Content.resolvedStyle.height, Is.GreaterThan(100), size.ToString());
                        Assert.That(lab.Workspace.Footer.worldBound.yMax, Is.LessThanOrEqualTo(window.rootVisualElement.worldBound.yMax + 1));
                        Assert.That(title, Is.EqualTo("Example warning"));
                        Assert.That(lifetime, Is.Zero, "Resizing must not change the selected dismissal policy.");
                    }
                    lab.Composer.EnabledWhen(() => false);
                    lab.RefreshForms();
                    Assert.That(add.enabledInHierarchy, Is.False, "Actions follow the composer's enabled state.");
                    lab.Composer.EnabledWhen(() => true);
                    lab.RefreshForms();
                    choice.Q<Button>("choice-1").Focus();
                    yield return null;
                    using (var evt = NavigationSubmitEvent.GetPooled())
                    {
                        evt.target = choice.Q<Button>("choice-1");
                        choice.Q<Button>("choice-1").SendEvent(evt);
                    }
                    Assert.That(lifetime, Is.EqualTo(1));
                }
                finally { DeucarianEditorAppearance.WorkspaceScalePercent = previousScale; window.Close(); }
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
                        if (!collection.Collection.ClassListContains("dw-split-stacked"))
                        {
                            float paneRatio = collection.Details.resolvedStyle.width / collection.Collection.resolvedStyle.width;
                            Assert.That(paneRatio, Is.EqualTo(0.58f).Within(0.025f), "Details retain their share of the split: " + size);
                        }
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
                        var row = view.Root.Q<Button>("control-center-open-layout-tool");
                        Assert.That(row, Is.Not.Null);
                        Assert.That(row.ClassListContains("dw-summary-card"), Is.False);
                        AssertInside(row, content, size.ToString());
                        Assert.That(row.ClassListContains("dw-navigation-row"), Is.True);
                        Assert.That(row.Query<Button>().ToList().Count, Is.LessThanOrEqualTo(1), "One row is one navigation action.");
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
