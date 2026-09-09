using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorWorkspaceScaleTests
    {
        private int previous;

        [SetUp] public void SavePreference() { previous = DeucarianEditorAppearance.WorkspaceScalePercent; DeucarianEditorAppearance.WorkspaceScalePercent = 100; }
        [TearDown] public void RestorePreference() => DeucarianEditorAppearance.WorkspaceScalePercent = previous;

        [TestCase(-100, 75)]
        [TestCase(125, 125)]
        [TestCase(1000, 150)]
        public void PreferenceClampsAndNotifiesOnlyOnChanges(int requested, int expected)
        {
            int changes = 0;
            Action changed = () => changes++;
            DeucarianEditorAppearance.Changed += changed;
            try
            {
                DeucarianEditorAppearance.WorkspaceScalePercent = requested;
                DeucarianEditorAppearance.WorkspaceScalePercent = requested;
                Assert.That(DeucarianEditorAppearance.WorkspaceScalePercent, Is.EqualTo(expected));
                Assert.That(DeucarianEditorProjectPreferences.GetInt(DeucarianEditorAppearance.ScaleKey), Is.EqualTo(expected));
                Assert.That(changes, Is.EqualTo(1));
            }
            finally { DeucarianEditorAppearance.Changed -= changed; }
        }

        [UnityTest]
        public IEnumerator ScaleFitsTheViewportPreservesDraftsAndWorksAfterDetachAndReset()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            var page = new VisualElement();
            window.rootVisualElement.Add(page);
            using (var workspace = new DeucarianEditorWorkspace(page, "Scale test"))
            {
                try
                {
                    workspace.Title.text = "Control Center";
                    DeucarianEditorWorkspaceNavigation.Populate(workspace, DeucarianToolIds.ControlCenter);
                    var input = new TextField { value = "Keep this draft", name = "scale-draft" };
                    var scroll = DeucarianEditorWorkspaceControls.Scroll("scale-test-scroll");
                    scroll.Add(input);
                    workspace.Content.Add(scroll);
                    DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
                    DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);
                    var slider = workspace.Root.Q<SliderInt>("workspace-scale-slider");
                    foreach (var size in new[] { new Vector2(1480, 697), new Vector2(1319, 697), new Vector2(820, 650) })
                    {
                        window.rootVisualElement.style.width = size.x;
                        window.rootVisualElement.style.height = size.y;
                        foreach (int percent in new[] { 75, 100, 125, 150 })
                        {
                            slider.value = percent;
                            for (int frame = 0; frame < 10; frame++) yield return null;
                            string context = size + " at " + percent + "%";
                            Assert.That(workspace.Root.worldBound.xMin, Is.EqualTo(page.worldBound.xMin).Within(1), context);
                            Assert.That(workspace.Root.worldBound.yMin, Is.EqualTo(page.worldBound.yMin).Within(1), context);
                            Assert.That(workspace.Root.worldBound.width, Is.EqualTo(page.worldBound.width).Within(2), context);
                            Assert.That(workspace.Root.worldBound.height, Is.EqualTo(page.worldBound.height).Within(2), context);
                            Assert.That(workspace.Root.resolvedStyle.width, Is.EqualTo(size.x * 100 / percent).Within(2), context);
                            Assert.That(workspace.Footer.worldBound.yMax, Is.LessThanOrEqualTo(page.worldBound.yMax + 2), context);
                            Assert.That(slider.worldBound.xMax, Is.LessThanOrEqualTo(page.worldBound.xMax + 2), context);
                            Assert.That(workspace.Content.resolvedStyle.height, Is.GreaterThan(30), context);
                            Assert.That(input.value, Is.EqualTo("Keep this draft"));
                            Assert.That(workspace.Root.ClassListContains("dw-compact"), Is.EqualTo(workspace.Root.resolvedStyle.width < 1100), context);
                        }
                    }
                    page.RemoveFromHierarchy();
                    DeucarianEditorAppearance.WorkspaceScalePercent = 90;
                    window.rootVisualElement.Add(page);
                    for (int frame = 0; frame < 8; frame++) yield return null;
                    Assert.That(slider.value, Is.EqualTo(90), "Cached pages adopt the preference when revisited.");
                    var reset = workspace.Root.Q<Button>("workspace-scale-reset");
                    reset.Focus();
                    yield return null;
                    using (var evt = NavigationSubmitEvent.GetPooled()) { evt.target = reset; reset.SendEvent(evt); }
                    Assert.That(DeucarianEditorAppearance.WorkspaceScalePercent, Is.EqualTo(100));
                    workspace.Dispose();
                    DeucarianEditorAppearance.WorkspaceScalePercent = 120;
                    Assert.That(slider.value, Is.EqualTo(100), "Disposed pages release their preference subscription.");
                }
                finally { window.Close(); }
            }
        }
    }
}
