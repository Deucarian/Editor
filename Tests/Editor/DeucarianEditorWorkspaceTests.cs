using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorWorkspaceTests
    {
        [Test]
        public void WorkspaceComposesNamedRegionsAndUsesOwnedStyleSheet()
        {
            var root = new VisualElement();
            using (var workspace = new DeucarianEditorWorkspace(root, "Test project"))
            {
                Assert.That(root.Q("workspace-header"), Is.Not.Null);
                Assert.That(root.Q("workspace-navigation"), Is.SameAs(workspace.Navigation));
                Assert.That(root.Q("workspace-content"), Is.SameAs(workspace.Content));
                Assert.That(workspace.ContextButton.text, Is.EqualTo("Test project"));
                Assert.That(AssetDatabase.LoadAssetAtPath<StyleSheet>(DeucarianEditorWorkspace.StyleSheetPath), Is.Not.Null);
            }
        }

        [Test]
        public void SelectionDoesNotInvokeConsumerCommandsAndRejectsDuplicateIds()
        {
            using (var workspace = new DeucarianEditorWorkspace(new VisualElement(), "Test"))
            {
                int calls = 0;
                var first = workspace.AddNavigation("first", "First", DeucarianEditorIconIds.Info, () => calls++);
                var second = workspace.AddNavigation("second", "Second", DeucarianEditorIconIds.Info, () => calls++);
                workspace.SelectNavigation("second");
                Assert.That(first.ClassListContains("dw-selected"), Is.False);
                Assert.That(second.ClassListContains("dw-selected"), Is.True);
                Assert.That(second.text, Is.Empty, "Navigation text must be a flex child beside the icon.");
                Assert.That(second.Q<Label>(className: "dw-nav-label").text, Is.EqualTo("Second"));
                Assert.That(calls, Is.Zero);
                Assert.Throws<ArgumentException>(() => workspace.AddNavigation("first", "Duplicate", null, null));
            }
        }

        [TestCase(1536, false, false)]
        [TestCase(900, true, false)]
        [TestCase(420, true, true)]
        [TestCase(float.NaN, false, false)]
        public void LayoutUsesResponsiveClassesWithoutScalingText(float width, bool compact, bool narrow)
        {
            using (var workspace = new DeucarianEditorWorkspace(new VisualElement(), "Test"))
            {
                workspace.ApplyWidth(width);
                Assert.That(workspace.Root.ClassListContains("dw-compact"), Is.EqualTo(compact));
                Assert.That(workspace.Root.ClassListContains("dw-narrow"), Is.EqualTo(narrow));
            }
        }

        [Test]
        public void ChoicesHaveValidatedSilentSynchronization()
        {
            var choices = new DeucarianEditorChoiceBar(new[] { "One", "Two" });
            int calls = 0;
            choices.Changed += _ => calls++;
            choices.SetValueWithoutNotify(1);
            Assert.That(choices.Value, Is.EqualTo(1));
            Assert.That(choices.Q<Button>("choice-1").ClassListContains("dw-selected"), Is.True);
            Assert.That(calls, Is.Zero);
            choices.SetChoiceEnabled(0, false, "Requires a connected app");
            Assert.That(choices.Q<Button>("choice-0").tooltip, Does.Contain("connected app"));
            Assert.Throws<ArgumentOutOfRangeException>(() => choices.SetValueWithoutNotify(3));
        }

        [Test]
        public void MessageProgressSanitizesValuesWithoutOwningTiming()
        {
            var row = new DeucarianEditorMessageRow("Title", "Body", DeucarianEditorStatus.Warning, "Until resolved");
            row.SetProgress("Timed", float.NaN);
            Assert.That(row.Q(className: "dw-progress").style.display.value, Is.EqualTo(DisplayStyle.None));
            row.SetProgress("Timed", float.PositiveInfinity);
            Assert.That(row.Q(className: "dw-progress").style.display.value, Is.EqualTo(DisplayStyle.None));
            row.SetProgress("Timed", 2);
            Assert.That(row.Q(className: "dw-progress-fill").style.width.value.value, Is.EqualTo(100));
            row.SetProgress("Timed", -2);
            Assert.That(row.Q(className: "dw-progress-fill").style.width.value.value, Is.Zero);
        }

        [UnityTest]
        public IEnumerator NarrowWorkspaceKeepsFieldsAndFooterInsideTheWindow()
        {
            var window = ScriptableObject.CreateInstance<DeucarianEditorWorkspacePreviewWindow>();
            try
            {
                window.position = new Rect(30, 30, 420, 960);
                window.Show();
                window.rootVisualElement.style.width = 420;
                window.rootVisualElement.style.height = 960;
                window.Repaint();
                for (int frame = 0; frame < 5; frame++) yield return null;
                var root = window.rootVisualElement;
                Assert.That(root.Q(className: "deucarian-workspace").ClassListContains("dw-narrow"), Is.True);
                Assert.That(root.Q("workspace-form-preview").resolvedStyle.flexDirection, Is.EqualTo(FlexDirection.Column));
                Assert.That(root.Q("specimen-title").worldBound.xMax, Is.LessThanOrEqualTo(root.worldBound.xMax + 1));
                Assert.That(root.Q("specimen-add").worldBound.xMax, Is.LessThanOrEqualTo(root.worldBound.xMax + 1));
                Assert.That(root.Q("workspace-footer").worldBound.yMax, Is.LessThanOrEqualTo(root.worldBound.yMax + 1));
                Assert.That(root.Q("workspace-content").resolvedStyle.height, Is.GreaterThan(100));
            }
            finally { window.Close(); }
        }

        [UnityTest]
        public IEnumerator ChoiceKeyboardNavigationSkipsUnavailableItems()
        {
            var window = ScriptableObject.CreateInstance<DeucarianEditorWorkspacePreviewWindow>();
            try
            {
                window.Show();
                yield return null;
                var choices = new DeucarianEditorChoiceBar(new[] { "First", "Unavailable", "Third" });
                choices.SetChoiceEnabled(1, false, "Not connected");
                window.rootVisualElement.Add(choices);
                choices.Q<Button>("choice-0").Focus();
                yield return null;
                int calls = 0;
                choices.Changed += _ => calls++;
                using (var evt = KeyDownEvent.GetPooled(new Event { type = EventType.KeyDown, keyCode = KeyCode.RightArrow }))
                {
                    evt.target = choices.Q<Button>("choice-0");
                    choices.Q<Button>("choice-0").SendEvent(evt);
                }
                Assert.That(choices.Value, Is.EqualTo(2));
                Assert.That(calls, Is.EqualTo(1));
                using (var evt = KeyDownEvent.GetPooled(new Event { type = EventType.KeyDown, keyCode = KeyCode.RightArrow }))
                {
                    evt.target = choices.Q<Button>("choice-2");
                    choices.Q<Button>("choice-2").SendEvent(evt);
                }
                Assert.That(choices.Value, Is.Zero);
                Assert.That(calls, Is.EqualTo(2));
            }
            finally { window.Close(); }
        }

        [UnityTest]
        public IEnumerator PreviewRendersRealControlsAndClearlyLabelsItsScope()
        {
            var window = ScriptableObject.CreateInstance<DeucarianEditorWorkspacePreviewWindow>();
            try
            {
                window.position = new Rect(30, 30, 1536, 960);
                window.Show();
                yield return null;
                yield return null;
                var root = window.rootVisualElement;
                Assert.That(root.Q("specimen-message-rows").childCount, Is.EqualTo(5));
                Assert.That(root.Q<TextField>("specimen-title").value, Is.EqualTo("Example warning"));
                Assert.That(root.Q<Label>(className: "dw-preview-label").text, Does.Contain("NOT CONNECTED"));
                Assert.That(root.Q("workspace-content").resolvedStyle.height, Is.GreaterThan(100));
                Assert.That(root.Q("workspace-sidebar").resolvedStyle.width, Is.GreaterThan(100));
                Assert.That(root.Q("specimen-add").resolvedStyle.height, Is.GreaterThanOrEqualTo(42));
                Assert.That(root.Q("workspace-footer").worldBound.yMax, Is.LessThanOrEqualTo(root.worldBound.yMax + 1));
            }
            finally { window.Close(); }
        }
    }
}
