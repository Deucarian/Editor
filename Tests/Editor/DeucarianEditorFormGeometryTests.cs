using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorFormGeometryTests
    {
        private sealed class Host : EditorWindow { }

        [Test] public void SliderDragRangesDoNotTruncateTypedOrExistingValues()
        {
            var host = ScriptableObject.CreateInstance<Host>(); host.Show();
            try
            {
            float number = 420; int count = 900;
            var root = host.rootVisualElement; var form = new DeucarianEditorWorkspaceForm(root);
            var value = form.NumberWithSlider("range", "Range", 0, 20, () => number, next => number = next);
            var integer = form.IntegerWithSlider("count", "Count", 0, 100, () => count, next => count = next);
            Assert.That(number, Is.EqualTo(420)); Assert.That(value.value, Is.EqualTo(420));
            Assert.That(count, Is.EqualTo(900)); Assert.That(integer.value, Is.EqualTo(900));
            value.value = 750; integer.value = 1500;
            Assert.That(number, Is.EqualTo(750)); Assert.That(count, Is.EqualTo(1500));
            root.Q<Slider>("range-slider").value = 10;
            Assert.That(value.value, Is.EqualTo(10)); Assert.That(number, Is.EqualTo(10));
            }
            finally { host.Close(); }
        }

        [UnityTest] public IEnumerator StackedScopePopupsDoNotOverflowTheirFieldOrOverlapTheNextRow()
        {
            int previous = DeucarianEditorAppearance.WorkspaceScalePercent;
            var host = ScriptableObject.CreateInstance<Host>(); host.position = new Rect(70, 70, 1500, 900); host.Show();
            using (var view = new DeucarianEditorCollectionWorkspace(host.rootVisualElement, "Test", "Content", "", "test", "Search"))
            {
                try
                {
                    view.UsePanels(true);
                    var form = new DeucarianEditorWorkspaceForm(view.Workspace.Scope);
                    var first = form.Choice("pack", "Pack", new[] { "Project content" }, () => 0, _ => { });
                    var second = form.Choice("type", "Type", new[] { "All types" }, () => 0, _ => { });
                    foreach (int scale in new[] { 75, 100, 125, 150 })
                    {
                        DeucarianEditorAppearance.WorkspaceScalePercent = scale;
                        for (int i = 0; i < 10; i++) yield return null;
                        var input = first.Q(className: "unity-base-field__input");
                        Assert.That(input.worldBound.yMax, Is.LessThanOrEqualTo(first.parent.worldBound.yMax + 1), "scale " + scale);
                        bool separated = input.worldBound.yMax <= second.parent.worldBound.yMin + 1 || input.worldBound.xMax <= second.parent.worldBound.xMin + 1;
                        Assert.That(separated, Is.True, "Popup overlaps the next field at scale " + scale);
                        Assert.That(first.resolvedStyle.height, Is.EqualTo(60).Within(1));
                    }
                }
                finally { DeucarianEditorAppearance.WorkspaceScalePercent = previous; host.Close(); }
            }
        }
    }
}
