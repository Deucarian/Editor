using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;

namespace Deucarian.Editor
{
    /// <summary>Editor-only discovery of explicitly marked, compiled definition sets.</summary>
    public static class DeucarianKeyChoices
    {
        public static IReadOnlyList<DeucarianKeyChoice> Read(Type keyType, Type definitionSetAttribute, bool includeEditorDefinitions = false)
        {
            if (keyType == null) throw new ArgumentNullException(nameof(keyType));
            if (definitionSetAttribute == null) throw new ArgumentNullException(nameof(definitionSetAttribute));
            var choices = new List<DeucarianKeyChoice>();
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);
            var playerAssemblies = new HashSet<string>(StringComparer.Ordinal);
            if (!includeEditorDefinitions)
                foreach (var assembly in CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies))
                    playerAssemblies.Add(assembly.name);
            foreach (Type set in TypeCache.GetTypesWithAttribute(definitionSetAttribute))
            {
                if (!set.IsVisible || set.ContainsGenericParameters) continue;
                if (!includeEditorDefinitions && !playerAssemblies.Contains(set.Assembly.GetName().Name)) continue;
                foreach (FieldInfo field in set.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    if (keyType.IsAssignableFrom(field.FieldType))
                        Add(field.GetValue(null), set, field.Name, keyType, choices, seen);
                foreach (PropertyInfo property in set.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    if (keyType.IsAssignableFrom(property.PropertyType) && property.CanRead && property.GetIndexParameters().Length == 0)
                        Add(property.GetValue(null), set, property.Name, keyType, choices, seen);
            }
            choices.Sort((left, right) => string.Compare(left.Label, right.Label, StringComparison.Ordinal));
            return choices;
        }

        private static void Add(object value, Type set, string name, Type keyType,
            List<DeucarianKeyChoice> choices, Dictionary<string, string> seen)
        {
            string label = set.Name + "/" + name;
            if (value == null) throw new InvalidOperationException(label + " is null. Initialize this definition before using it in the Inspector.");
            var idProperty = keyType.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
            if (idProperty == null || idProperty.PropertyType != typeof(string))
                throw new InvalidOperationException(keyType.FullName + " must expose a string Id for its Inspector picker.");
            string id = (string)idProperty.GetValue(value);
            if (seen.TryGetValue(id, out string previous))
                throw new InvalidOperationException(label + " duplicates '" + id + "' from " + previous + ". Keep one definition and reuse its typed key.");
            seen.Add(id, label);
            choices.Add(new DeucarianKeyChoice(id, label));
        }
    }
}
