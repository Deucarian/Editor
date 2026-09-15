using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianProjectSettingsLayoutTests
    {
        [UnityTest]
        public IEnumerator CompactSettingsFitAndDoNotChangeTheControlCenterShell()
        {
            var host = ScriptableObject.CreateInstance<SettingsLayoutHost>();
            try
            {
                host.Show();
                foreach (int width in new[] { 480, 720, 1200 })
                {
                    host.position = new Rect(30, 30, width, 650);
                    using (var page = new DeucarianEditorProjectSettingsPage(host.rootVisualElement,
                        "Logging", "Configure runtime log output for this project."))
                    {
                        var field = new TextField("Prefix") { value = "Deucarian" };
                        page.Content.Add(field);
                        var action = DeucarianEditorWorkspaceControls.Button("Reset to defaults", () => { });
                        page.Content.Add(action);
                        var feature = new DeucarianEditorFeatureSection("output", "Runtime logging",
                            "Choose what appears in the Console.", DeucarianEditorIconIds.Document, _ => { });
                        page.Content.Add(feature.Root);
                        var example = DeucarianEditorWorkspaceControls.Label("[Example] Your message appears here.", "dw-code-example");
                        feature.Details.Add(example);
                        for (int i = 0; i < 5; i++) yield return null;
                        Assert.Greater(page.Root.worldBound.height, 100);
                        Assert.LessOrEqual(field.worldBound.xMax, page.Root.worldBound.xMax + 1);
                        Assert.GreaterOrEqual(field.worldBound.xMin, page.Root.worldBound.xMin);
                        Assert.That(action.resolvedStyle.height, Is.InRange(32, 38));
                        Assert.That(feature.Switch.resolvedStyle.width, Is.InRange(52, 56));
                        Assert.That(feature.Switch.resolvedStyle.height, Is.InRange(26, 30));
                        Assert.Greater(example.worldBound.height, 10);
                        Assert.LessOrEqual(feature.Root.worldBound.xMax, page.Root.worldBound.xMax + 1);
#if UNITY_6000_0_OR_NEWER
                        Assert.AreEqual(TextGeneratorType.Standard, example.resolvedStyle.unityTextGenerator);
#endif
                        Assert.IsNull(page.Root.Q("workspace-scale"), "No nested Control Center scaler.");
                    }
                    Assert.AreEqual(0, host.rootVisualElement.childCount);
                }
            }
            finally { host.Close(); }
        }
        private sealed class SettingsLayoutHost : EditorWindow { }
    }
}
