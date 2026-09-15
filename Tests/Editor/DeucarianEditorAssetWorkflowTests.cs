using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorAssetWorkflowTests
    {
        private sealed class Asset : ScriptableObject { }
        private sealed class Window : EditorWindow { }

        [TestCase("Packages/example/Settings.asset")]
        [TestCase("Assets/../Settings.asset")]
        [TestCase("Assets\\Settings.asset")]
        [TestCase("Assets/MissingParent/Settings.asset")]
        [TestCase("Assets/Settings.cs")]
        public void CreationRejectsInvalidOrNonProjectPaths(string path)
        {
            Assert.IsFalse(DeucarianEditorAssetCatalog.IsUnusedProjectPath(path));
        }

        [UnityTest]
        public IEnumerator PickerSurvivesRepeatedSelectionsAndDependentContentRebuilds()
        {
            var window = ScriptableObject.CreateInstance<Window>(); window.Show();
            var first = ScriptableObject.CreateInstance<Asset>();
            var second = ScriptableObject.CreateInstance<Asset>();
            var third = ScriptableObject.CreateInstance<Asset>();
            Object selected = null;
            int writes = 0, creates = 0;
            DeucarianEditorAssetField control = null;
            void Render()
            {
                window.rootVisualElement.Clear();
                window.rootVisualElement.Add(control.Root);
                window.rootVisualElement.Add(new Label(selected == null ? "None" : selected.name));
            }
            control = new DeucarianEditorAssetField("asset", typeof(Asset), () => selected,
                value => { writes++; selected = value; Render(); }, () => { creates++; return first; }, defaultValue: () => first);
            try
            {
                Render(); yield return null;
                Assert.That(writes, Is.Zero, "Rendering a default must not save or silently select it.");
                Assert.That(creates, Is.Zero);
                var input = control.Input;
                foreach (Object value in new Object[] { first, second, third, null })
                {
                    input.value = value; yield return null;
                    Assert.AreSame(value, selected);
                    Assert.AreSame(input, window.rootVisualElement.Q("asset"));
                    Assert.NotNull(input.panel);
                }
                Assert.That(writes, Is.EqualTo(4));
            }
            finally { window.Close(); Object.DestroyImmediate(first); Object.DestroyImmediate(second); Object.DestroyImmediate(third); }
        }

        [Test]
        public void DiscoveryIncludesInstalledPackageAssetsAndCachesUntilInvalidated()
        {
            var catalog = new DeucarianEditorAssetCatalog(typeof(StyleSheet));
            var first = catalog.Find();
            Assert.That(first, Has.Some.Matches<Object>(DeucarianEditorAssetCatalog.IsPackageAsset));
            Assert.AreSame(first, catalog.Find());
            catalog.Invalidate(); Assert.AreNotSame(first, catalog.Find());
        }
    }
}
