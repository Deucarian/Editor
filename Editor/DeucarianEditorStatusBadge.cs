using System;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public enum DeucarianEditorStatus
    {
        Info,
        Success,
        Warning,
        Error,
        Disabled
    }

    public static class DeucarianEditorStatusBadge
    {
        public static void Draw(string label, DeucarianEditorStatus status, params GUILayoutOption[] options)
        {
            Draw(new GUIContent(label ?? string.Empty), status, options);
        }

        public static void Draw(GUIContent content, DeucarianEditorStatus status, params GUILayoutOption[] options)
        {
            GUIStyle style = CreateStyle(status);
            EditorGUILayout.LabelField(content ?? GetContent(null, status), style, options);
        }

        public static void Draw(Rect rect, string label, DeucarianEditorStatus status, GUIStyle style = null)
        {
            Draw(rect, new GUIContent(label ?? string.Empty), status, style);
        }

        public static void Draw(Rect rect, GUIContent content, DeucarianEditorStatus status, GUIStyle style = null)
        {
            GUIStyle resolvedStyle = style ?? CreateStyle(status);
            if (resolvedStyle.normal.background == null)
            {
                resolvedStyle = new GUIStyle(resolvedStyle);
                resolvedStyle.normal.background = DeucarianEditorTextureCache.Get("badge-" + status, GetColor(status));
                resolvedStyle.normal.textColor = DeucarianEditorColors.BadgeText;
            }

            GUI.Label(rect, content ?? GetContent(null, status), resolvedStyle);
        }

        public static GUIContent GetContent(string label, DeucarianEditorStatus status)
        {
            return new GUIContent(label ?? GetDefaultLabel(status));
        }

        public static Color GetColor(DeucarianEditorStatus status)
        {
            return DeucarianEditorColors.GetStatusColor(status);
        }

        public static bool IsValid(DeucarianEditorStatus status)
        {
            return Enum.IsDefined(typeof(DeucarianEditorStatus), status);
        }

        public static GUIStyle CreateStyle(DeucarianEditorStatus status)
        {
            GUIStyle style = new GUIStyle(DeucarianEditorStyles.StatusBadge);
            style.normal.background = DeucarianEditorTextureCache.Get("badge-" + status, GetColor(status));
            style.normal.textColor = DeucarianEditorColors.BadgeText;
            return style;
        }

        private static string GetDefaultLabel(DeucarianEditorStatus status)
        {
            switch (status)
            {
                case DeucarianEditorStatus.Success:
                    return "Success";
                case DeucarianEditorStatus.Warning:
                    return "Warning";
                case DeucarianEditorStatus.Error:
                    return "Error";
                case DeucarianEditorStatus.Disabled:
                    return "Disabled";
                default:
                    return "Info";
            }
        }
    }
}
