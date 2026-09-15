using NUnit.Framework;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorAssetMenuTests
    {
        [TestCase("Packages/com.deucarian.theming/Runtime/Defaults/Visual/Themes/Bundled/Light.asset", "Light")]
        [TestCase("Packages/com.deucarian.theming/Runtime/Audio/Cues/Hover.asset", "Hover")]
        [TestCase("Packages/com.deucarian.notifications/Samples~/Catalogs/Example.asset", "Example")]
        [TestCase("Assets/Deucarian/Theming/Support/Styles/FrostedGlassStyle.asset", "Frosted Glass")]
        [TestCase("Assets/Very/Deep/Folder/Tree/Content.asset", "Name/with\\slashes")]
        public void AllAssetCategoriesHaveAtMostThreeLevels(string path, string name)
        {
            string label = DeucarianEditorAssetMenu.Path(path, name);
            Assert.That(label.Split('/').Length, Is.LessThanOrEqualTo(3));
            Assert.That(label, Does.Contain(name.Replace('/', '∕').Replace('\\', '∕')));
        }

        [Test]
        public void SameNamesRemainDistinguishableByFolderAndSubAssetIdentity()
        {
            Assert.That(DeucarianEditorAssetMenu.Path("Assets/Models/Tree.prefab", "Tree"),
                Is.Not.EqualTo(DeucarianEditorAssetMenu.Path("Assets/Models/Tree.fbx", "Tree")));
            Assert.That(DeucarianEditorAssetMenu.Path("Assets/Themes/A.asset", "Dark"),
                Is.Not.EqualTo(DeucarianEditorAssetMenu.Path("Assets/Themes/B.asset", "Dark")));
            Assert.That(DeucarianEditorAssetMenu.Path("Assets/One/Theme.asset", "Theme"),
                Is.Not.EqualTo(DeucarianEditorAssetMenu.Path("Assets/Two/Theme.asset", "Theme")));
            Assert.That(DeucarianEditorAssetMenu.Path("Assets/Theme.asset", "Color", 1),
                Is.Not.EqualTo(DeucarianEditorAssetMenu.Path("Assets/Theme.asset", "Color", 2)));
            Assert.That(DeucarianEditorAssetMenu.Path("Packages/first/Theme.asset", "Theme"),
                Is.Not.EqualTo(DeucarianEditorAssetMenu.Path("Packages/second/Theme.asset", "Theme")));
        }
    }
}
