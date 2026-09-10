using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorDesignControlsTests
    {
        [Test]
        public void SliderFillTracksSilentChangesAndClampedValues()
        {
            var slider = new DeucarianEditorSlider(0, 10);
            int changes = 0;
            slider.RegisterValueChangedCallback(_ => changes++);
            slider.SetValueWithoutNotify(4);
            Assert.That(slider.Q("slider-fill").style.width.value.value, Is.EqualTo(40).Within(.01));
            slider.SetValueWithoutNotify(20);
            Assert.That(slider.value, Is.EqualTo(10));
            Assert.That(slider.Q("slider-fill").style.width.value.value, Is.EqualTo(100));
            Assert.That(changes, Is.Zero);
            var integer = new DeucarianEditorIntegerSlider(75, 150);
            integer.SetValueWithoutNotify(100);
            Assert.That(integer.Q("slider-fill").style.width.value.value, Is.EqualTo(100f / 3).Within(.01));
        }

        [Test]
        public void SwitchSilentRefreshUpdatesAppearanceWithoutWriting()
        {
            var toggle = new DeucarianEditorSwitch();
            int writes = 0;
            toggle.RegisterValueChangedCallback(_ => writes++);
            toggle.SetValueWithoutNotify(true);
            Assert.That(toggle.ClassListContains("dw-switch-on"), Is.True);
            toggle.SetValueWithoutNotify(false);
            Assert.That(toggle.ClassListContains("dw-switch-on"), Is.False);
            Assert.That(writes, Is.Zero);
        }

        [Test]
        public void DisabledCapabilityKeepsExplanationAndSettingsActionOutsideDisabledContent()
        {
            bool enabled = false;
            int stops = 0;
            var content = new VisualElement();
            var text = new TextField { value = "Keep my choice" };
            content.Add(text);
            var gate = new DeucarianEditorCapabilityGate(content, () => enabled, "Audio is off",
                "Enable audio in Project setup.", "Go to Project setup", () => { }, () => stops++);
            Assert.That(content.enabledSelf, Is.False);
            Assert.That(gate.Root.Q<Button>("capability-open-settings").enabledInHierarchy, Is.True);
            Assert.That(stops, Is.EqualTo(1));
            gate.Refresh();
            Assert.That(stops, Is.EqualTo(1), "Ordinary repaint/refresh must not repeatedly stop or clear previews.");
            enabled = true;
            gate.Refresh();
            Assert.That(content.enabledSelf, Is.True);
            Assert.That(text.value, Is.EqualTo("Keep my choice"));
            Assert.That(gate.Root.Q("capability-disabled").style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        [TestCase(DeucarianEditorButtonRole.Primary, "dw-primary")]
        [TestCase(DeucarianEditorButtonRole.Secondary, "dw-button")]
        [TestCase(DeucarianEditorButtonRole.Quiet, "dw-quiet")]
        [TestCase(DeucarianEditorButtonRole.Destructive, "dw-destructive")]
        public void ButtonRolesShareOneControlFactory(DeucarianEditorButtonRole role, string style)
        {
            var button = DeucarianEditorWorkspaceControls.Button("Action", () => { }, role);
            Assert.That(button.ClassListContains("dw-button"), Is.True);
            Assert.That(button.ClassListContains(style), Is.True);
        }

        [Test]
        public void ToolkitInspectorReusesControlsWithoutWindowNavigationOrScale()
        {
            var root = DeucarianEditorInspector.CreateToolkit("Profile");
            var form = new DeucarianEditorWorkspaceForm(root);
            var value = UnityEngine.Vector3.one;
            var field = form.Vector("offset", "Offset", () => value, next => value = next);
            field.value = UnityEngine.Vector3.up;
            Assert.That(value, Is.EqualTo(UnityEngine.Vector3.up));
            Assert.That(root.ClassListContains("dw-inspector"), Is.True);
            Assert.That(root.Q("workspace-navigation"), Is.Null);
            Assert.That(root.Q("workspace-scale-slider"), Is.Null);
        }
    }
}
