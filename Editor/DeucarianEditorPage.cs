using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>A window-local page. Its owner keeps it alive while another page is selected.</summary>
    public interface IDeucarianEditorPage : IDisposable
    {
        VisualElement Root { get; }
        void Activate(string route);
        void Deactivate();
        void Update(Rect windowBounds);
    }

    /// <summary>Opt-in, owner-sanitized editor drafts. Never include credentials or runtime payloads.</summary>
    public interface IDeucarianEditorReloadState
    {
        string CaptureReloadState();
        void RestoreReloadState(string state);
    }

    public sealed class DeucarianEditorPage : IDeucarianEditorPage, IDeucarianEditorReloadState
    {
        private readonly Action<string> activate;
        private readonly Action deactivate;
        private readonly Action<Rect> update;
        private Action dispose;
        private readonly Func<string> captureReloadState;
        private readonly Action<string> restoreReloadState;

        public DeucarianEditorPage(VisualElement root, Action<string> activate = null,
            Action deactivate = null, Action<Rect> update = null, Action dispose = null,
            Func<string> captureReloadState = null, Action<string> restoreReloadState = null)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            this.activate = activate;
            this.deactivate = deactivate;
            this.update = update;
            this.dispose = dispose;
            this.captureReloadState = captureReloadState;
            this.restoreReloadState = restoreReloadState;
        }

        public VisualElement Root { get; }
        public void Activate(string route) => activate?.Invoke(route);
        public void Deactivate() => deactivate?.Invoke();
        public void Update(Rect windowBounds) => update?.Invoke(windowBounds);
        public string CaptureReloadState() => captureReloadState?.Invoke();
        public void RestoreReloadState(string state) => restoreReloadState?.Invoke(state);
        public void Dispose()
        {
            var release = dispose;
            dispose = null;
            Root.RemoveFromHierarchy();
            release?.Invoke();
        }
    }
}
