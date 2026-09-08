using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>
    /// Canonical reusable command surface for editor navigation, contextual actions,
    /// and state. The legacy workbench-toolbar API remains available as a compatibility
    /// facade, while new editor windows should compose controls through this type.
    /// </summary>
    public static class DeucarianEditorCommandBar
    {
        public const string RootClass = "deucarian-command-bar";
        public const string NavigationGroupClass = "deucarian-command-bar__navigation";
        public const string ActionGroupClass = "deucarian-command-bar__actions";
        public const string ActionClass = "deucarian-command-bar__action";
        public const string ToggleClass = "deucarian-command-bar__toggle";
        public const string StateClass = "deucarian-command-bar__state";
        public const string ReservedSlotClass = "deucarian-command-bar__reserved-slot";
        public const string SpacerClass = "deucarian-command-bar__spacer";
        public const string LeadingLaneClass = "deucarian-command-bar__leading";
        public const string SummaryLaneClass = "deucarian-command-bar__summary";
        public const string TrailingLaneClass = "deucarian-command-bar__trailing";
        public const string LeadingClass = LeadingLaneClass;
        public const string SummaryClass = SummaryLaneClass;
        public const string TrailingClass = TrailingLaneClass;

        public static VisualElement Create(
            DeucarianEditorWorkbenchToolbarLayout layout =
                DeucarianEditorWorkbenchToolbarLayout.Responsive)
        {
            VisualElement commandBar = DeucarianEditorWorkbenchToolbar.CreateToolbar(layout);
            commandBar.AddToClassList(RootClass);
            return commandBar;
        }

        /// <summary>
        /// Appends the canonical leading, flexible summary, and trailing lanes to an
        /// existing command bar. Existing children are preserved; callers that want a
        /// fresh composition should clear the command bar before calling this method.
        /// </summary>
        public static DeucarianEditorCommandBarLanes CreateLanes(VisualElement commandBar)
        {
            if (commandBar == null)
            {
                throw new ArgumentNullException(nameof(commandBar));
            }

            commandBar.AddToClassList(RootClass);

            VisualElement leading = CreateNavigationGroup();
            leading.AddToClassList(LeadingLaneClass);

            Label summary = CreateSummary(string.Empty);
            summary.AddToClassList(SummaryLaneClass);

            VisualElement trailing = CreateActionGroup();
            trailing.AddToClassList(TrailingLaneClass);

            commandBar.Add(leading);
            commandBar.Add(summary);
            commandBar.Add(trailing);
            return new DeucarianEditorCommandBarLanes(
                commandBar,
                leading,
                summary,
                trailing);
        }

        public static VisualElement CreateNavigationGroup()
        {
            VisualElement group = DeucarianEditorWorkbenchToolbar.CreateGroup();
            group.AddToClassList(NavigationGroupClass);
            return group;
        }

        public static VisualElement CreateActionGroup()
        {
            VisualElement group = DeucarianEditorWorkbenchToolbar.CreateGroup(true);
            group.AddToClassList(ActionGroupClass);
            return group;
        }

        public static Button CreateAction(
            string iconId,
            string text,
            Action clicked,
            bool emphasized = false,
            string tooltip = null)
        {
            Button button = DeucarianEditorWorkbenchToolbar.CreateIconActionButton(
                iconId,
                text,
                clicked,
                emphasized,
                tooltip);
            button.AddToClassList(ActionClass);
            return button;
        }

        public static Button CreateToggle(
            string text,
            Action clicked,
            bool active = false,
            string iconId = null,
            string tooltip = null)
        {
            Button button = DeucarianEditorWorkbenchToolbar.CreateIconToggleButton(
                iconId,
                text,
                clicked,
                active,
                tooltip);
            button.tooltip = tooltip ?? button.tooltip;
            button.AddToClassList(ActionClass);
            button.AddToClassList(ToggleClass);
            return button;
        }

        public static VisualElement CreateState(string iconId, string text, string tooltip = null)
        {
            VisualElement state = DeucarianEditorWorkbenchToolbar.CreateStatusPill(
                iconId,
                text,
                tooltip);
            state.AddToClassList(StateClass);
            return state;
        }

        public static VisualElement CreateReservedSlot(float width)
        {
            VisualElement slot = DeucarianEditorWorkbenchToolbar.CreateReservedActionSlot(width);
            slot.AddToClassList(ReservedSlotClass);
            return slot;
        }

        public static void SetReservedContent(
            VisualElement slot,
            VisualElement content,
            bool visible = true)
        {
            DeucarianEditorWorkbenchToolbar.SetReservedAction(slot, content, visible);
        }

        public static void SetReservedVisible(VisualElement slot, bool visible)
        {
            DeucarianEditorWorkbenchToolbar.SetReservedActionVisible(slot, visible);
        }

        public static VisualElement CreateSpacer()
        {
            VisualElement spacer = DeucarianEditorWorkbenchToolbar.CreateSpacer();
            spacer.AddToClassList(SpacerClass);
            return spacer;
        }

        public static Label CreateSummary(string text)
        {
            return DeucarianEditorWorkbenchToolbar.CreateSummary(text);
        }

        public static void ConfigureAction(
            Button button,
            string iconId,
            string text,
            string tooltip = null)
        {
            DeucarianEditorWorkbenchToolbar.SetButtonIcon(button, iconId, text, tooltip);
            button?.AddToClassList(ActionClass);
        }

        public static void SetText(Button button, string text)
        {
            DeucarianEditorWorkbenchToolbar.SetIconActionButtonText(button, text);
        }

        public static void SetMinimumWidth(VisualElement control, float width)
        {
            if (control == null)
            {
                return;
            }

            control.style.minWidth = Mathf.Max(0f, width);
            control.style.flexShrink = 0f;
        }

        public static void SetActive(VisualElement toggle, bool active)
        {
            DeucarianEditorWorkbenchToolbar.SetToggleActive(toggle, active);
        }
    }
}
