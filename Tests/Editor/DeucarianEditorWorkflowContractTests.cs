using NUnit.Framework;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorWorkflowContractTests
    {
        [Test]
        public void StatusChip_NormalizesOptionalTextAndPreservesStatus()
        {
            var chip = new DeucarianEditorStatusChip(
                null,
                DeucarianEditorStatus.Warning,
                null);

            Assert.AreEqual(string.Empty, chip.Label);
            Assert.AreEqual(DeucarianEditorStatus.Warning, chip.Status);
            Assert.AreEqual(string.Empty, chip.Tooltip);
        }

        [Test]
        public void TimelineEvent_NormalizesTextAndPreservesPresentationFlags()
        {
            var timelineEvent = new DeucarianEditorTimelineEvent(
                null,
                null,
                visualAssigned: true,
                audioAssigned: false);

            Assert.AreEqual(string.Empty, timelineEvent.Label);
            Assert.AreEqual(string.Empty, timelineEvent.Detail);
            Assert.IsTrue(timelineEvent.VisualAssigned);
            Assert.IsFalse(timelineEvent.AudioAssigned);
            Assert.IsTrue(timelineEvent.Enabled);
        }

        [Test]
        public void SplitPaneWidths_PreserveCalculatedValues()
        {
            var widths = new DeucarianEditorSplitPaneWidths(240f, 360f, 280f);

            Assert.AreEqual(240f, widths.Left);
            Assert.AreEqual(360f, widths.Center);
            Assert.AreEqual(280f, widths.Right);
        }
    }
}
