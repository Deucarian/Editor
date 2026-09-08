using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>
    /// Shared icon-and-label button composition for UI Toolkit and IMGUI editor surfaces.
    /// Surface-specific factories may add their own visual class, but the icon metrics,
    /// content order, spacing, and text updates always come from this archetype.
    /// </summary>
    public static class DeucarianEditorIconTextButton
    {
        public const string RootClass = "deucarian-icon-text-button";
        public const string ContentClass = "deucarian-icon-text-button__content";
        public const string IconClass = "deucarian-icon-text-button__icon";
        public const string GapClass = "deucarian-icon-text-button__gap";
        public const string LabelClass = "deucarian-icon-text-button__label";
        public const string LeadingClass = "deucarian-icon-text-button--leading";
        public const float IconSize = DeucarianEditorLayoutMetrics.IconSize;
        public const float IconTextGap = DeucarianEditorLayoutMetrics.IconTextGap;
        public const float HorizontalPadding = DeucarianEditorLayoutMetrics.IconTextHorizontalPadding;
        public const float VerticalPadding = DeucarianEditorLayoutMetrics.IconTextVerticalPadding;
        public const float TextHeight = DeucarianEditorLayoutMetrics.TextLineHeight;
        public const float ImGuiLabelHeight = TextHeight;

        public static Button Create(
            string iconId,
            string text,
            Action clicked,
            string tooltip = null,
            bool leading = false)
        {
            var button = new Button(clicked);
            Configure(button, iconId, text, tooltip, leading);
            return button;
        }

        public static void Configure(
            Button button,
            string iconId,
            string text,
            string tooltip = null,
            bool leading = false)
        {
            if (button == null)
            {
                return;
            }

            button.Clear();
            button.text = string.Empty;
            button.tooltip = tooltip ?? string.Empty;
            button.AddToClassList(RootClass);
            button.EnableInClassList(LeadingClass, leading);
            button.style.position = Position.Relative;
            button.style.height = DeucarianEditorLayoutMetrics.CommandControlHeight;
            button.style.minHeight = DeucarianEditorLayoutMetrics.CommandControlHeight;
            button.style.maxHeight = DeucarianEditorLayoutMetrics.CommandControlHeight;
            button.style.paddingLeft = HorizontalPadding;
            button.style.paddingRight = HorizontalPadding;
            button.style.paddingTop = VerticalPadding;
            button.style.paddingBottom = VerticalPadding;
            button.style.flexShrink = 0f;

            VisualElement content = CreateContent(iconId, text, leading);
            button.Add(content);
        }

        public static VisualElement CreateContent(
            string iconId,
            string text,
            bool leading = false)
        {
            var content = new VisualElement { pickingMode = PickingMode.Ignore };
            content.AddToClassList(ContentClass);
            content.style.flexDirection = FlexDirection.Row;
            content.style.alignItems = Align.Center;
            content.style.justifyContent = leading ? Justify.FlexStart : Justify.Center;
            content.style.height = TextHeight;
            content.style.minHeight = TextHeight;
            content.style.maxHeight = TextHeight;

            var icon = new Image
            {
                image = DeucarianEditorIcons.GetIcon(iconId),
                scaleMode = ScaleMode.ScaleToFit,
                tintColor = DeucarianEditorTheme.Text,
                pickingMode = PickingMode.Ignore
            };
            icon.AddToClassList(IconClass);
            icon.AddToClassList(DeucarianEditorWorkbenchToolbar.IconClass);
            icon.style.width = IconSize;
            icon.style.minWidth = IconSize;
            icon.style.maxWidth = IconSize;
            icon.style.height = IconSize;
            icon.style.minHeight = IconSize;
            icon.style.maxHeight = IconSize;

            var gap = new VisualElement { pickingMode = PickingMode.Ignore };
            gap.AddToClassList(GapClass);
            gap.style.width = IconTextGap;
            gap.style.minWidth = IconTextGap;
            gap.style.maxWidth = IconTextGap;
            gap.style.flexGrow = 0f;
            gap.style.flexShrink = 0f;

            var label = new Label(text ?? string.Empty)
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    color = DeucarianEditorTheme.Text
                }
            };
            label.AddToClassList(LabelClass);
            label.AddToClassList(DeucarianEditorWorkbenchToolbar.IconLabelClass);
            label.style.height = TextHeight;
            label.style.minHeight = TextHeight;
            label.style.maxHeight = TextHeight;

            content.Add(icon);
            content.Add(gap);
            content.Add(label);
            SetIconVisibility(content, !string.IsNullOrWhiteSpace(iconId));
            return content;
        }

        public static void SetText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            Label label = button.Q<Label>(className: LabelClass)
                ?? button.Q<Label>(
                    className: DeucarianEditorWorkbenchToolbar.IconLabelClass);
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
            else
            {
                button.text = text ?? string.Empty;
            }
        }

        public static void SetIcon(Button button, string iconId)
        {
            if (button == null)
            {
                return;
            }

            VisualElement content = button.Q<VisualElement>(className: ContentClass);
            Image icon = content?.Q<Image>(className: IconClass);
            if (content == null || icon == null)
            {
                return;
            }

            bool visible = !string.IsNullOrWhiteSpace(iconId);
            icon.image = visible ? DeucarianEditorIcons.GetIcon(iconId) : null;
            SetIconVisibility(content, visible);
        }

        private static void SetIconVisibility(VisualElement content, bool visible)
        {
            if (content == null)
            {
                return;
            }

            Image icon = content.Q<Image>(className: IconClass);
            VisualElement gap = content.Q<VisualElement>(className: GapClass);
            if (icon != null)
            {
                icon.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (gap != null)
            {
                gap.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public static void CalculateImGuiContentRects(
            Rect row,
            out Rect iconRect,
            out Rect textRect)
        {
            float iconY = Mathf.Round(row.center.y - IconSize * 0.5f);
            iconRect = new Rect(
                row.x + HorizontalPadding,
                iconY,
                IconSize,
                IconSize);
            textRect = new Rect(
                iconRect.xMax + IconTextGap,
                Mathf.Round(row.center.y - ImGuiLabelHeight * 0.5f),
                Mathf.Max(
                    0f,
                    row.xMax - iconRect.xMax - IconTextGap - HorizontalPadding),
                Mathf.Min(row.height, ImGuiLabelHeight));
        }
    }
}
