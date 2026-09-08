using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorWorkbenchToolbar
    {
        public const string ToolbarClass = "deucarian-workbench-toolbar";
        public const string SummaryClass = "deucarian-workbench-toolbar__summary";
        public const string ActionClass = "deucarian-workbench-toolbar__action";
        public const string StandardActionClass = "deucarian-workbench-toolbar__action--standard";
        public const string EmphasizedActionClass = "deucarian-workbench-toolbar__action--emphasized";
        public const string ToggleClass = "deucarian-workbench-toolbar__toggle";
        public const string ToggleActiveClass = "deucarian-workbench-toolbar__toggle--active";
        public const string IconActionClass = "deucarian-workbench-toolbar__action--icon";
        public const string IconClass = "deucarian-workbench-toolbar__icon";
        public const string IconLabelClass = "deucarian-workbench-toolbar__icon-label";
        public const string SpacerClass = "deucarian-workbench-toolbar__spacer";
        public const string StableActionLanesClass = "deucarian-workbench-toolbar--stable-action-lanes";
        public const string CompactSingleLineClass = "deucarian-workbench-toolbar--compact-single-line";
        public const string GroupClass = "deucarian-workbench-toolbar__group";
        public const string NavigationGroupClass = "deucarian-workbench-toolbar__group--navigation";
        public const string ActionGroupClass = "deucarian-workbench-toolbar__group--actions";
        public const string ReservedSlotClass = "deucarian-workbench-toolbar__reserved-slot";
        public const string StatusPillClass = "deucarian-workbench-toolbar__status-pill";

        public static VisualElement CreateToolbar()
        {
            return CreateToolbar(DeucarianEditorWorkbenchToolbarLayout.Responsive);
        }

        public static VisualElement CreateToolbar(DeucarianEditorWorkbenchToolbarLayout layout)
        {
            VisualElement toolbar = DeucarianEditorVisualShell.CreateToolbarRow();
            toolbar.name = "deucarian-workbench-toolbar";
            toolbar.AddToClassList(ToolbarClass);
            toolbar.EnableInClassList(
                StableActionLanesClass,
                layout == DeucarianEditorWorkbenchToolbarLayout.StableActionLanes);
            toolbar.EnableInClassList(
                CompactSingleLineClass,
                layout == DeucarianEditorWorkbenchToolbarLayout.CompactSingleLine);
            return toolbar;
        }

        public static VisualElement CreateGroup(bool actions = false)
        {
            var group = new VisualElement();
            group.AddToClassList(GroupClass);
            group.AddToClassList(actions ? ActionGroupClass : NavigationGroupClass);
            return group;
        }

        public static VisualElement CreateReservedActionSlot(float width)
        {
            float safeWidth = Mathf.Max(0f, width);
            var slot = new VisualElement();
            slot.AddToClassList(ReservedSlotClass);
            slot.style.width = safeWidth;
            slot.style.minWidth = safeWidth;
            slot.style.maxWidth = safeWidth;
            return slot;
        }

        public static void SetReservedAction(
            VisualElement slot,
            VisualElement content,
            bool visible = true)
        {
            if (slot == null)
            {
                return;
            }

            slot.Clear();
            if (content != null)
            {
                slot.Add(content);
            }

            SetReservedActionVisible(slot, visible && content != null);
        }

        public static void SetReservedActionVisible(VisualElement slot, bool visible)
        {
            if (slot == null)
            {
                return;
            }

            slot.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
            slot.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
        }

        public static VisualElement CreateStatusPill(string iconId, string text, string tooltip = null)
        {
            var pill = new VisualElement { tooltip = tooltip ?? string.Empty };
            pill.AddToClassList(StatusPillClass);
            VisualElement content = DeucarianEditorIconTextButton.CreateContent(iconId, text);
            Image icon = content.Q<Image>(className: DeucarianEditorIconTextButton.IconClass);
            if (icon != null)
            {
                icon.tintColor = DeucarianEditorTheme.Success;
            }

            pill.Add(content);
            return pill;
        }

        public static Label CreateSummary(string text)
        {
            var summary = new Label(text ?? string.Empty);
            summary.AddToClassList(SummaryClass);
            summary.style.height = DeucarianEditorLayoutMetrics.TextLineHeight;
            summary.style.minHeight = DeucarianEditorLayoutMetrics.TextLineHeight;
            summary.style.maxHeight = DeucarianEditorLayoutMetrics.TextLineHeight;
            return summary;
        }

        public static Button CreateActionButton(string text, Action clicked, bool emphasized = false)
        {
            var button = new Button(clicked) { text = text ?? string.Empty };
            button.AddToClassList(ActionClass);
            button.AddToClassList(emphasized ? EmphasizedActionClass : StandardActionClass);
            return button;
        }

        public static Button CreateIconActionButton(
            string iconId,
            string text,
            Action clicked,
            bool emphasized = false,
            string tooltip = null)
        {
            Button button = CreateActionButton(string.Empty, clicked, emphasized);
            SetButtonIcon(button, iconId, text, tooltip);
            return button;
        }

        public static Button CreateToggleButton(string text, Action clicked, bool active = false)
        {
            var button = new Button(clicked) { text = text ?? string.Empty };
            button.AddToClassList(ActionClass);
            button.AddToClassList(ToggleClass);
            button.AddToClassList("deucarian-toggle-button");
            SetToggleActive(button, active);
            return button;
        }

        public static Button CreateIconToggleButton(
            string iconId,
            string text,
            Action clicked,
            bool active = false,
            string tooltip = null)
        {
            Button button = CreateToggleButton(string.Empty, clicked, active);
            SetButtonIcon(button, iconId, text, tooltip);
            return button;
        }

        public static void SetIconActionButtonText(Button button, string text)
        {
            DeucarianEditorIconTextButton.SetText(button, text);
        }

        public static void SetToggleActive(VisualElement toggle, bool active)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.EnableInClassList(ToggleActiveClass, active);
            toggle.EnableInClassList("deucarian-toggle-button--active", active);
        }

        public static VisualElement CreateSpacer()
        {
            var spacer = new VisualElement();
            spacer.AddToClassList(SpacerClass);
            spacer.AddToClassList("deucarian-toolbar-spacer");
            return spacer;
        }

        public static void SetButtonIcon(
            Button button,
            string iconId,
            string text,
            string tooltip)
        {
            if (button == null)
            {
                return;
            }

            button.AddToClassList(IconActionClass);
            DeucarianEditorIconTextButton.Configure(button, iconId, text, tooltip);
        }
    }
}
