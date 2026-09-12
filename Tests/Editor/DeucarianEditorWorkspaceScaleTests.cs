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
        private int previousLegacy;
        private int previousEarlier;

        [SetUp] public void SavePreference()
        {
            previous = DeucarianEditorProjectPreferences.GetInt(DeucarianEditorAppearance.ScaleKey, int.MinValue);
            previousLegacy = DeucarianEditorProjectPreferences.GetInt(DeucarianEditorAppearance.LegacyScaleKey, int.MinValue);
            previousEarlier = DeucarianEditorProjectPreferences.GetInt(DeucarianEditorAppearance.EarlierScaleKey, int.MinValue);
            DeucarianEditorProjectPreferences.Delete(DeucarianEditorAppearance.EarlierScaleKey);
            DeucarianEditorProjectPreferences.Delete(DeucarianEditorAppearance.LegacyScaleKey);
            DeucarianEditorProjectPreferences.SetInt(DeucarianEditorAppearance.ScaleKey, 100);
        }
        [TearDown] public void RestorePreference()
        {
            Restore(DeucarianEditorAppearance.ScaleKey, previous);
            Restore(DeucarianEditorAppearance.LegacyScaleKey, previousLegacy);
            Restore(DeucarianEditorAppearance.EarlierScaleKey, previousEarlier);
        }

        private static void Restore(string key, int value)
        {
            if (value == int.MinValue) DeucarianEditorProjectPreferences.Delete(key);
            else DeucarianEditorProjectPreferences.SetInt(key, value);
        }

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

        [Test]
        public void NewBaselineDefaultsToThePreviousSeventyFivePercentSize()
        {
            DeucarianEditorProjectPreferences.Delete(DeucarianEditorAppearance.ScaleKey);
            Assert.That(DeucarianEditorAppearance.WorkspaceScalePercent, Is.EqualTo(100));
            Assert.That(DeucarianEditorWorkspaceScale.DefaultScale, Is.EqualTo(0.675f * 0.75f).Within(0.00001f));
        }

        [TestCase(75, 100)]
        [TestCase(90, 120)]
        [TestCase(100, 100)]
        [TestCase(125, 150)]
        [TestCase(150, 150)]
        public void LegacyPreferenceRebasesOnceWithinTheSupportedRange(int legacy, int expected)
        {
            DeucarianEditorProjectPreferences.Delete(DeucarianEditorAppearance.ScaleKey);
            DeucarianEditorProjectPreferences.SetInt(DeucarianEditorAppearance.LegacyScaleKey, legacy);
            Assert.That(DeucarianEditorAppearance.WorkspaceScalePercent, Is.EqualTo(expected));
            DeucarianEditorProjectPreferences.SetInt(DeucarianEditorAppearance.LegacyScaleKey, 75);
            Assert.That(DeucarianEditorAppearance.WorkspaceScalePercent, Is.EqualTo(expected));
            DeucarianEditorAppearance.WorkspaceScalePercent = 100;
            Assert.That(DeucarianEditorAppearance.WorkspaceScalePercent, Is.EqualTo(100));
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
                    var slider = page.Q<SliderInt>("workspace-scale-slider");
                    workspace.Footer.Clear();
                    workspace.Footer.Add(new Label("Consumer-owned status footer"));
                    Assert.That(page.Q<SliderInt>("workspace-scale-slider"), Is.SameAs(slider), "Consumer footer replacement must not remove shared scale controls.");
                    var viewport = page.Q("workspace-scale-viewport");
                    foreach (var size in new[] { new Vector2(1480, 697), new Vector2(1319, 697), new Vector2(820, 650) })
                    {
                        window.rootVisualElement.style.width = size.x;
                        window.rootVisualElement.style.height = size.y;
                        Rect? sliderBounds = null;
                        Rect? resetBounds = null;
                        foreach (int percent in new[] { 75, 100, 125, 150 })
                        {
                            slider.value = percent;
                            for (int frame = 0; frame < 10; frame++) yield return null;
                            string context = size + " at " + percent + "%";
                            Assert.That(workspace.Root.worldBound.xMin, Is.EqualTo(viewport.worldBound.xMin).Within(1), context);
                            Assert.That(workspace.Root.worldBound.yMin, Is.EqualTo(viewport.worldBound.yMin).Within(1), context);
                            Assert.That(workspace.Root.worldBound.width, Is.EqualTo(viewport.worldBound.width).Within(2), context);
                            Assert.That(workspace.Root.worldBound.height, Is.EqualTo(viewport.worldBound.height).Within(2), context);
                            Assert.That(workspace.Root.resolvedStyle.width, Is.EqualTo(size.x * 100 / percent / DeucarianEditorWorkspaceScale.DefaultScale).Within(2), context);
                            Assert.That(workspace.Footer.worldBound.yMax, Is.LessThanOrEqualTo(page.worldBound.yMax + 2), context);
                            Assert.That(slider.worldBound.xMax, Is.LessThanOrEqualTo(page.worldBound.xMax + 2), context);
                            Assert.That(workspace.Content.resolvedStyle.height, Is.GreaterThan(30), context);
                            if (workspace.Root.resolvedStyle.width < 1470)
                            {
                                Assert.That(page.Q("workspace-navigation-rail").resolvedStyle.display,
                                    Is.EqualTo(DisplayStyle.Flex), context + " uses the compact icon rail");
                                Assert.That(workspace.Sidebar.resolvedStyle.width, Is.EqualTo(110).Within(1), context);
                                Assert.That(page.Q("workspace-navigation-scroll").resolvedStyle.display, Is.EqualTo(DisplayStyle.None), context);
                            }
                            var resetButton = page.Q<Button>("workspace-scale-reset");
                            if (sliderBounds.HasValue)
                            {
                                Assert.That(Vector2.Distance(slider.worldBound.position, sliderBounds.Value.position), Is.LessThan(0.1f), context + " slider must not move");
                                Assert.That(slider.worldBound.size, Is.EqualTo(sliderBounds.Value.size), context + " slider must not resize");
                                Assert.That(resetButton.worldBound, Is.EqualTo(resetBounds.Value), context + " reset must not move or resize");
                            }
                            sliderBounds = slider.worldBound;
                            resetBounds = resetButton.worldBound;
                            Assert.That(viewport.worldBound.yMax, Is.LessThanOrEqualTo(slider.worldBound.yMin + 1), context + " the fixed dock must not cover content");
                            Assert.That(input.value, Is.EqualTo("Keep this draft"));
                            Assert.That(workspace.Root.ClassListContains("dw-compact"), Is.EqualTo(workspace.Root.resolvedStyle.width < 1470), context);
                        }
                    }
                    page.RemoveFromHierarchy();
                    DeucarianEditorAppearance.WorkspaceScalePercent = 90;
                    window.rootVisualElement.Add(page);
                    for (int frame = 0; frame < 8; frame++) yield return null;
                    Assert.That(slider.value, Is.EqualTo(90), "Cached pages adopt the preference when revisited.");
                    var reset = page.Q<Button>("workspace-scale-reset");
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
