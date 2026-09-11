using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorNativeFormsTests
    {
        [Test]
        public void ChoiceIconsFollowTheSelectedSemanticStatusWithoutRetainingOldTints()
        {
            var root = new VisualElement();
            var form = new DeucarianEditorWorkspaceForm(root);
            int selection = 0;
            var choice = form.Choice("severity", "Type", new[] { "Warning", "Error", "Info", "Success", "Other" },
                () => selection, value => selection = value,
                new[] { DeucarianEditorIconIds.Warning, DeucarianEditorIconIds.Error, DeucarianEditorIconIds.Info,
                    DeucarianEditorIconIds.Success, DeucarianEditorIconIds.Settings });
            var icon = choice.Q(className: "dw-choice-icon");
            string[] states = { "warning", "error", "info", "success" };
            for (selection = 0; selection < 5; selection++)
            {
                form.Refresh();
                for (int state = 0; state < states.Length; state++)
                    Assert.That(icon.ClassListContains("dw-status-" + states[state]), Is.EqualTo(selection == state));
            }
        }

        [UnityTest]
        public IEnumerator SerializedFieldsRoundTripAndDisposeReleasesBindings()
        {
            var asset = ScriptableObject.CreateInstance<FormAsset>();
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var root = DeucarianEditorInspector.CreateToolkit();
            var form = new DeucarianEditorSerializedForm(root, asset);
            var toggle = (DeucarianEditorSwitch)form.Property("feature", "Enabled");
            var slider = form.Slider("gain", "Gain", 0, 1);
            var text = (TextField)form.Property("caption", "Caption");
            window.Show();
            window.rootVisualElement.Add(root);
            try
            {
                for (int i = 0; i < 8; i++) yield return null;
                Assert.That(slider.value, Is.EqualTo(.4f).Within(.01f));
                toggle.value = true;
                slider.value = .7f;
                text.value = "Updated";
                for (int i = 0; i < 3; i++) yield return null;
                using (var values = new SerializedObject(asset))
                {
                    Assert.That(values.FindProperty("feature").boolValue, Is.True);
                    Assert.That(values.FindProperty("gain").floatValue, Is.EqualTo(.7f).Within(.01f));
                    Assert.That(values.FindProperty("caption").stringValue, Is.EqualTo("Updated"));
                }
                form.Dispose();
                text.value = "Detached";
                for (int i = 0; i < 3; i++) yield return null;
                using (var values = new SerializedObject(asset))
                    Assert.That(values.FindProperty("caption").stringValue, Is.EqualTo("Updated"));
                Assert.Throws<System.ObjectDisposedException>(() => form.Property("caption"));
            }
            finally { form.Dispose(); window.Close(); Object.DestroyImmediate(asset); }
        }

        [UnityTest]
        public IEnumerator NativeHomePageReceivesUpdatesAndExactlyOneDisposal()
        {
            int updates = 0, deactivations = 0, disposals = 0;
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var home = new DeucarianEditorPage(new VisualElement(), deactivate: () => deactivations++,
                update: _ => updates++, dispose: () => disposals++);
            window.Show();
            var session = new DeucarianEditorPageSession(window, "test.native-home", home);
            try
            {
                for (int i = 0; i < 20; i++) yield return null;
                Assert.That(updates, Is.GreaterThan(0));
                session.Dispose();
                session.Dispose();
                Assert.That(deactivations, Is.EqualTo(1));
                Assert.That(disposals, Is.EqualTo(1));
            }
            finally { session.Dispose(); window.Close(); }
        }

        [Test]
        public void ToggleAndCheckboxHaveDistinctStyleContracts()
        {
            var toggle = new DeucarianEditorSwitch();
            var checkbox = new Toggle();
            DeucarianEditorWorkspaceControls.Field("Switch", toggle);
            DeucarianEditorWorkspaceControls.Field("Checkbox", checkbox);
            Assert.That(toggle.ClassListContains("dw-checkbox"), Is.False);
            Assert.That(checkbox.ClassListContains("dw-checkbox"), Is.True);
            toggle.SetValueWithoutNotify(true);
            Assert.That(toggle.Q<Label>(className: "dw-switch-label").text, Is.EqualTo("On"));
        }

        private sealed class FormAsset : ScriptableObject
        {
            [SerializeField] private bool feature;
            [SerializeField] private float gain = .4f;
            [SerializeField] private string caption = "Original";
        }
    }
}
