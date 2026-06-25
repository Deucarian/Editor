using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorStatusPanel
    {
        public static void DrawStatusCard(string message, DeucarianEditorStatus status)
        {
            Rect rect = EditorGUILayout.BeginVertical(GUIStyle.none);
            DeucarianEditorVisualShell.DrawInsetSurface(
                rect,
                GetStatusBackground(status),
                DeucarianEditorColors.GetStatusColor(status),
                7f);
            EditorGUILayout.LabelField(message ?? string.Empty, MessageStyle);
            EditorGUILayout.EndVertical();
            GUILayout.Space(DeucarianEditorSpacing.Small);
        }

        public static void DrawStatusBar(string left, string center, string right)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(DeucarianEditorSpacing.BottomStatusHeight));
            DeucarianEditorVisualShell.DrawInsetSurface(rect, DeucarianEditorTheme.GlassPanel, DeucarianEditorTheme.BorderSubtle, 7f);
            EditorGUILayout.LabelField(left ?? string.Empty, StatusBarStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(center ?? string.Empty, StatusBarCenterStyle, GUILayout.MaxWidth(360f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(right ?? string.Empty, StatusBarRightStyle, GUILayout.MaxWidth(360f));
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawValidationCard(string summary, IEnumerable<string> messages, DeucarianEditorStatus status)
        {
            DrawStatusCard(summary, status);
            if (messages == null)
            {
                return;
            }

            DeucarianEditorCards.DrawInlineCard(() =>
            {
                foreach (string message in messages)
                {
                    EditorGUILayout.LabelField(message ?? string.Empty, MessageStyle);
                }
            });
        }

        private static GUIStyle messageStyle;
        private static GUIStyle statusBarStyle;
        private static GUIStyle statusBarCenterStyle;
        private static GUIStyle statusBarRightStyle;

        private static GUIStyle MessageStyle
        {
            get
            {
                if (messageStyle == null)
                {
                    messageStyle = new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true,
                        padding = new RectOffset(10, 10, 7, 7)
                    };
                    messageStyle.normal.textColor = DeucarianEditorTheme.Text;
                }

                return messageStyle;
            }
        }

        private static GUIStyle StatusBarStyle
        {
            get
            {
                if (statusBarStyle == null)
                {
                    statusBarStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(10, 10, 0, 0)
                    };
                    statusBarStyle.normal.textColor = DeucarianEditorTheme.MutedText;
                }

                return statusBarStyle;
            }
        }

        private static GUIStyle StatusBarCenterStyle
        {
            get
            {
                if (statusBarCenterStyle == null)
                {
                    statusBarCenterStyle = new GUIStyle(StatusBarStyle)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return statusBarCenterStyle;
            }
        }

        private static GUIStyle StatusBarRightStyle
        {
            get
            {
                if (statusBarRightStyle == null)
                {
                    statusBarRightStyle = new GUIStyle(StatusBarStyle)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }

                return statusBarRightStyle;
            }
        }

        private static Color GetStatusBackground(DeucarianEditorStatus status)
        {
            switch (status)
            {
                case DeucarianEditorStatus.Success:
                    return new Color(0.08f, 0.22f, 0.17f, 0.72f);
                case DeucarianEditorStatus.Warning:
                    return new Color(0.24f, 0.18f, 0.08f, 0.72f);
                case DeucarianEditorStatus.Error:
                    return new Color(0.24f, 0.10f, 0.10f, 0.76f);
                default:
                    return DeucarianEditorTheme.GlassPanelSoft;
            }
        }
    }
}
