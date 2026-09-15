using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianWorkspaceControlAlignmentTests
    {
        private sealed class ControlWindow : EditorWindow { }

        [UnityTest]
        public IEnumerator StepperEmitsOnlyOneClampedChangeFromItsNestedInput()
        {
            var window = ScriptableObject.CreateInstance<ControlWindow>();
            window.Show();
            try
            {
                var stepper = new DeucarianEditorStepper(1, 5);
                window.rootVisualElement.Add(stepper);
                yield return null;
                int calls = 0, observed = -1;
                stepper.RegisterValueChangedCallback(evt => { calls++; observed = evt.newValue; });
                var input = stepper.Q<IntegerField>();
                input.value = 99;
                Assert.That(calls, Is.EqualTo(1));
                Assert.That(observed, Is.EqualTo(5));
                Assert.That(stepper.value, Is.EqualTo(5));
                Assert.That(input.value, Is.EqualTo(5));
                input.value = 100;
                Assert.That(calls, Is.EqualTo(1), "An unchanged clamped value must not leak the nested input event.");
                Assert.That(input.value, Is.EqualTo(5));
                stepper.SetValueWithoutNotify(3);
                Assert.That(calls, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(window); }
        }

        [UnityTest]
        public IEnumerator SliderThumbsAreCenteredOnTheTrackInFormsAndTheStationaryDock()
        {
            int originalScale = DeucarianEditorAppearance.WorkspaceScalePercent;
            var window = ScriptableObject.CreateInstance<ControlWindow>();
            window.position = new Rect(20, 20, 1586, 940);
            window.Show();
            try
            {
                using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Layout test"))
                {
                    var form = new DeucarianEditorWorkspaceForm(workspace.Content);
                    var slider = form.Slider("test-volume", "Volume", 0, 1, () => .5f, _ => { });
                    var dock = window.rootVisualElement.Q(className: "dw-scale-dock");
                    Assert.That(dock, Is.Not.Null);
                    var scale = dock.Q<SliderInt>();
                    Assert.That(scale, Is.Not.Null);
                    foreach (int percent in new[] { 75, 100, 125, 150 })
                    {
                        DeucarianEditorAppearance.WorkspaceScalePercent = percent;
                        for (int frame = 0; frame < 12; frame++) yield return null;
                        AssertCentered(slider);
                        AssertCentered(scale);
                    }
                }
            }
            finally { Object.DestroyImmediate(window); DeucarianEditorAppearance.WorkspaceScalePercent = originalScale; }
        }

        [Test]
        public void AutomaticNavigationExpansionDoesNotAccumulateAcrossPages()
        {
            var state = new DeucarianEditorNavigationState();
            Assert.That(state.IsExpanded("Theming", true), Is.True);
            Assert.That(state.IsExpanded("Theming", false), Is.False);
            state.SetExpanded("Theming", true);
            Assert.That(state.IsExpanded("Theming", false), Is.True, "Explicit user expansion is preserved.");
            state.SetExpanded("Theming", false);
            Assert.That(state.IsExpanded("Theming", true), Is.False, "Explicit collapse also wins.");
        }

        private static void AssertCentered(VisualElement slider)
        {
            var track = slider.Q(className: "unity-base-slider__tracker");
            var thumb = slider.Q(className: "unity-base-slider__dragger");
            Assert.That(track.worldBound.width, Is.GreaterThan(10));
            Assert.That(Mathf.Abs(track.worldBound.center.y - thumb.worldBound.center.y), Is.LessThanOrEqualTo(1),
                slider.name + " thumb " + thumb.worldBound + " vs track " + track.worldBound +
                " thumb top/margin " + thumb.resolvedStyle.top + "/" + thumb.resolvedStyle.marginTop +
                " track top/margin " + track.resolvedStyle.top + "/" + track.resolvedStyle.marginTop +
                " positions " + thumb.resolvedStyle.position + "/" + track.resolvedStyle.position +
                " parents " + thumb.parent.name + ":" + thumb.parent.worldBound + " vs " +
                track.parent.name + ":" + track.parent.worldBound + " classes " + string.Join(" ", slider.GetClasses()));
        }
    }
}
