using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorAppearance
    {
        private const string DecorativeKey = "Editor.DecorativeBackgrounds";
        internal const string EarlierScaleKey = "Editor.WorkspaceScalePercent.CompactBaseline";
        internal const string LegacyScaleKey = "Editor.WorkspaceScalePercent.ComfortBaseline";
        internal const string ScaleKey = "Editor.WorkspaceScalePercent.RefinedBaseline";
        private static readonly ConditionalWeakTable<VisualElement, Binding> Bindings = new ConditionalWeakTable<VisualElement, Binding>();
        public static event Action Changed;

        public static int WorkspaceScalePercent
        {
            get => ReadWorkspaceScale();
            set
            {
                int percent = UnityEngine.Mathf.Clamp(value, 75, 150);
                if (WorkspaceScalePercent == percent) return;
                DeucarianEditorProjectPreferences.SetInt(ScaleKey, percent);
                Changed?.Invoke();
            }
        }

        private static int ReadWorkspaceScale()
        {
            int saved = DeucarianEditorProjectPreferences.GetInt(ScaleKey, int.MinValue);
            if (saved != int.MinValue) return UnityEngine.Mathf.Clamp(saved, 75, 150);
            int legacy = DeucarianEditorProjectPreferences.GetInt(LegacyScaleKey, int.MinValue);
            if (legacy == int.MinValue)
            {
                int earlier = DeucarianEditorProjectPreferences.GetInt(EarlierScaleKey, int.MinValue);
                if (earlier != int.MinValue)
                    legacy = earlier == 100 ? 100 : UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(earlier / 0.9f), 75, 150);
            }
            if (legacy == int.MinValue) return 100;
            int rebased = legacy == 100 ? 100 : UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(legacy / 0.75f), 75, 150);
            DeucarianEditorProjectPreferences.SetInt(ScaleKey, rebased);
            return rebased;
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
