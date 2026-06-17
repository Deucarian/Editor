using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorVisualShell
    {
        public const float SurfaceRadius = 8f;

        private const float BackgroundImageAlpha = 0.58f;
        private const string BackgroundAssetFileName = "DeucarianInstallerBackground.png";

        private static bool backgroundLoadAttempted;
        private static Texture2D backgroundTexture;

        public static Color DeepBackground
        {
            get { return new Color(0.012f, 0.020f, 0.035f, 1f); }
        }

        public static Color MainPanel
        {
            get { return new Color(23f / 255f, 32f / 255f, 39f / 255f, 0.72f); }
        }

        public static Color NestedSurface
        {
            get { return new Color(32f / 255f, 47f / 255f, 56f / 255f, 0.62f); }
        }

        public static Color HeaderPanel
        {
            get { return new Color(35f / 255f, 52f / 255f, 61f / 255f, 0.68f); }
        }

        public static Color Border
        {
            get { return new Color(90f / 255f, 111f / 255f, 160f / 255f, 0.35f); }
        }

        public static Color SubtleBorder
        {
            get { return new Color(90f / 255f, 111f / 255f, 160f / 255f, 0.24f); }
        }

        public static Color InteractiveBorder
        {
            get { return new Color(59f / 255f, 166f / 255f, 154f / 255f, 0.55f); }
        }

        public static Color Text
        {
            get { return new Color(0.88f, 0.93f, 0.96f, 1f); }
        }

        public static Color MutedText
        {
            get { return new Color(0.58f, 0.68f, 0.75f, 1f); }
        }

        public static VisualElement CreateWindowShell(VisualElement root, Texture2D background = null)
        {
            if (root == null)
            {
                return null;
            }

            root.Clear();
            DeucarianEditorUIResources.TryAddSharedStyleSheet(root);
            root.AddToClassList("deucarian-editor");
            root.AddToClassList("deucarian-window-shell-host");

            VisualElement shell = new VisualElement { name = "deucarian-window-shell" };
            shell.AddToClassList("deucarian-window-shell");

            VisualElement backgroundLayer = new VisualElement { name = "deucarian-window-background" };
            backgroundLayer.AddToClassList("deucarian-window-background");

            Texture2D resolvedBackground = background != null ? background : GetDefaultBackgroundTexture();

            if (resolvedBackground != null)
            {
                backgroundLayer.style.backgroundImage = new StyleBackground(resolvedBackground);
                backgroundLayer.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            }

            VisualElement overlay = new VisualElement { name = "deucarian-window-overlay" };
            overlay.AddToClassList("deucarian-window-overlay");

            VisualElement content = new VisualElement { name = "deucarian-window-content" };
            content.AddToClassList("deucarian-window-content");

            shell.Add(backgroundLayer);
            shell.Add(overlay);
            shell.Add(content);
            root.Add(shell);

            return content;
        }

        public static VisualElement CreateHeader(string title, string subtitle)
        {
            VisualElement header = new VisualElement();
            header.AddToClassList("deucarian-panel");
            header.AddToClassList("deucarian-panel--header");

            Label titleLabel = new Label(title ?? string.Empty);
            titleLabel.AddToClassList("deucarian-header__title");
            header.Add(titleLabel);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                Label subtitleLabel = new Label(subtitle);
                subtitleLabel.AddToClassList("deucarian-header__subtitle");
                header.Add(subtitleLabel);
            }

            return header;
        }

        public static VisualElement CreatePanel(params string[] additionalClasses)
        {
            VisualElement panel = new VisualElement();
            panel.AddToClassList("deucarian-panel");

            if (additionalClasses != null)
            {
                foreach (string additionalClass in additionalClasses)
                {
                    if (!string.IsNullOrWhiteSpace(additionalClass))
                    {
                        panel.AddToClassList(additionalClass);
                    }
                }
            }

            return panel;
        }

        public static VisualElement CreateToolbarRow()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("deucarian-toolbar-row");
            return row;
        }

        public static Texture2D GetDefaultBackgroundTexture()
        {
            if (backgroundLoadAttempted)
            {
                return backgroundTexture;
            }

            backgroundLoadAttempted = true;
            backgroundTexture = DeucarianEditorUIResources.LoadInstallerBackground();

            if (backgroundTexture != null)
            {
                return backgroundTexture;
            }

            string[] guids = AssetDatabase.FindAssets("DeucarianInstallerBackground t:Texture2D");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrWhiteSpace(path) &&
                    path.EndsWith("/" + BackgroundAssetFileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                    if (backgroundTexture != null)
                    {
                        break;
                    }
                }
            }

            return backgroundTexture;
        }

        public static void DrawWindowBackground(Rect rect)
        {
            DrawWindowBackground(rect, DeepBackground, null);
        }

        public static void DrawWindowBackground(Rect rect, Color fallbackColor, Texture2D background = null)
        {
            Event currentEvent = Event.current;

            if (currentEvent == null || currentEvent.type != EventType.Repaint || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            EditorGUI.DrawRect(rect, fallbackColor);

            Texture2D resolvedBackground = background != null ? background : GetDefaultBackgroundTexture();

            if (resolvedBackground == null)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, BackgroundImageAlpha);
            GUI.DrawTexture(rect, resolvedBackground, ScaleMode.ScaleAndCrop, true);
            GUI.color = previousColor;

            EditorGUI.DrawRect(rect, new Color(0.005f, 0.014f, 0.030f, 0.24f));
        }

        public static void DrawFrostedSurface(Rect rect, Color backgroundColor, Color borderColor)
        {
            DrawRoundedSurface(rect, backgroundColor, borderColor, SurfaceRadius, true);
        }

        public static void DrawInsetSurface(Rect rect, Color backgroundColor, Color borderColor, float radius)
        {
            DrawRoundedSurface(rect, backgroundColor, borderColor, radius, false);
        }

        private static void DrawRoundedSurface(
            Rect rect,
            Color backgroundColor,
            Color borderColor,
            float radius,
            bool drawShadow)
        {
            Event currentEvent = Event.current;

            if (currentEvent == null || currentEvent.type != EventType.Repaint || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            Rect alignedRect = AlignToPixels(rect);
            radius = Mathf.Min(radius, Mathf.Min(alignedRect.width, alignedRect.height) * 0.5f);

            if (drawShadow)
            {
                Rect shadowRect = new Rect(
                    alignedRect.x + 1f,
                    alignedRect.y + 2f,
                    alignedRect.width,
                    alignedRect.height);
                DrawRoundedFill(shadowRect, radius, new Color(0f, 0f, 0f, 0.14f));
            }

            DrawRoundedFill(alignedRect, radius, borderColor);

            Rect innerRect = new Rect(
                alignedRect.x + 1f,
                alignedRect.y + 1f,
                Mathf.Max(0f, alignedRect.width - 2f),
                Mathf.Max(0f, alignedRect.height - 2f));
            DrawRoundedFill(innerRect, Mathf.Max(0f, radius - 1f), backgroundColor);

            if (innerRect.width > 8f && innerRect.height > 2f)
            {
                DrawRoundedFill(
                    new Rect(innerRect.x + radius, innerRect.y, innerRect.width - radius * 2f, 1f),
                    0f,
                    new Color(0.75f, 0.94f, 1f, 0.08f));
            }
        }

        private static Rect AlignToPixels(Rect rect)
        {
            return new Rect(
                Mathf.Floor(rect.x),
                Mathf.Floor(rect.y),
                Mathf.Ceil(rect.width),
                Mathf.Ceil(rect.height));
        }

        private static void DrawRoundedFill(Rect rect, float radius, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f || color.a <= 0f)
            {
                return;
            }

            radius = Mathf.Min(radius, Mathf.Min(rect.width, rect.height) * 0.5f);

            if (radius < 1f)
            {
                EditorGUI.DrawRect(rect, color);
                return;
            }

            float middleWidth = Mathf.Max(0f, rect.width - radius * 2f);
            float middleHeight = Mathf.Max(0f, rect.height - radius * 2f);

            if (middleWidth > 0f)
            {
                EditorGUI.DrawRect(new Rect(rect.x + radius, rect.y, middleWidth, rect.height), color);
            }

            if (middleHeight > 0f)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + radius, radius, middleHeight), color);
                EditorGUI.DrawRect(new Rect(rect.xMax - radius, rect.y + radius, radius, middleHeight), color);
            }

            int rows = Mathf.CeilToInt(radius);
            float radiusSquared = radius * radius;

            for (int row = 0; row < rows; row++)
            {
                float sample = radius - row - 0.5f;
                float inset = radius - Mathf.Sqrt(Mathf.Max(0f, radiusSquared - sample * sample));
                float width = rect.width - inset * 2f;

                if (width <= 0f)
                {
                    continue;
                }

                EditorGUI.DrawRect(new Rect(rect.x + inset, rect.y + row, width, 1f), color);
                EditorGUI.DrawRect(new Rect(rect.x + inset, rect.yMax - row - 1f, width, 1f), color);
            }
        }
    }
}
