using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Shared keyboard-accessible tabs and segmented choices, without domain state.</summary>
    public sealed class DeucarianEditorChoiceBar : VisualElement
    {
        private readonly List<Button> buttons = new List<Button>();
        public event Action<int> Changed;
        public int Value { get; private set; }

        public DeucarianEditorChoiceBar(IReadOnlyList<string> labels, int value = 0, bool tabs = false)
        {
            if (labels == null || labels.Count == 0) throw new ArgumentException("At least one choice is required.", nameof(labels));
            AddToClassList(tabs ? "dw-tab-bar" : "dw-choice-bar");
            for (int i = 0; i < labels.Count; i++)
            {
                int index = i;
                var button = DeucarianEditorWorkspaceControls.Button(labels[i], () => Select(index));
                button.name = "choice-" + i;
                button.EnableInClassList("dw-first", i == 0);
                button.EnableInClassList("dw-last", i == labels.Count - 1);
                button.RegisterCallback<KeyDownEvent>(evt => OnKeyDown(evt, index));
                buttons.Add(button);
                Add(button);
            }
            SetValueWithoutNotify(value);
        }

        public void SetValueWithoutNotify(int value)
        {
            if (value < 0 || value >= buttons.Count) throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
            for (int i = 0; i < buttons.Count; i++) buttons[i].EnableInClassList("dw-selected", i == value);
        }

        public void SetChoiceEnabled(int index, bool enabled, string explanation = null)
        {
            if (index < 0 || index >= buttons.Count) throw new ArgumentOutOfRangeException(nameof(index));
            buttons[index].SetEnabled(enabled);
            buttons[index].tooltip = explanation ?? string.Empty;
        }

        private void Select(int value)
        {
            if (Value == value || !buttons[value].enabledSelf) return;
            SetValueWithoutNotify(value);
            Changed?.Invoke(value);
        }

        private void OnKeyDown(KeyDownEvent evt, int index)
        {
            int direction = evt.keyCode == KeyCode.RightArrow ? 1 : evt.keyCode == KeyCode.LeftArrow ? -1 : 0;
            if (direction == 0) return;
            for (int offset = 1; offset < buttons.Count; offset++)
            {
                int next = (index + direction * offset + buttons.Count) % buttons.Count;
                if (!buttons[next].enabledSelf) continue;
                Select(next);
                buttons[next].Focus();
                break;
            }
            evt.StopPropagation();
#if UNITY_6000_0_OR_NEWER
            panel?.focusController.IgnoreEvent(evt);
#else
            evt.PreventDefault();
#endif
        }
    }
}
