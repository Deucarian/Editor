using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorNativeInspectorTests
    {
        private sealed class Host : EditorWindow { }
        private sealed class Settings : ScriptableObject
        {
            public bool active;
            [UnityEngine.Range(0f, 1f)] public float amount = .25f;
            public string caption = "Original";
        }

        [UnityTest] public IEnumerator NativePropertyBindingsKeepSharedControlsAndFollowUndo()
        {
            var asset = ScriptableObject.CreateInstance<Settings>();
            var window = ScriptableObject.CreateInstance<Host>(); window.position = new Rect(80, 80, 450, 650); window.Show();
            using var serialized = new SerializedObject(asset);
            try
            {
                var root = DeucarianEditorInspector.CreateToolkit("Settings"); window.rootVisualElement.Add(root);
                DeucarianEditorInspector.Properties(root, serialized);
                // Property drawers and their parent inspector can both request shared styling.
                DeucarianEditorNativeControlStyling.Bind(root, serialized);
                for (int frame = 0; frame < 12; frame++) yield return null;
                var slider = root.Q<Slider>(); var toggle = root.Q<Toggle>();
                Assert.That(slider, Is.Not.Null); Assert.That(toggle, Is.Not.Null);
                Assert.That(slider.ClassListContains("dw-slider"), Is.True);
                Assert.That(slider.Query(className: "dw-slider-fill").ToList(), Has.Count.EqualTo(1));
                Assert.That(toggle.ClassListContains("dw-switch"), Is.True);
                Assert.That(slider.lowValue, Is.EqualTo(0)); Assert.That(slider.highValue, Is.EqualTo(1));
                Undo.IncrementCurrentGroup(); Undo.RecordObject(asset, "Inspector source update");
                asset.active = true; asset.amount = .75f; Undo.FlushUndoRecordObjects(); EditorUtility.SetDirty(asset);
                yield return WaitFor(() => toggle.value && toggle.ClassListContains("dw-switch-on") && Mathf.Approximately(slider.value, .75f));
                Assert.That(toggle.value, Is.True); Assert.That(slider.value, Is.EqualTo(.75f).Within(.001f));
                Assert.That(toggle.ClassListContains("dw-switch-on"), Is.True);
                var tracker = slider.Q(className: "unity-base-slider__tracker");
                yield return WaitFor(() => Mathf.Abs(slider.Q("slider-fill").resolvedStyle.width / tracker.resolvedStyle.width - .75f) < .04f);
                Assert.That(slider.Q("slider-fill").resolvedStyle.width / tracker.resolvedStyle.width, Is.EqualTo(.75).Within(.04));
                Undo.PerformUndo();
                yield return WaitFor(() => !toggle.value && !toggle.ClassListContains("dw-switch-on") && Mathf.Approximately(slider.value, .25f));
                Assert.That(toggle.value, Is.False); Assert.That(toggle.ClassListContains("dw-switch-on"), Is.False);
                Assert.That(slider.value, Is.EqualTo(.25f).Within(.001f));
                slider.value = .6f;
                for (int frame = 0; frame < 6; frame++) yield return null;
                Assert.That(asset.amount, Is.EqualTo(.6f).Within(.001f));
                Assert.That(asset.caption, Is.EqualTo("Original"));
                Assert.That(slider.Query(className: "dw-slider-fill").ToList(), Has.Count.EqualTo(1));
            }
            finally { window.Close(); Undo.ClearUndo(asset); Object.DestroyImmediate(asset); }
        }

        private static IEnumerator WaitFor(System.Func<bool> settled)
        {
            double deadline = EditorApplication.timeSinceStartup + 5;
            while (!settled() && EditorApplication.timeSinceStartup < deadline) yield return null;
            Assert.That(settled(), Is.True, "Serialized fields and their scheduled styling must settle within five seconds.");
        }

        [UnityTest] public IEnumerator MultiSelectionUsesTheOriginalSerializedTargets()
        {
            var first = ScriptableObject.CreateInstance<Settings>(); var second = ScriptableObject.CreateInstance<Settings>();
            second.active = true;
            var window = ScriptableObject.CreateInstance<Host>(); window.Show();
            using var serialized = new SerializedObject(new Object[] { first, second });
            try
            {
                var root = DeucarianEditorInspector.CreateToolkit(); window.rootVisualElement.Add(root);
                DeucarianEditorInspector.Properties(root, serialized, "caption");
                for (int frame = 0; frame < 12; frame++) yield return null;
                Assert.That(root.Q<Toggle>().showMixedValue, Is.True);
                root.Q<Slider>().value = .9f;
                for (int frame = 0; frame < 6; frame++) yield return null;
                Assert.That(first.amount, Is.EqualTo(.9f).Within(.001)); Assert.That(second.amount, Is.EqualTo(.9f).Within(.001));
                Assert.That(root.Q("caption"), Is.Null);
                Assert.That(first.caption, Is.EqualTo("Original")); Assert.That(second.caption, Is.EqualTo("Original"));
            }
            finally { window.Close(); Undo.ClearUndo(first); Undo.ClearUndo(second); Object.DestroyImmediate(first); Object.DestroyImmediate(second); }
        }
    }
}
