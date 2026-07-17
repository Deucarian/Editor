using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Samples.Tests
{
    public sealed class EditorShellExampleTests
    {
        [Test]
        public void CreateBuildsSharedShellAndPackagePanel()
        {
            VisualElement root = EditorShellExampleView.Create();
            Label status = root.Q<Label>("editor-shell-example-status");

            Assert.NotNull(root.Q<VisualElement>("deucarian-window-shell"));
            Assert.NotNull(root.Q<VisualElement>("editor-shell-example-panel"));
            Assert.NotNull(status);
            Assert.That(status.text, Is.EqualTo("Ready to add package-specific controls."));
        }
    }
}
