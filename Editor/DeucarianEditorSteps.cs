using System;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorSteps
    {
        private readonly VisualElement[] steps;
        public VisualElement Root { get; }
        public DeucarianEditorSteps(params string[] labels)
        {
            if (labels == null || labels.Length == 0) throw new ArgumentException("At least one step is required.", nameof(labels));
            Root = DeucarianEditorWorkspaceControls.Region(null, "dw-steps");
            steps = new VisualElement[labels.Length];
            for (int index = 0; index < labels.Length; index++)
            {
                var step = DeucarianEditorWorkspaceControls.Region(null, "dw-step");
                var line = DeucarianEditorWorkspaceControls.Region(null, "dw-step-line");
                if (index == labels.Length - 1) line.style.display = DisplayStyle.None;
                step.Add(DeucarianEditorWorkspaceControls.Label((index + 1).ToString(), "dw-step-number"));
                step.Add(DeucarianEditorWorkspaceControls.Label(labels[index], "dw-step-label"));
                step.Add(line);
                Root.Add(step); steps[index] = step;
            }
            SetCurrent(0);
        }
        public void SetCurrent(int index)
        {
            if (index < 0 || index >= steps.Length) throw new ArgumentOutOfRangeException(nameof(index));
            for (int current = 0; current < steps.Length; current++)
                steps[current].EnableInClassList("dw-step-current", current == index);
        }
    }
}
