using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Loads the package-owned Lucide catalog and legacy package icon aliases.</summary>
    public static class DeucarianEditorIcons
    {
        private const string LucideIconRoot = DeucarianEditorUIResources.IconsPath + "/Lucide";

        private static readonly Dictionary<string, string> LegacyPackageIconAliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "package-installer", DeucarianEditorIconIds.CreatePackage },
                { "theming", DeucarianEditorIconIds.Palette },
                { "diagnostics", DeucarianEditorIconIds.Info },
                { "logging", DeucarianEditorIconIds.Logging },
                { "object-loading", DeucarianEditorIconIds.OpenFolder },
                { "api-helper", "braces" },
                { "session", "key-round" },
                { "selection", "mouse-pointer-click" },
                { "generic-ui-items", "component" },
                { "editor", DeucarianEditorIconIds.Wrench }
            };

        private static readonly Dictionary<string, Texture2D> SharedIcons =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Loads a vendored Lucide icon. Unsafe, unknown, and missing IDs resolve to the
        /// canonical Lucide package icon; Unity built-in icon names are never consulted.
        /// </summary>
        public static Texture2D GetIcon(string iconId)
        {
            return TryGetVendoredIcon(iconId, out Texture2D icon)
                ? icon
                : GetCanonicalPackageIcon();
        }

        /// <summary>Creates IMGUI content using a shared icon and visible text.</summary>
        public static GUIContent GetIconContent(string iconId, string text, string tooltip = null)
        {
            return new GUIContent(text ?? string.Empty, GetIcon(iconId), tooltip ?? string.Empty);
        }

        /// <summary>Returns true only for a safe slug whose PNG is actually vendored by this package.</summary>
        public static bool IsKnownIconId(string iconId)
        {
            return TryGetVendoredIcon(iconId, out _);
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

        /// <summary>
        /// Resolves a direct Lucide ID first, then the legacy package-key aliases retained
        /// for source compatibility. Unknown values use the canonical package glyph.
        /// </summary>
        public static Texture2D GetPackageIcon(string packageKey)
        {
            if (TryGetVendoredIcon(packageKey, out Texture2D directIcon))
            {
                return directIcon;
            }

            if (!string.IsNullOrWhiteSpace(packageKey) &&
                LegacyPackageIconAliases.TryGetValue(packageKey.Trim(), out string iconId) &&
                TryGetVendoredIcon(iconId, out Texture2D aliasIcon))
            {
                return aliasIcon;
            }

            return GetCanonicalPackageIcon();
        }

        /// <summary>Returns the canonical Lucide package glyph for unknown legacy content.</summary>
        public static Texture2D GetFallbackIcon(string label)
        {
            return GetCanonicalPackageIcon();
        }

        public static GUIContent GetPackageContent(string packageKey, string title, string tooltip = null)
        {
            return new GUIContent(title ?? string.Empty, GetPackageIcon(packageKey), tooltip ?? string.Empty);
        }

        public static bool IsKnownPackageKey(string packageKey)
        {
            return TryGetVendoredIcon(packageKey, out _) ||
                   (!string.IsNullOrWhiteSpace(packageKey) &&
                    LegacyPackageIconAliases.ContainsKey(packageKey.Trim()));
        }

        private static bool TryGetVendoredIcon(string iconId, out Texture2D icon)
        {
            icon = null;

            if (!TryNormalizeIconId(iconId, out string normalizedId))
            {
                return false;
            }

            if (SharedIcons.TryGetValue(normalizedId, out Texture2D cached) && cached != null)
            {
                icon = cached;
                return true;
            }

            Texture2D loaded = DeucarianEditorUIResources.LoadTexture(GetLucideAssetPath(normalizedId));
            if (loaded == null)
            {
                return false;
            }

            SharedIcons[normalizedId] = loaded;
            icon = loaded;
            return true;
        }

        private static Texture2D GetCanonicalPackageIcon()
        {
            if (TryGetVendoredIcon(DeucarianEditorIconIds.Package, out Texture2D packageIcon))
            {
                return packageIcon;
            }

            // The package icon is a required package asset. Returning null here makes a
            // damaged package visible without silently changing to a Unity built-in glyph.
            return null;
        }

        private static string GetLucideAssetPath(string normalizedId)
        {
            return LucideIconRoot + "/" + normalizedId + ".png";
        }

        private static bool TryNormalizeIconId(string iconId, out string normalizedId)
        {
            normalizedId = string.Empty;
            if (string.IsNullOrWhiteSpace(iconId))
            {
                return false;
            }

            string candidate = iconId.Trim();
            if (candidate.Length == 0 || candidate.Length > 96 || candidate[0] == '-' || candidate[candidate.Length - 1] == '-')
            {
                return false;
            }

            bool previousWasHyphen = false;
            for (int index = 0; index < candidate.Length; index++)
            {
                char character = candidate[index];
                bool alphaNumeric = character >= 'a' && character <= 'z' ||
                                    character >= '0' && character <= '9';
                if (alphaNumeric)
                {
                    previousWasHyphen = false;
                    continue;
                }

                if (character != '-' || previousWasHyphen)
                {
                    return false;
                }

                previousWasHyphen = true;
            }

            normalizedId = candidate;
            return true;
        }

    }
}
