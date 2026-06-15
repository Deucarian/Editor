using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorIcons
    {
        private static readonly Dictionary<string, IconDefinition> PackageIcons =
            new Dictionary<string, IconDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "package-installer", new IconDefinition("Package Installer", "d_Package Manager", "Package Manager") },
                { "theming", new IconDefinition("Theming", "d_SceneViewFx", "SceneViewFx") },
                { "logging", new IconDefinition("Logging", "d_UnityEditor.ConsoleWindow", "UnityEditor.ConsoleWindow") },
                { "object-loading", new IconDefinition("Object Loading", "d_Prefab Icon", "Prefab Icon") },
                { "api-helper", new IconDefinition("API Helper") },
                { "session", new IconDefinition("Session") },
                { "selection", new IconDefinition("Selection") },
                { "generic-ui-items", new IconDefinition("Generic UI Items") },
                { "editor", new IconDefinition("Editor", "d_UnityEditor.InspectorWindow", "UnityEditor.InspectorWindow") }
            };

        private static readonly Dictionary<string, Texture2D> FallbackIcons =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        public static Texture2D GetPackageIcon(string packageKey)
        {
            IconDefinition definition;
            if (!string.IsNullOrWhiteSpace(packageKey) && PackageIcons.TryGetValue(packageKey.Trim(), out definition))
            {
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
            public IconDefinition(string label, params string[] iconNames)
            {
                Label = label;
                IconNames = iconNames ?? Array.Empty<string>();
            }

            public string Label { get; private set; }

            public string[] IconNames { get; private set; }
        }
    }
}
