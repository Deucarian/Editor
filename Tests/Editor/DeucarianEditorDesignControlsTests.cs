using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorDesignControlsTests
    {
        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator ReadOnlyFieldsShareTheCaptionBaseline()
        {
            var window = UnityEngine.ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var root = DeucarianEditorInspector.CreateToolkit();
            var value = DeucarianEditorWorkspaceControls.Label("Local snapshot", "dw-readonly");
            var row = DeucarianEditorWorkspaceControls.Field("Current version", value);
            root.Add(row);
            try
            {
                window.Show(); window.rootVisualElement.Add(root);
                window.position = new UnityEngine.Rect(50, 50, 740, 420);
                for (int frame = 0; frame < 8; frame++) yield return null;
                var caption = row.Q<Label>(className: "dw-field-label");
                Assert.That(value.worldBound.y + value.resolvedStyle.paddingTop,
                    Is.EqualTo(caption.worldBound.y + caption.resolvedStyle.paddingTop).Within(1));
            }
            finally { window.Close(); }
        }

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

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator SwitchAndSliderStaySizedInsideStackedAndWideForms()
        {
            var window = UnityEngine.ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var root = DeucarianEditorInspector.CreateToolkit();
            var toggle = new DeucarianEditorSwitch();
            var slider = new DeucarianEditorSlider(0, 1) { showInputField = true };
            root.Add(DeucarianEditorWorkspaceControls.Field("Loop", toggle));
            root.Add(DeucarianEditorWorkspaceControls.Field("Visibility", slider));
            try
            {
                window.Show();
                window.rootVisualElement.Add(root);
                foreach (float width in new[] { 340f, 740f })
                {
                    window.position = new UnityEngine.Rect(80, 80, width, 420);
                    for (int frame = 0; frame < 8; frame++) yield return null;
                    var track = toggle.Q(className: "unity-toggle__input");
                    Assert.That(track.resolvedStyle.width, Is.EqualTo(54).Within(1), "A switch is not a full-column track.");
                    var valueField = slider.Q(className: "unity-base-slider__text-field");
                    var input = valueField.Q(className: "unity-base-field__input");
                    Assert.That(valueField.worldBound.yMin, Is.GreaterThanOrEqualTo(slider.worldBound.yMin - 1));
                    Assert.That(valueField.worldBound.yMax, Is.LessThanOrEqualTo(slider.worldBound.yMax + 1));
                    Assert.That(input.worldBound.yMax, Is.LessThanOrEqualTo(valueField.worldBound.yMax + 1));
                }
            }
            finally { window.Close(); }
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

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator PrimaryButtonKeepsReadableContrastInEveryInteractionState()
        {
            var window = UnityEngine.ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var root = DeucarianEditorInspector.CreateToolkit();
            var button = DeucarianEditorWorkspaceControls.Button("Validate settings", () => { }, true);
            root.Add(button);
            var states = typeof(VisualElement).GetProperty("pseudoStates",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(states, Is.Not.Null);
            try
            {
                window.Show();
                window.rootVisualElement.Add(root);
                foreach (bool light in new[] { false, true })
                {
                    root.EnableInClassList("deucarian-theme--light", light);
                    root.EnableInClassList("deucarian-theme--dark", !light);
                    foreach (string state in new[] { "0", "Focus", "Hover", "Active", "Focus, Active" })
                    {
                        states.SetValue(button, System.Enum.Parse(states.PropertyType, state));
                        for (int frame = 0; frame < 8; frame++) yield return null;
                        double text = Luminance(button.resolvedStyle.color);
                        double background = Luminance(button.resolvedStyle.backgroundColor);
                        double contrast = (System.Math.Max(text, background) + .05) /
                            (System.Math.Min(text, background) + .05);
                        Assert.That(contrast, Is.GreaterThanOrEqualTo(4.5), (light ? "Light " : "Dark ") + state);
                    }
                }
            }
            finally { window.Close(); }
        }

        private static double Luminance(UnityEngine.Color color)
        {
            double Channel(double value) => value <= .04045 ? value / 12.92 : System.Math.Pow((value + .055) / 1.055, 2.4);
            return .2126 * Channel(color.r) + .7152 * Channel(color.g) + .0722 * Channel(color.b);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator DisabledPrimaryIconButtonsKeepTheirTextAndIconReadable()
        {
            var window = UnityEngine.ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var root = DeucarianEditorInspector.CreateToolkit();
            var button = DeucarianEditorWorkspaceControls.IconButton("Send command", DeucarianEditorIconIds.Play,
                () => { }, DeucarianEditorButtonRole.Primary);
            root.Add(button);
            button.SetEnabled(false);
            try
            {
                window.Show(); window.rootVisualElement.Add(root);
                foreach (bool light in new[] { false, true })
                {
                    root.EnableInClassList("deucarian-theme--light", light);
                    root.EnableInClassList("deucarian-theme--dark", !light);
                    for (int frame = 0; frame < 8; frame++) yield return null;
                    var label = button.Q<Label>(className: "dw-label");
                    var icon = button.Q(className: "dw-icon");
                    Assert.That(label.resolvedStyle.color, Is.EqualTo(button.resolvedStyle.color));
                    Assert.That(icon.resolvedStyle.unityBackgroundImageTintColor, Is.EqualTo(button.resolvedStyle.color));
                    double text = Luminance(label.resolvedStyle.color);
                    double background = Luminance(button.resolvedStyle.backgroundColor);
                    Assert.That((System.Math.Max(text, background) + .05) / (System.Math.Min(text, background) + .05),
                        Is.GreaterThanOrEqualTo(4.5), light ? "Light disabled palette" : "Dark disabled palette");
                }
            }
            finally { window.Close(); }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator ToolkitInspectorReusesControlsWithoutWindowNavigationOrScale()
        {
            var root = DeucarianEditorInspector.CreateToolkit("Profile");
            var form = new DeucarianEditorWorkspaceForm(root);
            var value = UnityEngine.Vector3.one;
            var field = form.Vector("offset", "Offset", () => value, next => value = next);
            var window = UnityEngine.ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            try
            {
                window.Show();
                window.rootVisualElement.Add(root);
                yield return null;
                field.value = UnityEngine.Vector3.up;
                Assert.That(value, Is.EqualTo(UnityEngine.Vector3.up));
                Assert.That(root.ClassListContains("dw-inspector"), Is.True);
                Assert.That(root.Q("workspace-navigation"), Is.Null);
                Assert.That(root.Q("workspace-scale-slider"), Is.Null);
            }
            finally { window.Close(); }
        }
    }
}
