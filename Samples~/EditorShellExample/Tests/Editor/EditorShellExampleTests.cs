using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Samples.Tests
{
    public sealed class EditorShellExampleTests
    {
        [Test]
        public void PageUsesSharedControlsAndKeepsDraftAcrossActivation()
        {
            using (var page = EditorShellExampleView.CreatePage())
            {
                var root = page.Root;
                var input = root.Q<TextField>("example-label");
                input.value = "Kept draft";
                page.Deactivate(); page.Activate(null);
                Assert.AreSame(input, root.Q<TextField>("example-label"));
                Assert.AreEqual("Kept draft", input.value);
                Assert.NotNull(root.Q("workspace-navigation"));
                Assert.NotNull(root.Q("workspace-scale-slider"));
                Assert.NotNull(root.Q("editor-shell-example-panel"));
                Assert.NotNull(root.Q(className: "dw-primary"));
                Assert.AreEqual("Hello", root.Q<Label>("editor-shell-example-status").text);
            }
        }
    }
}
