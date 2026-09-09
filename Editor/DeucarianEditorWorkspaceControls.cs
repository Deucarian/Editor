using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorWorkspaceControls
    {
        public static VisualElement Region(string name, string className)
        {
            var element = new VisualElement { name = name ?? string.Empty };
            element.AddToClassList(className);
            return element;
        }

        public static Label Label(string text, string className = "dw-label")
        {
            var label = new Label(text ?? string.Empty);
            label.AddToClassList(className);
            return label;
        }

        public static Button Button(string text, Action clicked, bool primary = false)
        {
            var button = new Button(clicked) { text = text ?? string.Empty };
            button.AddToClassList("dw-button");
            if (primary) button.AddToClassList("dw-primary");
            return button;
        }

        public static VisualElement Icon(string id)
        {
            var image = new VisualElement { pickingMode = PickingMode.Ignore };
            image.style.backgroundImage = new StyleBackground(DeucarianEditorIcons.GetIcon(id));
#if UNITY_2022_2_OR_NEWER
            image.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            image.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            image.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            image.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
#else
            image.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
#endif
            image.AddToClassList("dw-icon");
            return image;
        }

        public static VisualElement Field(string label, VisualElement input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var row = Region(null, "dw-field");
            var caption = Label(label, "dw-field-label");
            row.Add(caption);
            input.AddToClassList("dw-field-input");
            if (string.IsNullOrEmpty(input.tooltip)) input.tooltip = label;
            row.Add(input);
            DeucarianEditorResponsiveLayout.AdaptToWidth(row, "dw-field-stacked", input is Label ? 320 : 520);
            return row;
        }

        public static ScrollView Scroll(string name)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical) { name = name };
            scroll.AddToClassList("dw-scroll");
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            return scroll;
        }

        public static VisualElement Split(VisualElement form, VisualElement preview)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (preview == null) throw new ArgumentNullException(nameof(preview));
            var split = Region("workspace-form-preview", "dw-split");
            form.AddToClassList("dw-form-pane");
            preview.AddToClassList("dw-preview-pane");
            split.Add(form);
            split.Add(preview);
            DeucarianEditorResponsiveLayout.AdaptToWidth(split, "dw-split-stacked", 840);
            return split;
        }

        public static void Show(VisualElement element, bool visible)
        {
            if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static VisualElement Actions(params VisualElement[] actions)
        {
            var row = Region(null, "dw-actions");
            foreach (var action in actions) if (action != null) row.Add(action);
            return row;
        }

        public static IMGUIContainer Embedded(Action draw, string name)
        {
            var container = new IMGUIContainer(draw) { name = name };
            container.AddToClassList("dw-embedded");
            return container;
        }
    }
}
