using NUnit.Framework;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorDialogContractTests
    {
        [Test]
        public void Action_NormalizesFallbackValuesAndInvalidStyle()
        {
            var action = new DeucarianEditorDialogAction(
                "  continue  ",
                " ",
                null,
                (DeucarianEditorDialogActionStyle)999);

            Assert.AreEqual("continue", action.Id);
            Assert.AreEqual("continue", action.Label);
            Assert.AreEqual(DeucarianEditorIconIds.Package, action.IconId);
            Assert.AreEqual(DeucarianEditorDialogActionStyle.Secondary, action.Style);
        }

        [Test]
        public void Options_NormalizeMissingConstructorValues()
        {
            var options = new DeucarianEditorDialogOptions(
                null,
                null,
                null,
                null);

            Assert.AreEqual("Deucarian", options.Title);
            Assert.AreEqual(string.Empty, options.Message);
            Assert.AreEqual(DeucarianEditorIconIds.Info, options.IconId);
            CollectionAssert.IsEmpty(options.Actions);
        }

        [Test]
        public void Result_NormalizesMissingActionAndPreservesCompletionState()
        {
            var result = new DeucarianEditorDialogResult(
                null,
                DeucarianEditorDialogCompletionReason.WindowClosed,
                true);

            Assert.AreEqual(string.Empty, result.ActionId);
            Assert.AreEqual(DeucarianEditorDialogCompletionReason.WindowClosed, result.Reason);
            Assert.IsTrue(result.WasCanceled);
        }
    }
}
