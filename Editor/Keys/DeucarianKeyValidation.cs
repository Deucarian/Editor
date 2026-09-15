using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Shared serialized-value traversal. Each domain drawer supplies its definition membership rules.</summary>
    public static class DeucarianKeyValidation
    {
        public static IReadOnlyList<DeucarianKeyDrawer> Drawers()
        {
            var result = new List<DeucarianKeyDrawer>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<DeucarianKeyDrawer>())
                if (type.IsVisible && !type.IsAbstract && !type.ContainsGenericParameters && type.GetConstructor(Type.EmptyTypes) != null)
                    result.Add((DeucarianKeyDrawer)Activator.CreateInstance(type));
            return result;
        }

        public static IReadOnlyList<string> Validate(UnityEngine.Object target, IReadOnlyList<DeucarianKeyDrawer> drawers = null)
        {
            var errors = new List<string>();
            if (target == null) return errors;
            drawers = drawers ?? Drawers();
            using (var serialized = new SerializedObject(target))
            {
                var property = serialized.GetIterator();
                bool descend = true;
                while (property.NextVisible(descend))
                {
                    descend = true;
                    if (property.propertyType != SerializedPropertyType.Generic) continue;
                    Type fieldType = DeucarianKeyPropertyType.Resolve(property);
                    if (fieldType == null) continue;
                    foreach (var drawer in drawers)
                    {
                        bool matches = drawer.KeyType.IsGenericTypeDefinition
                            ? fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == drawer.KeyType
                            : drawer.KeyType.IsAssignableFrom(fieldType);
                        if (!matches) continue;
                        try
                        {
                            string error = drawer.Validate(property, drawer.ReadChoices(fieldType));
                            if (error != null) errors.Add(target.name + " (" + target.GetType().Name + "): " + error);
                        }
                        catch (Exception error) { errors.Add(target.name + ": " + error.GetBaseException().Message); }
                        descend = false;
                        break;
                    }
                }
            }
            return errors;
        }
    }
}
