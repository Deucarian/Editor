using Deucarian.Editor.Definitions;
using NUnit.Framework;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianDefinitionWorkQueueTests
    {
        [Test]
        public void RepeatedEditsWaitForAQuietPeriodAndCoalesceEachAsset()
        {
            var queue = new DeucarianDefinitionWorkQueue();
            Assert.That(queue.Enqueue("first.asset", 1), Is.True);
            Assert.That(queue.Enqueue("first.asset", 1.2), Is.False);
            Assert.That(queue.Enqueue("second.asset", 1.5), Is.False);
            Assert.That(queue.TakeReady(2, false), Is.Null);
            Assert.That(queue.TakeReady(3, false), Is.EquivalentTo(new[] { "first.asset", "second.asset" }));
            Assert.That(queue.Enqueue("third.asset", 4), Is.True, "A drained queue must schedule the next edit.");
        }

        [Test]
        public void BusyEditorRetainsPendingWorkUntilReady()
        {
            var queue = new DeucarianDefinitionWorkQueue();
            queue.Enqueue("example.asset", 1);
            Assert.That(queue.TakeReady(3, true), Is.Null);
            Assert.That(queue.TakeReady(4, false), Is.EqualTo(new[] { "example.asset" }));
            Assert.That(queue.TakeReady(5, false), Is.Empty);
        }

        [Test]
        public void TwoEditorsRetainIndependentLeasesAndRequeueOnFinalRelease()
        {
            var queue = new DeucarianDefinitionWorkQueue();
            var first = queue.BeginEditing("example.asset", path => queue.Enqueue(path, 1));
            var second = queue.BeginEditing("example.asset", path => queue.Enqueue(path, 2));
            first.Dispose(); first.Dispose();
            Assert.That(queue.IsEditing("example.asset"), Is.True);
            queue.TakeReady(3, false);
            Assert.That(queue.IsEditing("example.asset"), Is.True, "Draining changes must not lose the second editor's lease.");
            second.Dispose(); second.Dispose();
            Assert.That(queue.IsEditing("example.asset"), Is.False);
            Assert.That(queue.TakeReady(4, false), Is.EqualTo(new[] { "example.asset" }));
        }

        [Test]
        public void RestoredEditorAllowsEarlierEditsToSynchronizeButDefersNewDraftChanges()
        {
            var queue = new DeucarianDefinitionWorkQueue();
            string assetHash = "latest-asset-after-reload";
            using (queue.BeginEditing("example.asset", path => queue.Enqueue(path, 1), () => assetHash))
            {
                queue.Enqueue("example.asset", 1);
                Assert.That(queue.ShouldDefer("example.asset"), Is.False,
                    "Restoring selection must not suppress unsynchronized changes from the previous domain.");
                assetHash = "new-edit-in-restored-panel";
                Assert.That(queue.ShouldDefer("example.asset"), Is.True);
                queue.Acknowledge("example.asset");
                Assert.That(queue.ShouldDefer("example.asset"), Is.False,
                    "Saving the current draft must allow subsequent code-first imports to converge.");
            }
        }

        [Test]
        public void DisposingAndRecreatingPanelKeepsPendingWorkAndAllowsItsLatestValuesThrough()
        {
            var queue = new DeucarianDefinitionWorkQueue();
            string assetHash = "original";
            var first = queue.BeginEditing("example.asset", path => queue.Enqueue(path, 1), () => assetHash);
            assetHash = "unsynchronized-draft";
            Assert.That(queue.ShouldDefer("example.asset"), Is.True);
            first.Dispose();
            using (queue.BeginEditing("example.asset", path => queue.Enqueue(path, 2), () => assetHash))
            {
                Assert.That(queue.TakeReady(3, false), Is.EqualTo(new[] { "example.asset" }));
                Assert.That(queue.ShouldDefer("example.asset"), Is.False);
            }
        }
    }
}
