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

    public sealed class DeucarianEditorPage : IDeucarianEditorPage
    {
        private readonly Action<string> activate;
        private readonly Action deactivate;
        private readonly Action<Rect> update;
        private Action dispose;

        public DeucarianEditorPage(VisualElement root, Action<string> activate = null,
            Action deactivate = null, Action<Rect> update = null, Action dispose = null)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            this.activate = activate;
            this.deactivate = deactivate;
            this.update = update;
            this.dispose = dispose;
        }

        public VisualElement Root { get; }
        public void Activate(string route) => activate?.Invoke(route);
        public void Deactivate() => deactivate?.Invoke();
        public void Update(Rect windowBounds) => update?.Invoke(windowBounds);
        public void Dispose()
        {
            var release = dispose;
            dispose = null;
            Root.RemoveFromHierarchy();
            release?.Invoke();
        }
    }
}
