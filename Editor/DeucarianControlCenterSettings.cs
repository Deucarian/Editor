using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterSettings
    {
        internal const string SettingsPath =
            "Project/Deucarian/Control Center";

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(
                SettingsPath,
                SettingsScope.Project)
            {
                label = "Control Center",
                guiHandler = _ => Draw()
            };
        }

        private static void Draw()
        {
            DeucarianEditorChrome.DrawSectionHeader("Editor appearance");
            DeucarianEditorAppearance.DecorativeBackgrounds = DeucarianEditorInputGUI.Toggle(
                "Decorative backgrounds", DeucarianEditorAppearance.DecorativeBackgrounds);
            using (new EditorGUI.DisabledScope(!DeucarianEditorAppearance.DecorativeBackgrounds))
            {
                var previous = DeucarianEditorAmbientMotionSettings.CurrentMode;
                var next = (DeucarianEditorAmbientMotionMode)DeucarianEditorInputGUI.EnumPopup("Background motion", previous);
                if (next != previous) DeucarianEditorAmbientMotionSettings.SetMode(next);
            }
            DeucarianEditorTextGUI.LabelField("Applies to Deucarian tools in this project.", DeucarianEditorWorkbenchGUI.WordWrappedMiniLabelStyle);
            EditorGUILayout.Space();
            IReadOnlyList<DeucarianProjectIssue> issues =
                DeucarianProjectValidationRegistry.Evaluate();
            int blockers = 0;
            foreach (DeucarianProjectIssue issue in issues)
            {
                if (issue.IsBlocking)
                {
                    blockers++;
                }
            }

            DeucarianEditorTextGUI.HelpBox(
                blockers == 0
                    ? "All contributed project checks pass."
                    : blockers + " blocking project issue(s) remain.",
                blockers == 0 ? MessageType.Info : MessageType.Error);
            if (DeucarianEditorActionGUI.Button("Open Deucarian Control Center"))
            {
                DeucarianControlCenterWindow.Open(
                    DeucarianControlCenterArea.Project);
            }
        }
    }
}
