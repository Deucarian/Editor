using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>
    /// Shared settings-page actions. Packages provide only their domain callback;
    /// labels, icons, tooltips, geometry, and padding remain Editor-owned.
    /// </summary>
    public static class DeucarianEditorSettingsActions
    {
        public const string ResetToDefaultsLabel = "Reset to Defaults";
        public const string ResetToDefaultsTooltip = "Restore the package defaults.";
        public const string ResetActionClass = "deucarian-settings-action--reset";
        public const float ResetButtonWidth = 164f;
        public const float ResetButtonHeight = DeucarianEditorWorkbenchGUI.CompactIconActionHeight;

        public static Button CreateResetToDefaultsButton(
            Action resetAction,
            string tooltip = null,
            bool enabled = true,
            float width = ResetButtonWidth)
        {
            Button button = DeucarianEditorIconTextButton.Create(
                DeucarianEditorIconIds.Reset,
                ResetToDefaultsLabel,
                resetAction,
                string.IsNullOrWhiteSpace(tooltip) ? ResetToDefaultsTooltip : tooltip,
                true);
            button.AddToClassList(ResetActionClass);
            button.style.width = Mathf.Max(0f, width);
            button.style.minWidth = Mathf.Max(0f, width);
            button.style.height = ResetButtonHeight;
            button.style.minHeight = ResetButtonHeight;
            button.SetEnabled(enabled);
            return button;
        }

        public static bool DrawResetToDefaultsButton(
            Action resetAction,
            string tooltip = null,
            bool enabled = true,
            float width = ResetButtonWidth)
        {
            bool clicked = DeucarianEditorWorkbenchGUI.DrawCompactIconAction(
                DeucarianEditorIconIds.Reset,
                ResetToDefaultsLabel,
                string.IsNullOrWhiteSpace(tooltip) ? ResetToDefaultsTooltip : tooltip,
                enabled,
                GUILayout.Width(Mathf.Max(0f, width)));
            if (clicked)
            {
                resetAction?.Invoke();
            }

            return clicked;
        }
    }
}
