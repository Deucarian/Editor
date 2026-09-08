using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed partial class DeucarianControlCenterView
    {

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
