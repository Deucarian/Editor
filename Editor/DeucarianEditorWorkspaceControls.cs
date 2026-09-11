using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public enum DeucarianEditorButtonRole { Secondary, Primary, Quiet, Destructive, Icon }

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
            => Button(text, clicked, primary ? DeucarianEditorButtonRole.Primary : DeucarianEditorButtonRole.Secondary);

        public static Button Button(string text, Action clicked, DeucarianEditorButtonRole role)
        {
            var button = new Button(clicked) { text = text ?? string.Empty };
            button.AddToClassList("dw-button");
            if (role != DeucarianEditorButtonRole.Secondary)
                button.AddToClassList("dw-" + role.ToString().ToLowerInvariant());
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
            if (input is Label) row.AddToClassList("dw-readonly-field");
            if (input is DeucarianEditorColorField) row.AddToClassList("dw-color-row");
            var caption = Label(label, "dw-field-label");
            row.Add(caption);
            input.AddToClassList("dw-field-input");
            if (input.ClassListContains("unity-base-field") && !input.ClassListContains("dw-multiline") &&
                !(input is Toggle) && !(input is Slider) && !(input is SliderInt))
                input.AddToClassList("dw-single-line");
            if (input is Toggle && !(input is DeucarianEditorSwitch)) input.AddToClassList("dw-checkbox");
            if (string.IsNullOrEmpty(input.tooltip)) input.tooltip = label;
            row.Add(input);
            DeucarianEditorResponsiveLayout.AdaptToWidth(row, "dw-field-stacked", input is Label ? 320 : 520);
            return row;
        }

        public static ScrollView Scroll(string name)
        {
            var scroll = new DeucarianEditorScrollView { name = name };
            scroll.AddToClassList("dw-scroll");
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            return scroll;
        }

        public static VisualElement Search(string id, string prompt, out TextField input)
        {
            var search = Region(null, "dw-search");
            search.Add(Icon(DeucarianEditorIconIds.Search));
            input = new TextField { name = id, tooltip = prompt };
            search.Add(input);
            var placeholder = Label(prompt, "dw-search-placeholder");
            placeholder.pickingMode = PickingMode.Ignore;
            search.Add(placeholder);
            input.RegisterValueChangedCallback(evt => Show(placeholder, string.IsNullOrEmpty(evt.newValue)));
            return search;
        }

        public static VisualElement Split(VisualElement form, VisualElement preview, float stackBelow = 840)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (preview == null) throw new ArgumentNullException(nameof(preview));
            var split = Region("workspace-form-preview", "dw-split");
            form.AddToClassList("dw-form-pane");
            preview.AddToClassList("dw-preview-pane");
            split.Add(form);
            split.Add(preview);
            DeucarianEditorResponsiveLayout.AdaptToWidth(split, "dw-split-stacked", stackBelow);
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

        public static Button IconButton(string label, string icon, Action clicked,
            DeucarianEditorButtonRole role = DeucarianEditorButtonRole.Secondary)
        {
            var button = Button(string.Empty, clicked, role);
            button.AddToClassList("dw-icon-button");
            button.tooltip = label;
            button.Add(Icon(icon));
            if (!string.IsNullOrEmpty(label)) button.Add(Label(label));
            return button;
        }

        public static VisualElement Panel(string id, string title = null)
        {
            var panel = Region(id, "dw-panel");
            if (!string.IsNullOrEmpty(title)) panel.Add(Label(title, "dw-section-title"));
            return panel;
        }

        public static VisualElement Divider() => Region(null, "dw-divider");

        public static VisualElement IconPanel(string id, string iconId, VisualElement content)
        {
            var panel = Panel(id);
            panel.AddToClassList("dw-icon-panel");
            panel.Add(Icon(iconId));
            content.AddToClassList("dw-icon-panel-content");
            panel.Add(content);
            return panel;
        }

        public static VisualElement EndActions(params VisualElement[] actions)
        {
            var row = Actions(actions);
            row.AddToClassList("dw-end-actions");
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
