using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{




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







}
