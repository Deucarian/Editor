using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorWorkbenchOptions
    {
        public DeucarianEditorWorkbenchOptions()
        {
            IncludeToolbar = true;
        }

        public bool IncludeToolbar { get; set; }
        public bool IncludeDrawer { get; set; }
        public bool IncludeFooter { get; set; }
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
                Toolbar = DeucarianEditorWorkbenchToolbar.CreateToolbar();
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

    public static class DeucarianEditorWorkbenchToolbar
    {
        public const string ToolbarClass = "deucarian-workbench-toolbar";
        public const string SummaryClass = "deucarian-workbench-toolbar__summary";
        public const string ActionClass = "deucarian-workbench-toolbar__action";
        public const string StandardActionClass = "deucarian-workbench-toolbar__action--standard";
        public const string EmphasizedActionClass = "deucarian-workbench-toolbar__action--emphasized";
        public const string ToggleClass = "deucarian-workbench-toolbar__toggle";
        public const string ToggleActiveClass = "deucarian-workbench-toolbar__toggle--active";
        public const string SpacerClass = "deucarian-workbench-toolbar__spacer";

        public static VisualElement CreateToolbar()
        {
            VisualElement toolbar = DeucarianEditorVisualShell.CreateToolbarRow();
            toolbar.name = "deucarian-workbench-toolbar";
            toolbar.AddToClassList(ToolbarClass);
            return toolbar;
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

        public static Button CreateToggleButton(string text, Action clicked, bool active = false)
        {
            var button = new Button(clicked) { text = text ?? string.Empty };
            button.AddToClassList(ActionClass);
            button.AddToClassList(ToggleClass);
            button.AddToClassList("deucarian-toggle-button");
            SetToggleActive(button, active);
            return button;
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
            Label statusLabel,
            Label summary,
            VisualElement spacer,
            Button action,
            Label version)
        {
            Root = root;
            Status = status;
            StatusIcon = statusIcon;
            StatusLabel = statusLabel;
            Summary = summary;
            Spacer = spacer;
            Action = action;
            Version = version;
        }

        public VisualElement Root { get; }
        public VisualElement Status { get; }
        public Label StatusIcon { get; }
        public Label StatusLabel { get; }
        public Label Summary { get; }
        public VisualElement Spacer { get; }
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
        public const string DrawerContentClass = "deucarian-workbench-operation-content";
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
            var label = new Label(status ?? string.Empty);
            label.AddToClassList(FooterStatusLabelClass);
            statusGroup.Add(icon);
            statusGroup.Add(label);

            var summaryLabel = new Label(summary ?? string.Empty);
            summaryLabel.AddToClassList(FooterSummaryClass);

            var spacer = new VisualElement();
            spacer.AddToClassList(FooterSpacerClass);

            var actionButton = new Button(action) { text = actionText ?? string.Empty };
            actionButton.AddToClassList(FooterActionClass);

            var versionLabel = new Label(version ?? string.Empty);
            versionLabel.AddToClassList(FooterVersionClass);

            root.Add(statusGroup);
            root.Add(summaryLabel);
            root.Add(spacer);
            root.Add(actionButton);
            root.Add(versionLabel);

            return new DeucarianEditorWorkbenchFooter(
                root,
                statusGroup,
                icon,
                label,
                summaryLabel,
                spacer,
                actionButton,
                versionLabel);
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
