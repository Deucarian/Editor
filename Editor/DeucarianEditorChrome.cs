using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorChrome
    {
        public static void DrawPackageHeader(string title, string subtitle, Texture2D icon = null)
        {
            DrawPackageHeader(title, subtitle, icon, 24f);
        }

        public static void DrawPackageHeader(
            string title,
            string subtitle,
            Texture2D icon,
            float iconSize)
        {
            Rect headerRect = EditorGUILayout.BeginVertical(DeucarianEditorStyles.PackageHeaderBox);
            DeucarianEditorWorkbenchGUI.DrawSurface(
                headerRect,
                Opaque(DeucarianEditorWorkbenchGUI.HeaderPanelBackgroundColor),
                DeucarianEditorWorkbenchGUI.PanelBorderColor);
            EditorGUILayout.BeginHorizontal();

            if (icon != null)
            {
                float safeIconSize = Mathf.Max(1f, iconSize);
                Rect iconRect = GUILayoutUtility.GetRect(
                    safeIconSize,
                    safeIconSize,
                    GUILayout.Width(safeIconSize),
                    GUILayout.Height(safeIconSize));
                DeucarianEditorIcons.DrawIcon(iconRect, icon, DeucarianEditorColors.TitleText);
                GUILayout.Space(8);
            }

            EditorGUILayout.BeginVertical();
            DrawBrandedLabel(
                title ?? string.Empty,
                DeucarianEditorStyles.PackageHeaderTitle,
                DeucarianEditorWorkbenchGUI.TextColor);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                DrawBrandedLabel(
                    subtitle,
                    DeucarianEditorStyles.PackageHeaderSubtitle,
                    DeucarianEditorWorkbenchGUI.MutedTextColor);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public static void DrawPackageHeader(string packageKey, string title, string subtitle)
        {
            DrawPackageHeader(title, subtitle, DeucarianEditorIcons.GetPackageIcon(packageKey));
        }

        public static void DrawPackageHeader(
            string packageKey,
            string title,
            string subtitle,
            float iconSize)
        {
            DrawPackageHeader(
                title,
                subtitle,
                DeucarianEditorIcons.GetPackageIcon(packageKey),
                iconSize);
        }

        public static void DrawSectionHeader(string title)
        {
            DrawBrandedLabel(
                title ?? string.Empty,
                DeucarianEditorStyles.SectionTitle,
                DeucarianEditorWorkbenchGUI.TextColor);
        }

        public static void BeginSection()
        {
            Rect sectionRect = EditorGUILayout.BeginVertical(DeucarianEditorStyles.SectionBox);
            DeucarianEditorWorkbenchGUI.DrawSurface(
                sectionRect,
                Opaque(DeucarianEditorWorkbenchGUI.PanelBackgroundColor),
                DeucarianEditorWorkbenchGUI.PanelBorderColor);
        }

        public static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        public static void DrawFooterVersion(string packageName, string version)
        {
            string packageLabel = string.IsNullOrWhiteSpace(packageName) ? DeucarianEditorPackageConstants.DisplayName : packageName.Trim();
            string versionLabel = string.IsNullOrWhiteSpace(version) ? "unknown" : version.Trim();
            DrawBrandedLabel(
                packageLabel + " " + versionLabel,
                DeucarianEditorStyles.FooterVersionText,
                DeucarianEditorWorkbenchGUI.MutedTextColor);
        }

        public static void DrawInlineHelp(string message, MessageType type)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            EditorGUILayout.HelpBox(message, type);
        }

        private static void DrawBrandedLabel(string text, GUIStyle sourceStyle, Color textColor)
        {
            var style = new GUIStyle(sourceStyle ?? GUIStyle.none);
            SetTextColor(style.normal, textColor);
            SetTextColor(style.onNormal, textColor);
            SetTextColor(style.hover, textColor);
            SetTextColor(style.onHover, textColor);
            SetTextColor(style.focused, textColor);
            SetTextColor(style.onFocused, textColor);
            SetTextColor(style.active, textColor);
            SetTextColor(style.onActive, textColor);

            Color previousContentColor = GUI.contentColor;
            try
            {
                GUI.contentColor = Color.white;
                EditorGUILayout.LabelField(text ?? string.Empty, style);
            }
            finally
            {
                GUI.contentColor = previousContentColor;
            }
        }

        private static void SetTextColor(GUIStyleState state, Color color)
        {
            if (state != null)
            {
                state.textColor = color;
            }
        }

        private static Color Opaque(Color color)
        {
            color.a = 1f;
            return color;
        }
    }
}
