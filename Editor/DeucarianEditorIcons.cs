using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorIcons
    {
        private const string LucideIconRoot = DeucarianEditorUIResources.IconsPath + "/Lucide";

        private static readonly HashSet<string> KnownIconIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                DeucarianEditorIconIds.Warning,
                DeucarianEditorIconIds.Undo,
                DeucarianEditorIconIds.Check,
                DeucarianEditorIconIds.Wrench,
                DeucarianEditorIconIds.CreateFolder,
                DeucarianEditorIconIds.CreatePackage,
                DeucarianEditorIconIds.OpenFolder,
                DeucarianEditorIconIds.Palette,
                DeucarianEditorIconIds.History,
                DeucarianEditorIconIds.Refresh,
                DeucarianEditorIconIds.Monitor,
                DeucarianEditorIconIds.Copy,
                DeucarianEditorIconIds.Info,
                DeucarianEditorIconIds.Reset,
                DeucarianEditorIconIds.Logging,
                DeucarianEditorIconIds.ChevronDown,
                DeucarianEditorIconIds.ChevronRight
            };

        private static readonly Dictionary<string, IconDefinition> PackageIcons =
            new Dictionary<string, IconDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "package-installer", new IconDefinition("Package Installer", DeucarianEditorIconIds.CreatePackage, "d_Package Manager", "Package Manager") },
                { "theming", new IconDefinition("Theming", DeucarianEditorIconIds.Palette, "d_SceneViewFx", "SceneViewFx") },
                { "diagnostics", new IconDefinition("Diagnostics", DeucarianEditorIconIds.Info, "d_console.infoicon", "console.infoicon") },
                { "logging", new IconDefinition("Logging", DeucarianEditorIconIds.Logging, "d_UnityEditor.ConsoleWindow", "UnityEditor.ConsoleWindow") },
                { "object-loading", new IconDefinition("Object Loading", DeucarianEditorIconIds.OpenFolder, "d_Prefab Icon", "Prefab Icon") },
                { "api-helper", new IconDefinition("API Helper", null) },
                { "session", new IconDefinition("Session", null) },
                { "selection", new IconDefinition("Selection", null) },
                { "generic-ui-items", new IconDefinition("Generic UI Items", null) },
                { "editor", new IconDefinition("Editor", DeucarianEditorIconIds.Wrench, "d_UnityEditor.InspectorWindow", "UnityEditor.InspectorWindow") }
            };

        private static readonly Dictionary<string, Texture2D> FallbackIcons =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Texture2D> SharedIcons =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Loads a curated shared icon, returning the existing fallback when the ID is unknown or missing.</summary>
        public static Texture2D GetIcon(string iconId)
        {
            string normalizedId = string.IsNullOrWhiteSpace(iconId) ? string.Empty : iconId.Trim();
            Texture2D cached;
            if (SharedIcons.TryGetValue(normalizedId, out cached) && cached != null)
            {
                return cached;
            }

            if (KnownIconIds.Contains(normalizedId))
            {
                Texture2D icon = DeucarianEditorUIResources.LoadTexture(
                    LucideIconRoot + "/" + normalizedId + ".png");
                if (icon != null)
                {
                    SharedIcons[normalizedId] = icon;
                    return icon;
                }
            }

            return GetFallbackIcon(normalizedId);
        }

        /// <summary>Creates IMGUI content using a shared icon and visible text.</summary>
        public static GUIContent GetIconContent(string iconId, string text, string tooltip = null)
        {
            return new GUIContent(text ?? string.Empty, GetIcon(iconId), tooltip ?? string.Empty);
        }

        public static bool IsKnownIconId(string iconId)
        {
            return !string.IsNullOrWhiteSpace(iconId) && KnownIconIds.Contains(iconId.Trim());
        }

        /// <summary>Draws a shared white glyph tinted for the current editor skin.</summary>
        public static void DrawIcon(Rect rect, Texture2D icon, Color? tint = null)
        {
            if (icon == null || Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = tint ?? DeucarianEditorTheme.Text;
            GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
        }

        public static Texture2D GetPackageIcon(string packageKey)
        {
            IconDefinition definition;
            if (!string.IsNullOrWhiteSpace(packageKey) && PackageIcons.TryGetValue(packageKey.Trim(), out definition))
            {
                if (!string.IsNullOrWhiteSpace(definition.AssetIconId))
                {
                    Texture2D sharedIcon = GetIcon(definition.AssetIconId);
                    if (sharedIcon != null)
                    {
                        return sharedIcon;
                    }
                }

                Texture2D icon = GetBuiltInIcon(definition.IconNames);
                if (icon != null)
                {
                    return icon;
                }

                return GetFallbackIcon(definition.Label);
            }

            return GetFallbackIcon(packageKey);
        }

        public static Texture2D GetFallbackIcon(string label)
        {
            string fallbackKey = string.IsNullOrWhiteSpace(label) ? "package" : label.Trim();

            Texture2D cached;
            if (FallbackIcons.TryGetValue(fallbackKey, out cached) && cached != null)
            {
                return cached;
            }

            Texture2D builtIn = GetBuiltInIcon("d_Package Manager", "Package Manager", "d_Folder Icon", "Folder Icon");
            if (builtIn != null)
            {
                FallbackIcons[fallbackKey] = builtIn;
                return builtIn;
            }

            Texture2D generated = CreateBadgeTexture(fallbackKey);
            FallbackIcons[fallbackKey] = generated;
            return generated;
        }

        public static GUIContent GetPackageContent(string packageKey, string title, string tooltip = null)
        {
            return new GUIContent(title ?? string.Empty, GetPackageIcon(packageKey), tooltip ?? string.Empty);
        }

        public static bool IsKnownPackageKey(string packageKey)
        {
            return !string.IsNullOrWhiteSpace(packageKey) && PackageIcons.ContainsKey(packageKey.Trim());
        }

        private static Texture2D GetBuiltInIcon(params string[] iconNames)
        {
            if (iconNames == null)
            {
                return null;
            }

            for (int i = 0; i < iconNames.Length; i++)
            {
                string iconName = iconNames[i];
                if (string.IsNullOrWhiteSpace(iconName))
                {
                    continue;
                }

                try
                {
                    GUIContent content = EditorGUIUtility.IconContent(iconName);
                    Texture2D texture = content != null ? content.image as Texture2D : null;
                    if (texture != null)
                    {
                        return texture;
                    }
                }
                catch
                {
                    // Built-in icon names differ between Unity versions. Missing icons should never break a window.
                }
            }

            return null;
        }

        private static Texture2D CreateBadgeTexture(string label)
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = "Deucarian Editor Icon " + label
            };

            Color32 background = ToColor32(DeucarianEditorColors.Slate);
            Color32 accent = ToColor32(DeucarianEditorColors.Teal);
            Color32 border = ToColor32(DeucarianEditorColors.Border);
            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                    bool isAccent = y >= size - 7 || x < 6;
                    pixels[y * size + x] = isBorder ? border : isAccent ? accent : background;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static Color32 ToColor32(Color color)
        {
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Clamp01(color.r) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(color.g) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(color.b) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(color.a) * 255f));
        }

        private sealed class IconDefinition
        {
            public IconDefinition(string label, string assetIconId, params string[] iconNames)
            {
                Label = label;
                AssetIconId = assetIconId;
                IconNames = iconNames ?? Array.Empty<string>();
            }

            public string Label { get; private set; }

            public string AssetIconId { get; private set; }

            public string[] IconNames { get; private set; }
        }
    }
}
