using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorWorkspaceScale : IDisposable
    {
        private readonly VisualElement viewport;
        private readonly VisualElement content;
        private readonly VisualElement footer;
        internal const float DefaultScale = 0.75f;
        private readonly SliderInt slider;
        private readonly Button reset;
        private bool disposed;

        internal DeucarianEditorWorkspaceScale(VisualElement content, VisualElement footer)
        {
            this.content = content;
            this.footer = footer;
            var shell = content.parent;
            viewport = DeucarianEditorWorkspaceControls.Region("workspace-scale-viewport", "dw-scale-viewport");
            shell.Add(viewport);
            viewport.Add(content);
            footer.AddToClassList("deucarian-workspace");
            footer.AddToClassList("dw-scale-dock");
            shell.Add(footer);
            content.style.position = Position.Absolute;
            content.style.left = 0;
            content.style.top = 0;
            content.style.transformOrigin = new TransformOrigin(0, 0);
            var controls = DeucarianEditorWorkspaceControls.Region("workspace-scale", "dw-scale");
            controls.Add(DeucarianEditorWorkspaceControls.Label("UI scale", "dw-muted"));
            slider = new DeucarianEditorIntegerSlider(75, 150) { name = "workspace-scale-slider", tooltip = "Size of this project's Deucarian workspaces (75–150%)." };
            slider.RegisterValueChangedCallback(evt => DeucarianEditorAppearance.WorkspaceScalePercent = evt.newValue);
            reset = DeucarianEditorWorkspaceControls.Button("100%", () => DeucarianEditorAppearance.WorkspaceScalePercent = 100);
            reset.name = "workspace-scale-reset";
            reset.tooltip = "Reset UI scale to 100%.";
            controls.Add(slider);
            controls.Add(reset);
            footer.Add(controls);
            viewport.RegisterCallback<GeometryChangedEvent>(OnResize);
            content.RegisterCallback<AttachToPanelEvent>(OnAttach);
            content.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            if (content.panel != null) Subscribe();
            Apply();
        }

        private void OnResize(GeometryChangedEvent evt) => Apply();
        private void OnAttach(AttachToPanelEvent evt) { Subscribe(); Apply(); }
        private void OnDetach(DetachFromPanelEvent evt) => DeucarianEditorAppearance.Changed -= Apply;

        private void Subscribe()
        {
            DeucarianEditorAppearance.Changed -= Apply;
            DeucarianEditorAppearance.Changed += Apply;
        }

        private void Apply()
        {
            if (disposed) return;
            int percent = DeucarianEditorAppearance.WorkspaceScalePercent;
            slider.SetValueWithoutNotify(percent);
            reset.text = percent + "%";
            float scale = DefaultScale * percent / 100f;
            content.style.scale = new Scale(new Vector3(scale, scale, 1));
            // Layout in logical pixels first; transforming alone would clip enlarged controls.
            content.style.width = Length.Percent(100f / scale);
            content.style.height = Length.Percent(100f / scale);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            DeucarianEditorAppearance.Changed -= Apply;
            viewport.UnregisterCallback<GeometryChangedEvent>(OnResize);
            footer.RemoveFromHierarchy();
            content.UnregisterCallback<AttachToPanelEvent>(OnAttach);
            content.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}
