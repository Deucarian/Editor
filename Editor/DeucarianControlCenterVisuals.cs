using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterVisuals
    {
        internal static void AddPageHeading(
            VisualElement parent,
            string title,
            string description)
        {
            Label heading = CreateLabel(title, true);
            heading.style.fontSize = 20f;
            heading.style.marginBottom = 3f;
            parent.Add(heading);
            parent.Add(CreateMutedLabel(description));
        }

        internal static void AddSectionHeading(
            VisualElement parent,
            string title,
            string description)
        {
            Label heading = CreateLabel(title, true);
            heading.style.fontSize = 15f;
            heading.style.marginTop = 10f;
            parent.Add(heading);
            if (!string.IsNullOrWhiteSpace(description))
            {
                parent.Add(CreateMutedLabel(description));
            }
        }

        internal static Label CreateLabel(string text, bool strong)
        {
            var label = new Label(text ?? string.Empty);
            label.style.color = DeucarianEditorTheme.Text;
            label.style.whiteSpace = WhiteSpace.Normal;
            if (strong)
            {
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            return label;
        }

        internal static Label CreateMutedLabel(string text)
        {
            Label label = CreateLabel(text, false);
            label.style.color = DeucarianEditorTheme.MutedText;
            label.style.marginBottom = 4f;
            return label;
        }

        internal static void StylePanel(VisualElement element)
        {
            Color panel = DeucarianEditorAppearance.DecorativeBackgrounds
                ? DeucarianEditorTheme.GlassPanelSoft : DeucarianEditorVisualShell.MainPanel;
            if (!DeucarianEditorAppearance.DecorativeBackgrounds) panel.a = 1f;
            element.style.backgroundColor = panel;
            element.style.borderTopWidth = 1f;
            element.style.borderRightWidth = 1f;
            element.style.borderBottomWidth = 1f;
            element.style.borderLeftWidth = 1f;
            SetBorderColor(element, DeucarianEditorTheme.BorderSubtle);
        }

        internal static void SetBorderColor(
            VisualElement element,
            Color color)
        {
            element.style.borderTopColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
        }

        internal static Color GetStatusColor(
            DeucarianControlCenterStatus status)
        {
            switch (status)
            {
                case DeucarianControlCenterStatus.Success:
                    return DeucarianEditorTheme.Success;
                case DeucarianControlCenterStatus.Warning:
                    return DeucarianEditorTheme.Warning;
                case DeucarianControlCenterStatus.Error:
                    return DeucarianEditorTheme.Error;
                default:
                    return DeucarianEditorTheme.Text;
            }
        }
    }
}
