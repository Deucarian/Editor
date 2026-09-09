using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorAppearance
    {
        private const string DecorativeKey = "Editor.DecorativeBackgrounds";
        internal const string ScaleKey = "Editor.WorkspaceScalePercent";
        private static readonly ConditionalWeakTable<VisualElement, Binding> Bindings = new ConditionalWeakTable<VisualElement, Binding>();
        public static event Action Changed;

        public static int WorkspaceScalePercent
        {
            get => UnityEngine.Mathf.Clamp(DeucarianEditorProjectPreferences.GetInt(ScaleKey, 100), 75, 150);
            set
            {
                int percent = UnityEngine.Mathf.Clamp(value, 75, 150);
                if (WorkspaceScalePercent == percent) return;
                DeucarianEditorProjectPreferences.SetInt(ScaleKey, percent);
                Changed?.Invoke();
            }
        }

        public static bool DecorativeBackgrounds
        {
            get => DeucarianEditorProjectPreferences.GetBool(DecorativeKey, false);
            set
            {
                if (DecorativeBackgrounds == value) return;
                DeucarianEditorProjectPreferences.SetBool(DecorativeKey, value);
                Changed?.Invoke();
                foreach (EditorWindow window in UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>()) window.Repaint();
            }
        }

        internal static void Bind(VisualElement root)
        {
            Bindings.GetValue(root, element => new Binding(element)).Apply();
        }

        private sealed class Binding
        {
            private readonly VisualElement root;
            internal Binding(VisualElement element)
            {
                root = element;
                root.RegisterCallback<AttachToPanelEvent>(_ => Attach());
                root.RegisterCallback<DetachFromPanelEvent>(_ => Changed -= Apply);
                if (root.panel != null) Attach();
            }
            private void Attach() { Changed -= Apply; Changed += Apply; Apply(); }
            internal void Apply() => root.EnableInClassList("deucarian-quiet", !DecorativeBackgrounds);
        }
    }
}
