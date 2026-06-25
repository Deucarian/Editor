using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorWindowChrome
    {
        public const string BackgroundLayerClass = "deucarian-fixed-wallpaper-layer";
        public const string OverlayLayerClass = "deucarian-fixed-wallpaper-overlay";
        public const string ReadabilityOverlayClass = "deucarian-readability-overlay";
        public const string SafeShellClass = "deucarian-wallpaper-safe-shell";
        public const string TopSafeFadeClass = "deucarian-wallpaper-top-safe-fade";

        public static void ConfigureFixedWallpaper(VisualElement root, VisualElement wallpaperHost = null, string topSafeFadeName = null)
        {
            if (root == null)
            {
                return;
            }

            VisualElement host = wallpaperHost ?? root;
            host.AddToClassList(SafeShellClass);
            host.style.overflow = Overflow.Hidden;

            VisualElement background = root.Q<VisualElement>("deucarian-window-background") ??
                                       host.Q<VisualElement>("deucarian-window-background");
            VisualElement overlay = root.Q<VisualElement>("deucarian-window-overlay") ??
                                    host.Q<VisualElement>("deucarian-window-overlay");

            ReparentWallpaperLayer(host, background, 0);
            ReparentWallpaperLayer(host, overlay, background != null ? 1 : 0);

            ConfigureFixedLayer(background, BackgroundLayerClass);
            ConfigureFixedLayer(overlay, OverlayLayerClass);
            overlay?.AddToClassList(ReadabilityOverlayClass);
            DeucarianEditorAmbientGlass.Install(host);
            EnsureWallpaperTopSafeFade(host, topSafeFadeName);
        }

        public static void DrawImGuiWindowBackground(Rect rect)
        {
            DeucarianEditorVisualShell.DrawWindowBackground(rect, DeucarianEditorTheme.WindowBackground);
        }

        private static void ReparentWallpaperLayer(VisualElement host, VisualElement layer, int index)
        {
            if (host == null || layer == null || layer.parent == host)
            {
                return;
            }

            layer.RemoveFromHierarchy();
            host.Insert(Mathf.Clamp(index, 0, host.childCount), layer);
        }

        private static void EnsureWallpaperTopSafeFade(VisualElement host, string topSafeFadeName)
        {
            if (host == null || string.IsNullOrWhiteSpace(topSafeFadeName) || host.Q<VisualElement>(topSafeFadeName) != null)
            {
                return;
            }

            VisualElement fade = new VisualElement { name = topSafeFadeName };
            fade.AddToClassList(TopSafeFadeClass);
            fade.pickingMode = PickingMode.Ignore;
            fade.style.position = Position.Absolute;
            fade.style.left = 0f;
            fade.style.right = 0f;
            fade.style.top = 0f;
            fade.style.height = 86f;
            host.Insert(host.childCount, fade);
        }

        private static void ConfigureFixedLayer(VisualElement element, string className)
        {
            if (element == null)
            {
                return;
            }

            element.AddToClassList(className);
            element.pickingMode = PickingMode.Ignore;
            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.right = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;
            element.style.translate = new Translate(0f, 0f, 0f);
            element.style.scale = new Scale(Vector3.one);
            element.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
        }
    }
}
