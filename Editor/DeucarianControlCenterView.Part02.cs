using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed partial class DeucarianControlCenterView
    {


        private static Label CreateMutedLabel(string text)
        {
            Label label = CreateLabel(text, false);
            label.style.color = DeucarianEditorTheme.MutedText;
            label.style.marginBottom = 4f;
            return label;
        }

        private static void StylePanel(VisualElement element)
        {
            element.style.backgroundColor = DeucarianEditorTheme.GlassPanelSoft;
            element.style.borderTopWidth = 1f;
            element.style.borderRightWidth = 1f;
            element.style.borderBottomWidth = 1f;
            element.style.borderLeftWidth = 1f;
            SetBorderColor(element, DeucarianEditorTheme.BorderSubtle);
        }

        private static void SetBorderColor(
            VisualElement element,
            Color color)
        {
            element.style.borderTopColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
        }

        private static Color GetStatusColor(
            DeucarianControlCenterStatus status)
        {
            switch (status)
            {
                case DeucarianControlCenterStatus.Success:
                    return DeucarianEditorTheme.Success;
                case DeucarianControlCenterStatus.Warning:
                    return DeucarianEditorTheme.Warning;
                case DeucarianControlCenterStatus.Error:
                    return DeucarianEditorTheme.Error;
                default:
                    return DeucarianEditorTheme.Text;
            }
        }

        private static bool Confirm(string title, bool required)
        {
            return !required || EditorUtility.DisplayDialog(
                "Confirm Control Center action",
                "Run '" + title + "'?",
                "Run",
                "Cancel");
        }

        private void InvokeSafely(string title, Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                DeucarianEditorActionErrors.Show(title + " could not complete", exception);
                return;
            }

            try
            {
                refresh();
            }
            catch (Exception exception)
            {
                DeucarianEditorActionErrors.Show("Status refresh failed", exception, actionCompleted: true);
            }
        }

        private static string GetAreaDescription(
            DeucarianControlCenterArea area)
        {
            switch (area)
            {
                case DeucarianControlCenterArea.Overview:
                    return "A concise view of project readiness and installed capabilities.";
                case DeucarianControlCenterArea.Project:
                    return "Actionable checks reported by their owning packages.";
                case DeucarianControlCenterArea.Developer:
                    return "Discover registered package tools and compatibility shortcuts.";
                default:
                    return "Status and actions contributed by installed packages.";
            }
        }
    }
}
