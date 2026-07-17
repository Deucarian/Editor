using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>
    /// Canonical spacing metrics shared by UI Toolkit and IMGUI editor surfaces.
    /// Keep the matching USS declarations in DeucarianEditor.uss synchronized.
    /// </summary>
    public static class DeucarianEditorLayoutMetrics
    {
        public const int PageHorizontalPadding = 10;
        public const int PageVerticalPadding = 8;
        public const int PageTopPadding = PageVerticalPadding;
        public const int PageBottomPadding = PageVerticalPadding;
        public const int SurfaceHorizontalPadding = 10;
        public const int SurfaceVerticalPadding = 8;
        public const int SurfaceSpacing = 8;
        public const int FooterHorizontalPadding = 10;
        public const int FooterVerticalPadding = 0;
        public const int CommandControlHeight = 28;
        public const int CommandControlHorizontalPadding = 8;
        public const int TextLineHeight = 18;
        public const int CommandBarSingleRowHeight = 46;
        public const int CommandBarTwoRowHeight = 78;
        public const int FooterHeight = 34;
        public const int ControlHeight = CommandControlHeight;
        public const int CommandBarHeight = CommandBarSingleRowHeight;
        public const int CommandBarStackedHeight = CommandBarTwoRowHeight;
        public const int IconTextHorizontalPadding = CommandControlHorizontalPadding;
        public const int IconTextVerticalPadding = 0;
        public const int IconTextGap = 8;
        public const int IconSize = 14;
        public const int PackageHeaderHorizontalPadding = SurfaceHorizontalPadding;
        public const int PackageHeaderVerticalPadding = SurfaceVerticalPadding;
        public const int PackageHeaderIconSize = 24;
        public const int PackageHeaderIconTextGap = 10;
        public const int PackageHeaderBottomMargin = 8;
    }

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
        public bool IncludeHeader { get; set; }
        public bool IncludeDrawer { get; set; }
        public bool IncludeFooter { get; set; }
        public string HeaderPackageKey { get; set; }
        public string HeaderTitle { get; set; }
        public string HeaderSubtitle { get; set; }
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
        public const string HeaderClass = "deucarian-workbench__header";
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

            if (options.IncludeHeader)
            {
                Header = DeucarianEditorPackageHeader.Create(
                    options.HeaderPackageKey,
                    options.HeaderTitle,
                    options.HeaderSubtitle);
                Header.AddToClassList(HeaderClass);
                ShellContent.Add(Header);
            }

            if (options.IncludeToolbar)
            {
                Toolbar = DeucarianEditorCommandBar.Create(options.ToolbarLayout);
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
        public VisualElement Header { get; }
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

    /// <summary>
    /// Canonical three-lane command-bar composition. The leading and trailing lanes
    /// own controls while the summary remains the flexible middle label.
    /// </summary>
    public sealed class DeucarianEditorCommandBarLanes
    {
        internal DeucarianEditorCommandBarLanes(
            VisualElement root,
            VisualElement leading,
            Label summary,
            VisualElement trailing)
        {
            Root = root;
            Leading = leading;
            Summary = summary;
            Trailing = trailing;
        }

        public VisualElement Root { get; }
        public VisualElement Leading { get; }
        public Label Summary { get; }
        public VisualElement Trailing { get; }
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
            VisualElement statusContent,
            Label statusIcon,
            Image statusImage,
            VisualElement statusGap,
            Label statusLabel,
            Label summary,
            VisualElement spacer,
            VisualElement actions,
            Button action,
            Label version)
        {
            Root = root;
            Status = status;
            StatusContent = statusContent;
            StatusIcon = statusIcon;
            StatusImage = statusImage;
            StatusGap = statusGap;
            StatusLabel = statusLabel;
            Summary = summary;
            Spacer = spacer;
            Actions = actions;
            Action = action;
            Version = version;
        }

        public VisualElement Root { get; }
        public VisualElement Status { get; }
        public VisualElement StatusContent { get; }
        public Label StatusIcon { get; }
        public Image StatusImage { get; }
        public VisualElement StatusGap { get; }
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
