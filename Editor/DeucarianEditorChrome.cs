using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorChrome
    {
        public static void DrawPackageHeader(string title, string subtitle, Texture2D icon = null)
        {
            EditorGUILayout.BeginVertical(DeucarianEditorStyles.PackageHeaderBox);
            EditorGUILayout.BeginHorizontal();

            if (icon != null)
            {
                GUILayout.Label(icon, GUILayout.Width(40), GUILayout.Height(40));
                GUILayout.Space(8);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title ?? string.Empty, DeucarianEditorStyles.PackageHeaderTitle);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                EditorGUILayout.LabelField(subtitle, DeucarianEditorStyles.PackageHeaderSubtitle);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public static void DrawPackageHeader(string packageKey, string title, string subtitle)
        {
            DrawPackageHeader(title, subtitle, DeucarianEditorIcons.GetPackageIcon(packageKey));
        }

        public static void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title ?? string.Empty, DeucarianEditorStyles.SectionTitle);
        }

        public static void BeginSection()
        {
            EditorGUILayout.BeginVertical(DeucarianEditorStyles.SectionBox);
        }

        public static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        public static void DrawFooterVersion(string packageName, string version)
        {
            string packageLabel = string.IsNullOrWhiteSpace(packageName) ? DeucarianEditorPackageConstants.DisplayName : packageName.Trim();
            string versionLabel = string.IsNullOrWhiteSpace(version) ? "unknown" : version.Trim();
            EditorGUILayout.LabelField(packageLabel + " " + versionLabel, DeucarianEditorStyles.FooterVersionText);
        }

        public static void DrawInlineHelp(string message, MessageType type)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            EditorGUILayout.HelpBox(message, type);
        }
    }
}
