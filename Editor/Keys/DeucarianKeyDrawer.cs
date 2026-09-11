using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Domains opt into the shared picker with a small custom property drawer.</summary>
    public abstract class DeucarianKeyDrawer : PropertyDrawer
    {
        public abstract Type KeyType { get; }
        public abstract Type DefinitionSetAttribute { get; }
        public abstract string SetupHint { get; }

        public virtual IReadOnlyList<DeucarianKeyChoice> ReadChoices(Type concreteKeyType) =>
            DeucarianKeyChoices.Read(concreteKeyType, DefinitionSetAttribute);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                Type type = DeucarianKeyPropertyType.Resolve(property) ?? KeyType;
                string generationError = DeucarianKeyGeneration.FindError(type);
                if (generationError != null) throw new InvalidOperationException(generationError);
                var choices = ReadChoices(type);
                var row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                DeucarianKeyPicker.Draw(row, property, label, choices, SetupHint);
                string error = Validate(property, choices);
                if (error != null)
                {
                    row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    row.height = EditorGUIUtility.singleLineHeight * 2;
                    EditorGUI.HelpBox(row, error, MessageType.Error);
                }
            }
            catch (Exception error) { EditorGUI.HelpBox(position, error.GetBaseException().Message, MessageType.Error); }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                Type type = DeucarianKeyPropertyType.Resolve(property) ?? KeyType;
                if (DeucarianKeyGeneration.FindError(type) != null) return EditorGUIUtility.singleLineHeight * 3;
                if (Validate(property, ReadChoices(type)) == null) return EditorGUIUtility.singleLineHeight;
            }
            catch { /* OnGUI renders the definition error with its repair instructions. */ }
            return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;
        }

        public string Validate(SerializedProperty property, IReadOnlyList<DeucarianKeyChoice> choices) =>
            DeucarianKeyPicker.Validate(property.FindPropertyRelative("definitionId")?.stringValue,
                choices, property.propertyPath, SetupHint);
    }
}
