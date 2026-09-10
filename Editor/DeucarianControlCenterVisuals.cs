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
            heading.style.marginTop = 20f;
            heading.style.marginBottom = 8f;
            parent.Add(heading);
            if (!string.IsNullOrWhiteSpace(description))
            {
                parent.Add(CreateMutedLabel(description));
            }
        }

        internal static Label CreateLabel(string text, bool strong)
        {
            var label = new Label(text ?? string.Empty);
            label.AddToClassList("dw-label");
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
            label.AddToClassList("dw-muted");
            label.style.marginBottom = 4f;
            return label;
        }

        internal static void StylePanel(VisualElement element)
        {
            element.AddToClassList("dw-summary-card");
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
