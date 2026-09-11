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
        [UnityTest]
        public IEnumerator SharedSearchKeepsItsPromptSeparateFromTheQuery()
        {
            var search = DeucarianEditorWorkspaceControls.Search("query", "Find a command…", out var input);
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>(); window.Show();
            try
            {
                window.rootVisualElement.Add(search); yield return null;
                var prompt = search.Q<Label>(className: "dw-search-placeholder");
                Assert.That(input.value, Is.Empty);
                Assert.That(prompt.text, Is.EqualTo("Find a command…"));
                Assert.That(prompt.pickingMode, Is.EqualTo(PickingMode.Ignore));
                input.value = "focus";
                Assert.That(prompt.style.display.value, Is.EqualTo(DisplayStyle.None));
                input.value = string.Empty;
                Assert.That(prompt.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            }
            finally { window.Close(); }
        }

        [UnityTest]
        public IEnumerator AssetPickerAndActionShareTheirRowWithLongNames()
        {
            var asset = ScriptableObject.CreateInstance<FormAsset>(); asset.name = "Long project settings asset name for a shared runtime profile";
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>(); window.Show();
            int scale = DeucarianEditorAppearance.WorkspaceScalePercent;
            var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Test");
            try
            {
                window.position = new Rect(50, 50, 1586, 940);
                DeucarianEditorAppearance.WorkspaceScalePercent = 100;
                var card = new DeucarianEditorFeatureSection("asset-layout", "Settings", "Shared project settings.", DeucarianEditorIconIds.Package);
                card.Root.AddToClassList("dw-feature-settings");
                var field = new UnityEditor.UIElements.ObjectField { value = asset, objectType = typeof(FormAsset) };
                var button = DeucarianEditorWorkspaceControls.Button("Select", () => { });
                var actions = DeucarianEditorWorkspaceControls.Actions(field, button); actions.AddToClassList("dw-asset-actions");
                card.Details.Add(DeucarianEditorWorkspaceControls.Field("Settings asset", actions)); card.SetState(true);
                var cards = new VisualElement(); cards.Add(card.Root);
                var split = DeucarianEditorWorkspaceControls.Split(cards, new VisualElement());
                split.AddToClassList("dw-spatial-split"); split.AddToClassList("dw-asset-preview-split");
                workspace.Content.Add(split);
                for (int frame = 0; frame < 20; frame++) yield return null;
                Assert.That(field.worldBound.center.y, Is.EqualTo(button.worldBound.center.y).Within(2));
                Assert.That(field.worldBound.xMax, Is.LessThan(button.worldBound.xMin));
                Assert.That(button.worldBound.xMax, Is.LessThanOrEqualTo(card.Root.worldBound.xMax));
            }
            finally { workspace.Dispose(); window.Close(); Object.DestroyImmediate(asset); DeucarianEditorAppearance.WorkspaceScalePercent = scale; }
        }

        [UnityTest]
        public IEnumerator InspectorPreviewActionsKeepIconsAndCheckboxLabelsWithinTheirControls()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>(); window.Show();
            try
            {
                var root = DeucarianEditorInspector.CreateToolkit("Preview"); window.rootVisualElement.Add(root);
                var preview = DeucarianEditorWorkspaceControls.Region(null, "dw-visibility-preview"); root.Add(preview);
                var actions = DeucarianEditorWorkspaceControls.Actions(
                    DeucarianEditorWorkspaceControls.IconButton("Enter", DeucarianEditorIconIds.Play, () => { }),
                    DeucarianEditorWorkspaceControls.IconButton("Exit", DeucarianEditorIconIds.Play, () => { }),
                    DeucarianEditorWorkspaceControls.IconButton("Stop", DeucarianEditorIconIds.Stop, () => { }));
                preview.Add(actions);
                var repeat = DeucarianEditorWorkspaceControls.Region(null, "dw-inline-checkbox");
                var check = new Toggle(); check.AddToClassList("dw-checkbox"); repeat.Add(check);
                var label = DeucarianEditorWorkspaceControls.Label("Loop"); repeat.Add(label); actions.Add(repeat);
                foreach (int width in new[] { 340, 460, 850 })
                {
                    window.position = new Rect(80, 80, width, 620);
                    for (int i = 0; i < 16; i++) yield return null;
                    Assert.That(check.Q(className: "unity-toggle__checkmark").worldBound.xMax, Is.LessThanOrEqualTo(label.worldBound.xMin));
                    Assert.That(label.worldBound.xMax, Is.LessThanOrEqualTo(root.worldBound.xMax));
                    foreach (var button in actions.Query<Button>().ToList())
                    {
                        var icon = button.Q(className: "dw-icon");
                        Assert.That(icon.worldBound.yMin, Is.GreaterThanOrEqualTo(button.worldBound.yMin));
                        Assert.That(icon.worldBound.yMax, Is.LessThanOrEqualTo(button.worldBound.yMax));
                    }
                }
            }
            finally { window.Close(); }
        }

        [UnityTest]
        public IEnumerator EmptyContextRegionsCollapseAndReturnWhenAConsumerAddsControls()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>(); window.Show();
            try
            {
                using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Review"))
                {
                    double deadline = EditorApplication.timeSinceStartup + 3;
                    while (workspace.Scope.resolvedStyle.display != DisplayStyle.None && EditorApplication.timeSinceStartup < deadline) yield return null;
                    Assert.That(workspace.Scope.resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
                    Assert.That(workspace.Tabs.resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
                    workspace.Scope.Add(new TextField()); workspace.Tabs.Add(new Button());
                    deadline = EditorApplication.timeSinceStartup + 3;
                    while (workspace.Scope.resolvedStyle.display == DisplayStyle.None && EditorApplication.timeSinceStartup < deadline) yield return null;
                    Assert.That(workspace.Scope.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
                    Assert.That(workspace.Tabs.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
                    workspace.Scope.Clear(); workspace.Tabs.Clear();
                    deadline = EditorApplication.timeSinceStartup + 3;
                    while (workspace.Scope.resolvedStyle.display != DisplayStyle.None && EditorApplication.timeSinceStartup < deadline) yield return null;
                    Assert.That(workspace.Scope.resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
                }
            }
            finally { window.Close(); }
        }

        [UnityTest]
        public IEnumerator CodeExamplesUseTheBundledMonospaceFont()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            try
            {
                using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Review"))
                {
                    var example = DeucarianEditorWorkspaceControls.Label("iii WWW 0123", "dw-code-example");
                    workspace.Content.Add(example);
                    for (int i = 0; i < 6; i++) yield return null;
                    var font = AssetDatabase.LoadAssetAtPath<Font>(DeucarianEditorUIResources.FontsPath + "/JetBrainsMono-Regular.ttf");
                    Assert.NotNull(font);
                    Assert.AreSame(font, example.resolvedStyle.unityFont);
                    Assert.AreSame(font, example.resolvedStyle.unityFontDefinition.font,
                        "Unity's inherited font definition must not override the code font.");
                    var narrow = example.MeasureTextSize("iiii", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    var wide = example.MeasureTextSize("WWWW", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    Assert.That(narrow.x, Is.EqualTo(wide.x).Within(0.1f), "Rendered glyph advances must be monospace.");
                }
            }
            finally { window.Close(); }
        }

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

        [UnityTest]
        public IEnumerator CheckedCheckboxUsesTheAccentFillAndRemainsLegible()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var root = DeucarianEditorInspector.CreateToolkit();
            var checkbox = new Toggle(); root.Add(DeucarianEditorWorkspaceControls.Field("Override", checkbox));
            window.Show(); window.rootVisualElement.Add(root);
            try
            {
                checkbox.value = true;
                for (int frame = 0; frame < 16; frame++) yield return null;
                var mark = checkbox.Q(className: "unity-toggle__checkmark");
                var fill = mark.resolvedStyle.backgroundColor;
                Assert.That(fill.a, Is.GreaterThan(.9f));
                Assert.That(fill.g, Is.GreaterThan(fill.r + .1f));
                Assert.That(fill.b, Is.GreaterThan(fill.r + .1f));
                Assert.That(mark.resolvedStyle.backgroundImage.texture, Is.Not.Null);
            }
            finally { window.Close(); }
        }

        private sealed class FormAsset : ScriptableObject
        {
            [SerializeField] private bool feature;
            [SerializeField] private float gain = .4f;
            [SerializeField] private string caption = "Original";
        }
    }
}
