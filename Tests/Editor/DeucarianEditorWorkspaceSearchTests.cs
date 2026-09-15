using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorWorkspaceSearchTests
    {
        [Test]
        public void RestoredAndEditedSearchValuesNeverOverlapThePlaceholder()
        {
            var root = DeucarianEditorWorkspaceControls.Search("search", "Find a tool", out var input);
            var placeholder = root.Q<Label>(className: "dw-search-placeholder");
            input.SetValueWithoutNotify("navigation");
            Assert.AreEqual(DisplayStyle.None, placeholder.style.display.value);
            input.SetValueWithoutNotify("");
            Assert.AreEqual(DisplayStyle.Flex, placeholder.style.display.value);
            input.value = "audio";
            Assert.AreEqual(DisplayStyle.None, placeholder.style.display.value);
        }
    }
}
