using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorFeatureSectionTests
    {
        [Test]
        public void RenderingStateDoesNotIssueCommandsAndUnknownConnectionsHaveNoCheckmark()
        {
            int commands = 0;
            var section = new DeucarianEditorFeatureSection("test", "Audio", "Description", "headphones", _ => commands++);
            section.Details.Add(new Label("Details"));
            section.Actions.Add(new Button());
            section.SetState(false, "Off");
            Assert.That(section.Details.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(section.Actions.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            section.SetState(true);
            Assert.That(section.Details.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(commands, Is.Zero);
            var unknown = DeucarianEditorFeatureSection.Connection("unknown", "Keyboard", false, "Not observed");
            Assert.That(unknown.ClassListContains("dw-connected"), Is.False);
            Assert.That(unknown.Q(className: "dw-feature-connection-icon").style.backgroundImage.keyword,
                Is.EqualTo(StyleKeyword.None));
        }

        [UnityTest]
        public IEnumerator FeatureSectionsFitAtEveryScaleAndKeepTheScaleDockStationary()
        {
            int previousScale = DeucarianEditorAppearance.WorkspaceScalePercent;
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Test"))
            {
                try
                {
                    workspace.Title.text = "Theming";
                    workspace.Subtitle.text = "Choose what this project uses.";
                    DeucarianEditorWorkspaceControls.Show(workspace.Scope, false);
                    DeucarianEditorWorkspaceControls.Show(workspace.Tabs, false);
                    var scroll = DeucarianEditorWorkspaceControls.Scroll("features");
                    scroll.AddToClassList("dw-feature-scroll");
                    workspace.Content.Add(scroll);
                    var content = DeucarianEditorWorkspaceControls.Region(null, "dw-feature-page");
                    scroll.Add(content);
                    var section = new DeucarianEditorFeatureSection("audio", "Audio", "Sounds for interactions and feedback.", "headphones", _ => { });
                    var form = new DeucarianEditorWorkspaceForm(section.Details);
                    form.Asset("palette", "Palette set", typeof(ScriptableObject), () => null, _ => { });
                    form.Choice("experience", "Experience", new[] { "Default", "XR" }, () => 1, _ => { });
                    var button = DeucarianEditorWorkspaceControls.Button("Open Audio Palette Lab →", () => { }, true);
                    section.Actions.Add(button);
                    section.SetState(true);
                    content.Add(section.Root);
                    foreach (var size in new[] { new Vector2(1590, 1000), new Vector2(1180, 800), new Vector2(820, 650), new Vector2(1920, 1080) })
                    {
                        window.rootVisualElement.style.width = size.x;
                        window.rootVisualElement.style.height = size.y;
                        Rect? scaleBounds = null;
                        foreach (int percent in new[] { 75, 100, 150 })
                        {
                            DeucarianEditorAppearance.WorkspaceScalePercent = percent;
                            for (int i = 0; i < 10; i++) yield return null;
                            string context = size + " at " + percent;
                            Inside(section.Switch, section.Root, context);
                            Inside(button, section.Root, context);
                            foreach (var field in section.Details.Query(className: "dw-field").ToList())
                                Inside(field.Q(className: "dw-field-input"), field, context);
                            Assert.That(section.Switch.resolvedStyle.height, Is.GreaterThanOrEqualTo(30));
                            var scale = window.rootVisualElement.Q<SliderInt>("workspace-scale-slider");
                            if (scaleBounds.HasValue) Assert.That(scale.worldBound, Is.EqualTo(scaleBounds.Value), context);
                            scaleBounds = scale.worldBound;
                        }
                    }
                }
                finally { window.Close(); DeucarianEditorAppearance.WorkspaceScalePercent = previousScale; }
            }
        }

        private static void Inside(VisualElement child, VisualElement parent, string context)
        {
            Assert.That(child.worldBound.xMin, Is.GreaterThanOrEqualTo(parent.worldBound.xMin - 1), context);
            Assert.That(child.worldBound.xMax, Is.LessThanOrEqualTo(parent.worldBound.xMax + 1), context);
        }
    }
}
