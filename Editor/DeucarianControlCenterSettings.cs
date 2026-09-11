using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterSettings
    {
        internal const string SettingsPath = "Project/Deucarian/Control Center";

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider() => new SettingsProvider(SettingsPath, SettingsScope.Project)
        {
            label = "Control Center",
            activateHandler = (_, root) =>
            {
                var content = DeucarianEditorInspector.CreateToolkit("Editor appearance");
                Build(content); root.Add(content);
            }
        };

        internal static void Build(VisualElement root)
        {
            var form = new DeucarianEditorWorkspaceForm(root);
            PopupField<string> motion = null;
            form.Toggle("editor-decorative-backgrounds", "Decorative backgrounds",
                () => DeucarianEditorAppearance.DecorativeBackgrounds, value =>
                {
                    DeucarianEditorAppearance.DecorativeBackgrounds = value; motion?.SetEnabled(value);
                });
            var modes = (DeucarianEditorAmbientMotionMode[])Enum.GetValues(typeof(DeucarianEditorAmbientMotionMode));
            motion = form.Choice("editor-background-motion", "Background motion",
                Array.ConvertAll(modes, value => ObjectNames.NicifyVariableName(value.ToString())),
                () => Array.IndexOf(modes, DeucarianEditorAmbientMotionSettings.CurrentMode),
                index => DeucarianEditorAmbientMotionSettings.SetMode(modes[index]));
            motion.SetEnabled(DeucarianEditorAppearance.DecorativeBackgrounds);
            form.Note(() => "Applies to Deucarian tools in this project.");
            form.Action("editor-project-checks", "Review project checks", () =>
                DeucarianEditorNavigation.Open(root, DeucarianToolIds.ControlCenter, "project"));
            root.schedule.Execute(() => { form.Refresh(); motion.SetEnabled(DeucarianEditorAppearance.DecorativeBackgrounds); }).Every(500);
        }
    }
}
