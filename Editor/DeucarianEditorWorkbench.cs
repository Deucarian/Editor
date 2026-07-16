using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public enum DeucarianEditorWorkbenchToolbarLayout
    {
        Responsive,
        StableActionLanes,
        CompactSingleLine
    }

    public enum DeucarianEditorWorkbenchDrawerMode
    {
        Inline,
        Overlay
    }

    public sealed class DeucarianEditorWorkbenchOptions
    {
        public DeucarianEditorWorkbenchOptions()
        {
            IncludeToolbar = true;
            ToolbarLayout = DeucarianEditorWorkbenchToolbarLayout.Responsive;
            DrawerMode = DeucarianEditorWorkbenchDrawerMode.Inline;
        }

        public bool IncludeToolbar { get; set; }
        public bool IncludeDrawer { get; set; }
        public bool IncludeFooter { get; set; }
        public DeucarianEditorWorkbenchToolbarLayout ToolbarLayout { get; set; }
        public DeucarianEditorWorkbenchDrawerMode DrawerMode { get; set; }
        public Texture2D Background { get; set; }
        public string TopSafeFadeName { get; set; }
    }

    /// <summary>
    /// Composes the shared editor shell into a domain-neutral workbench. Callers own
    /// the controls and state added to the exposed regions.
    /// </summary>
    public sealed class DeucarianEditorWorkbench : IDisposable
    {
        public const string RootClass = "deucarian-workbench";
        public const string MainClass = "deucarian-workbench__main";
        public const string ContentClass = "deucarian-workbench__content";
        public const string DrawerClass = "deucarian-workbench__drawer";
        public const string FooterClass = "deucarian-workbench__footer";

        private readonly EventCallback<GeometryChangedEvent> geometryChangedCallback;
        private bool disposed;

        private DeucarianEditorWorkbench(VisualElement root, DeucarianEditorWorkbenchOptions options)
        {
            Root = root;
            ShellContent = DeucarianEditorVisualShell.CreateWindowShell(root, options.Background);
            if (ShellContent == null)
            {
                return;
            }

            ShellContent.AddToClassList(RootClass);
            DeucarianEditorWindowChrome.ConfigureFixedWallpaper(root, ShellContent, options.TopSafeFadeName);

            if (options.IncludeToolbar)
            {
                Toolbar = DeucarianEditorWorkbenchToolbar.CreateToolbar(options.ToolbarLayout);
                ShellContent.Add(Toolbar);
            }

            Main = new VisualElement { name = "deucarian-workbench-main" };
            Main.AddToClassList(MainClass);

            Content = new VisualElement { name = "deucarian-workbench-content" };
            Content.AddToClassList(ContentClass);
            Main.Add(Content);

            if (options.IncludeDrawer)
            {
                Drawer = new VisualElement { name = "deucarian-workbench-drawer" };
                Drawer.AddToClassList(DrawerClass);
                Drawer.EnableInClassList(
                    DeucarianEditorWorkbenchSurfaces.OverlayDrawerHostClass,
                    options.DrawerMode == DeucarianEditorWorkbenchDrawerMode.Overlay);
                Main.Add(Drawer);
            }

            ShellContent.Add(Main);

            if (options.IncludeFooter)
            {
                Footer = new VisualElement { name = "deucarian-workbench-footer" };
                Footer.AddToClassList(FooterClass);
                ShellContent.Add(Footer);
            }

            geometryChangedCallback = OnGeometryChanged;
            ShellContent.RegisterCallback(geometryChangedCallback);
            ApplyResponsiveLayout(GetUsableWidth(ShellContent.resolvedStyle.width));
        }

        public VisualElement Root { get; }
        public VisualElement ShellContent { get; }
        public VisualElement Toolbar { get; }
        public VisualElement Main { get; }
        public VisualElement Content { get; }
        public VisualElement Drawer { get; }
        public VisualElement Footer { get; }
        public DeucarianEditorLayoutMode LayoutMode { get; private set; }

        public static DeucarianEditorWorkbench Create(
            VisualElement root,
            DeucarianEditorWorkbenchOptions options = null)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            return new DeucarianEditorWorkbench(root, options ?? new DeucarianEditorWorkbenchOptions());
        }

        public DeucarianEditorLayoutMode ApplyResponsiveLayout(float width)
        {
            if (ShellContent == null)
            {
                return LayoutMode;
            }

            LayoutMode = DeucarianEditorResponsiveLayout.ApplyResponsiveClasses(ShellContent, width);
            return LayoutMode;
        }

        public IMGUIContainer AddImGuiContent(Action onGuiHandler, string name = null)
        {
            var container = new IMGUIContainer(onGuiHandler)
            {
                name = string.IsNullOrWhiteSpace(name) ? "deucarian-workbench-imgui" : name
            };
            container.AddToClassList("deucarian-workbench__imgui");
            Content?.Add(container);
            return container;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            if (ShellContent != null && geometryChangedCallback != null)
            {
                ShellContent.UnregisterCallback(geometryChangedCallback);
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ApplyResponsiveLayout(GetUsableWidth(evt.newRect.width));
        }

        private static float GetUsableWidth(float width)
        {
            return float.IsNaN(width) || float.IsInfinity(width) || width <= 0f
                ? DeucarianEditorResponsiveLayout.WorkbenchWideBreakpoint
                : width;
        }
    }

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
        public const string LabelClass = "deucarian-icon-text-button__label";
        public const string LeadingClass = "deucarian-icon-text-button--leading";
        public const float IconSize = 14f;
        public const float IconTextGap = 8f;
        public const float HorizontalPadding = 8f;

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

            var content = new VisualElement { pickingMode = PickingMode.Ignore };
            content.AddToClassList(ContentClass);

            var icon = new Image
            {
                image = DeucarianEditorIcons.GetIcon(iconId),
                scaleMode = ScaleMode.ScaleToFit,
                tintColor = DeucarianEditorTheme.Text,
                pickingMode = PickingMode.Ignore
            };
            icon.AddToClassList(IconClass);
            icon.style.display = string.IsNullOrWhiteSpace(iconId)
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            var label = new Label(text ?? string.Empty)
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    color = DeucarianEditorTheme.Text
                }
            };
            label.AddToClassList(LabelClass);

            content.Add(icon);
            content.Add(label);
            button.Add(content);
        }

        public static void SetText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            Label label = button.Q<Label>(className: LabelClass);
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
            else
            {
                button.text = text ?? string.Empty;
            }
        }

        public static void CalculateImGuiContentRects(
            Rect row,
            out Rect iconRect,
            out Rect textRect)
        {
            float iconY = row.y + (row.height - IconSize) * 0.5f;
            iconRect = new Rect(
                row.x + HorizontalPadding,
                iconY,
                IconSize,
                IconSize);
            textRect = new Rect(
                iconRect.xMax + IconTextGap,
                row.y,
                Mathf.Max(
                    0f,
                    row.xMax - iconRect.xMax - IconTextGap - HorizontalPadding),
                row.height);
        }
    }

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
        public const string IconClass = DeucarianEditorIconTextButton.IconClass;
        public const string IconLabelClass = DeucarianEditorIconTextButton.LabelClass;
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

            var icon = new Image
            {
                image = DeucarianEditorIcons.GetIcon(iconId),
                scaleMode = ScaleMode.ScaleToFit,
                tintColor = DeucarianEditorTheme.Success,
                pickingMode = PickingMode.Ignore
            };
            icon.AddToClassList(IconClass);

            var label = new Label(text ?? string.Empty) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(IconLabelClass);
            pill.Add(icon);
            pill.Add(label);
            return pill;
        }

        public static Label CreateSummary(string text)
        {
            var summary = new Label(text ?? string.Empty);
            summary.AddToClassList(SummaryClass);
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

    public sealed class DeucarianEditorWorkbenchDrawer
    {
        internal DeucarianEditorWorkbenchDrawer(VisualElement root, ScrollView scrollView, VisualElement content)
        {
            Root = root;
            ScrollView = scrollView;
            Content = content;
        }

        public VisualElement Root { get; }
        public ScrollView ScrollView { get; }
        public VisualElement Content { get; }
    }

    public sealed class DeucarianEditorWorkbenchFooter
    {
        internal DeucarianEditorWorkbenchFooter(
            VisualElement root,
            VisualElement status,
            Label statusIcon,
            Image statusImage,
            Label statusLabel,
            Label summary,
            VisualElement spacer,
            VisualElement actions,
            Button action,
            Label version)
        {
            Root = root;
            Status = status;
            StatusIcon = statusIcon;
            StatusImage = statusImage;
            StatusLabel = statusLabel;
            Summary = summary;
            Spacer = spacer;
            Actions = actions;
            Action = action;
            Version = version;
        }

        public VisualElement Root { get; }
        public VisualElement Status { get; }
        public Label StatusIcon { get; }
        public Image StatusImage { get; }
        public Label StatusLabel { get; }
        public Label Summary { get; }
        public VisualElement Spacer { get; }
        public VisualElement Actions { get; }
        public Button Action { get; }
        public Label Version { get; }
    }

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
            var iconImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            iconImage.AddToClassList(FooterStatusIconClass);
            iconImage.style.display = DisplayStyle.None;
            var label = new Label(status ?? string.Empty);
            label.AddToClassList(FooterStatusLabelClass);
            statusGroup.Add(icon);
            statusGroup.Add(iconImage);
            statusGroup.Add(label);

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
                icon,
                iconImage,
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
            footer.StatusIcon.style.display = hasIcon ? DisplayStyle.None : DisplayStyle.Flex;
            footer.StatusImage.style.display = hasIcon ? DisplayStyle.Flex : DisplayStyle.None;
            footer.StatusImage.image = hasIcon ? DeucarianEditorIcons.GetIcon(iconId) : null;
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
