using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorWorkbenchSurfaces
    {
        public const string OperationSurfaceClass = "deucarian-workbench-operation-surface";
        public const string DrawerClass = "deucarian-workbench-operation-drawer";
        public const string DrawerExpandedClass = "deucarian-workbench-operation-drawer--expanded";
        public const string DrawerCollapsedClass = "deucarian-workbench-operation-drawer--collapsed";
        public const string DrawerScrollClass = "deucarian-workbench-operation-drawer__scroll";
        public const string OverlayDrawerHostClass = "deucarian-workbench__drawer--overlay";
        public const string DrawerContentClass = "deucarian-workbench-operation-content";
        public const string DrawerHeaderClass = "deucarian-workbench-operation-drawer__header";
        public const string DrawerHeaderTitleClass = "deucarian-workbench-operation-drawer__header-title";
        public const string DrawerColumnsClass = "deucarian-workbench-operation-drawer__columns";
        public const string DrawerColumnClass = "deucarian-workbench-operation-drawer__column";
        public const string DrawerGroupTitleClass = "deucarian-workbench-operation-drawer__group-title";
        public const string DrawerActionClass = "deucarian-workbench-operation-drawer__action";
        public const string RowClass = "deucarian-workbench-operation-row";
        public const string HeaderRowClass = "deucarian-workbench-operation-row--header";
        public const string OptionRowClass = "deucarian-workbench-operation-row--option";
        public const string MessageRowClass = "deucarian-workbench-operation-row--message";
        public const string PrimaryTextClass = "deucarian-workbench-operation-text--primary";
        public const string SecondaryTextClass = "deucarian-workbench-operation-text--secondary";
        public const string FooterClass = "deucarian-workbench-operation-footer";
        public const string FooterStatusClass = "deucarian-workbench-operation-footer__status";
        public const string FooterStatusContentClass = "deucarian-workbench-operation-footer__status-content";
        public const string FooterStatusIconClass = "deucarian-workbench-operation-footer__status-icon";
        public const string FooterStatusLabelClass = "deucarian-workbench-operation-footer__status-label";
        public const string FooterSummaryClass = "deucarian-workbench-operation-footer__summary";
        public const string FooterSpacerClass = "deucarian-workbench-operation-footer__spacer";
        public const string FooterActionClass = "deucarian-workbench-operation-footer__action";
        public const string FooterActionsClass = "deucarian-workbench-operation-footer__actions";
        public const string FooterVersionClass = "deucarian-workbench-operation-footer__version";
        public const string FooterStatusSuccessClass = "deucarian-workbench-operation-footer__status-icon--success";
        public const string FooterStatusNeutralClass = "deucarian-workbench-operation-footer__status-icon--neutral";
        public const string FooterStatusWarningClass = "deucarian-workbench-operation-footer__status-icon--warning";
        public const string FooterStatusErrorClass = "deucarian-workbench-operation-footer__status-icon--error";
        public const string FooterStatusBusyClass = "deucarian-workbench-operation-footer__status-icon--busy";

        public static DeucarianEditorWorkbenchDrawer CreateDrawer(bool expanded = false)
        {
            var root = new VisualElement { name = "deucarian-workbench-operation-drawer" };
            root.AddToClassList(OperationSurfaceClass);
            root.AddToClassList(DrawerClass);

            var scrollView = new ScrollView { name = "deucarian-workbench-operation-drawer-scroll" };
            scrollView.AddToClassList(DrawerScrollClass);

            var content = new VisualElement { name = "deucarian-workbench-operation-drawer-content" };
            content.AddToClassList(DrawerContentClass);
            scrollView.Add(content);
            root.Add(scrollView);
            SetDrawerExpanded(root, expanded);
            return new DeucarianEditorWorkbenchDrawer(root, scrollView, content);
        }

        public static void SetDrawerExpanded(VisualElement drawer, bool expanded)
        {
            if (drawer == null)
            {
                return;
            }

            drawer.EnableInClassList(DrawerExpandedClass, expanded);
            drawer.EnableInClassList(DrawerCollapsedClass, !expanded);
        }

        public static VisualElement CreateDrawerHeader(string title)
        {
            var header = new VisualElement();
            header.AddToClassList(DrawerHeaderClass);
            var titleLabel = new Label(title ?? string.Empty);
            titleLabel.AddToClassList(DrawerHeaderTitleClass);
            header.Add(titleLabel);
            header.Add(DeucarianEditorWorkbenchToolbar.CreateSpacer());
            return header;
        }

        public static VisualElement CreateDrawerColumns()
        {
            var columns = new VisualElement();
            columns.AddToClassList(DrawerColumnsClass);
            return columns;
        }

        public static VisualElement CreateDrawerColumn(string heading)
        {
            var column = new VisualElement();
            column.AddToClassList(DrawerColumnClass);
            var title = new Label(heading ?? string.Empty);
            title.AddToClassList(DrawerGroupTitleClass);
            column.Add(title);
            return column;
        }

        public static Button CreateDrawerAction(
            string iconId,
            string text,
            Action clicked,
            string tooltip = null)
        {
            Button button = DeucarianEditorIconTextButton.Create(
                iconId,
                text,
                clicked,
                tooltip ?? text ?? string.Empty,
                true);
            button.AddToClassList(DrawerActionClass);
            button.AddToClassList(DeucarianEditorWorkbenchToolbar.IconActionClass);
            return button;
        }

        public static VisualElement CreateRow(params string[] additionalClasses)
        {
            var row = new VisualElement();
            row.AddToClassList(RowClass);
            if (additionalClasses != null)
            {
                foreach (string className in additionalClasses)
                {
                    if (!string.IsNullOrWhiteSpace(className))
                    {
                        row.AddToClassList(className);
                    }
                }
            }

            return row;
        }

        public static DeucarianEditorWorkbenchFooter CreateFooter(
            string statusIcon,
            string status,
            string summary,
            string actionText,
            Action action,
            string version)
        {
            var root = new VisualElement { name = "deucarian-workbench-operation-footer" };
            root.AddToClassList(OperationSurfaceClass);
            root.AddToClassList(FooterClass);

            var statusGroup = new VisualElement();
            statusGroup.AddToClassList(FooterStatusClass);

            var icon = new Label(statusIcon ?? string.Empty);
            icon.AddToClassList(FooterStatusIconClass);
            icon.AddToClassList(DeucarianEditorIconTextButton.IconClass);
            bool hasFallbackIcon = !string.IsNullOrWhiteSpace(statusIcon);
            icon.style.display = hasFallbackIcon ? DisplayStyle.Flex : DisplayStyle.None;

            VisualElement statusContent = DeucarianEditorIconTextButton.CreateContent(
                null,
                status,
                true);
            statusContent.AddToClassList(FooterStatusContentClass);
            var iconImage = statusContent.Q<Image>(
                className: DeucarianEditorIconTextButton.IconClass);
            iconImage.AddToClassList(FooterStatusIconClass);
            iconImage.style.display = DisplayStyle.None;
            VisualElement statusGap = statusContent.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.GapClass);
            statusGap.style.display = hasFallbackIcon
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            var label = statusContent.Q<Label>(
                className: DeucarianEditorIconTextButton.LabelClass);
            label.AddToClassList(FooterStatusLabelClass);
            statusContent.Insert(0, icon);
            statusGroup.Add(statusContent);

            var summaryLabel = new Label(summary ?? string.Empty);
            summaryLabel.AddToClassList(FooterSummaryClass);

            var spacer = new VisualElement();
            spacer.AddToClassList(FooterSpacerClass);

            var actions = new VisualElement();
            actions.AddToClassList(FooterActionsClass);

            Button actionButton = DeucarianEditorIconTextButton.Create(
                null,
                actionText,
                action,
                actionText);
            actionButton.AddToClassList(FooterActionClass);
            actions.Add(actionButton);

            var versionLabel = new Label(version ?? string.Empty);
            versionLabel.AddToClassList(FooterVersionClass);

            root.Add(statusGroup);
            root.Add(summaryLabel);
            root.Add(spacer);
            root.Add(actions);
            root.Add(versionLabel);

            return new DeucarianEditorWorkbenchFooter(
                root,
                statusGroup,
                statusContent,
                icon,
                iconImage,
                statusGap,
                label,
                summaryLabel,
                spacer,
                actions,
                actionButton,
                versionLabel);
        }

        public static Button AddFooterAction(
            DeucarianEditorWorkbenchFooter footer,
            string iconId,
            string text,
            Action action,
            string tooltip = null,
            float width = 96f)
        {
            if (footer?.Actions == null)
            {
                return null;
            }

            float safeWidth = Mathf.Max(0f, width);
            Button button = DeucarianEditorIconTextButton.Create(
                iconId,
                text,
                action,
                tooltip);
            button.AddToClassList(FooterActionClass);
            button.AddToClassList(DeucarianEditorWorkbenchToolbar.IconActionClass);
            button.style.width = safeWidth;
            button.style.minWidth = safeWidth;
            button.style.maxWidth = safeWidth;
            footer.Actions.Add(button);
            return button;
        }

        public static void SetFooterIcon(DeucarianEditorWorkbenchFooter footer, string iconId)
        {
            if (footer?.StatusIcon == null || footer.StatusImage == null)
            {
                return;
            }

            bool hasIcon = !string.IsNullOrWhiteSpace(iconId);
            bool hasFallback = !hasIcon &&
                               !string.IsNullOrWhiteSpace(footer.StatusIcon.text);
            footer.StatusIcon.style.display = hasFallback
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            footer.StatusImage.style.display = hasIcon ? DisplayStyle.Flex : DisplayStyle.None;
            footer.StatusImage.image = hasIcon ? DeucarianEditorIcons.GetIcon(iconId) : null;
            if (footer.StatusGap != null)
            {
                footer.StatusGap.style.display = hasIcon || hasFallback
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }

        public static void SetFooterStatus(DeucarianEditorWorkbenchFooter footer, DeucarianEditorStatus status)
        {
            if (footer?.StatusIcon == null)
            {
                return;
            }

            footer.StatusIcon.EnableInClassList(FooterStatusSuccessClass, status == DeucarianEditorStatus.Success);
            footer.StatusIcon.EnableInClassList(FooterStatusWarningClass, status == DeucarianEditorStatus.Warning);
            footer.StatusIcon.EnableInClassList(FooterStatusErrorClass, status == DeucarianEditorStatus.Error);
            footer.StatusIcon.EnableInClassList(
                FooterStatusNeutralClass,
                status == DeucarianEditorStatus.Info || status == DeucarianEditorStatus.Disabled);
            footer.StatusIcon.EnableInClassList(FooterStatusBusyClass, false);
            if (footer.StatusImage != null)
            {
                footer.StatusImage.tintColor = DeucarianEditorStatusBadge.GetColor(status);
            }
        }

        public static void SetFooterBusy(DeucarianEditorWorkbenchFooter footer, bool busy)
        {
            if (footer?.StatusIcon != null)
            {
                footer.StatusIcon.EnableInClassList(FooterStatusBusyClass, busy);
            }
        }
    }
}
