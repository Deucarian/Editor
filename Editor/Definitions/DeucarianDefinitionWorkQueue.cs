using System;
using System.Collections.Generic;
using System.Linq;

namespace Deucarian.Editor.Definitions
{
    /// <summary>One import session's quiet-period scheduling and counted editor ownership.</summary>
    internal sealed class DeucarianDefinitionWorkQueue
    {
        private readonly Dictionary<string, WorkItem> pending = new Dictionary<string, WorkItem>(StringComparer.Ordinal);
        private double nextDrain;

        internal bool Enqueue(string path, double now)
        {
            Work(path).Queued = true;
            bool needsSchedule = nextDrain == 0;
            nextDrain = now + 0.75;
            return needsSchedule;
        }

        internal string[] TakeReady(double now, bool busy)
        {
            if (busy || now < nextDrain) return null;
            nextDrain = 0;
            var paths = pending.Where(x => x.Value.Queued).Select(x => x.Key).ToArray();
            foreach (string path in paths)
            {
                var work = pending[path]; work.Queued = false;
                if (work.Editors.Count == 0) pending.Remove(path);
            }
            return paths;
        }

        internal bool IsEditing(string path) => pending.TryGetValue(path, out var work) && work.Editors.Count > 0;
        internal bool ShouldDefer(string path) => pending.TryGetValue(path, out var work) && work.Editors.Any(x => x.HasChanges);
        internal IDisposable BeginEditing(string path, Action<string> requeue, Func<string> capture = null) => new EditingScope(this, path, requeue, capture);
        internal void Acknowledge(string path)
        {
            if (pending.TryGetValue(path, out var work)) foreach (var editor in work.Editors) editor.Acknowledge();
        }

        private WorkItem Work(string path)
        {
            if (!pending.TryGetValue(path, out var work)) pending.Add(path, work = new WorkItem());
            return work;
        }

        private sealed class WorkItem
        {
            internal bool Queued;
            internal readonly List<EditingScope> Editors = new List<EditingScope>();
        }

        private sealed class EditingScope : IDisposable
        {
            private readonly DeucarianDefinitionWorkQueue owner;
            private readonly Action<string> requeue;
            private readonly Func<string> capture;
            private string path;
            private string snapshot;

            internal EditingScope(DeucarianDefinitionWorkQueue owner, string path, Action<string> requeue, Func<string> capture)
            {
                this.owner = owner; this.path = path; this.requeue = requeue;
                this.capture = capture;
                Acknowledge();
                owner.Work(path).Editors.Add(this);
            }

            internal bool HasChanges => snapshot == null || Snapshot() != snapshot;
            internal void Acknowledge() => snapshot = Snapshot();
            private string Snapshot()
            {
                try { return capture == null ? string.Empty : capture(); }
                catch (Exception) { return null; } // Invalid draft values stay in the form until explicitly resolved.
            }

            public void Dispose()
            {
                if (path == null) return;
                string released = path; path = null;
                var work = owner.Work(released);
                work.Editors.Remove(this);
                if (work.Editors.Count == 0 && !work.Queued) owner.pending.Remove(released);
                requeue(released);
            }
        }
    }
}
