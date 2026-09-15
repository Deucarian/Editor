using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Shared typed-key presentation. Domains supply their authoritative choices.</summary>
    public static class DeucarianKeyPicker
    {
        public static void Draw(Rect position, SerializedProperty property, GUIContent label,
            IReadOnlyList<DeucarianKeyChoice> choices, string setupHint)
        {
            SerializedProperty id = property.FindPropertyRelative("definitionId");
            if (id == null)
            {
                EditorGUI.HelpBox(position, "Cannot read " + property.propertyPath + ". " + setupHint, MessageType.Error);
                return;
            }
            var labels = new List<GUIContent> { new GUIContent("Select a definition…") };
            int current = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                labels.Add(new GUIContent(choices[i].Label));
                if (choices[i].Id == id.stringValue) current = i + 1;
            }
            if (current == 0 && !string.IsNullOrEmpty(id.stringValue))
            {
                current = labels.Count;
                labels.Add(new GUIContent("Missing: " + id.stringValue, "The definition was removed. Select an existing definition."));
            }
            EditorGUI.BeginProperty(position, label, property);
            bool previousMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = id.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int next = EditorGUI.Popup(position, label, current, labels.ToArray());
            if (EditorGUI.EndChangeCheck() && next <= choices.Count)
                id.stringValue = next == 0 ? string.Empty : choices[next - 1].Id;
            EditorGUI.showMixedValue = previousMixed;
            EditorGUI.EndProperty();
        }

        public static string Validate(string id, IReadOnlyList<DeucarianKeyChoice> choices, string location, string setupHint)
        {
            foreach (var choice in choices)
                if (string.Equals(choice.Id, id, StringComparison.Ordinal)) return null;
            return location + (string.IsNullOrEmpty(id) ? " has no selected definition. " : " references missing definition '" + id + "'. ") + setupHint;
        }
    }
}
