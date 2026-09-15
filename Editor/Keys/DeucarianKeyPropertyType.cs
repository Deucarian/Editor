using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Deucarian.Editor
{
    /// <summary>Resolves the declared field type, including keys inside nested lists and arrays.</summary>
    public static class DeucarianKeyPropertyType
    {
        public static Type Resolve(SerializedProperty property)
        {
            if (property == null || property.serializedObject.targetObject == null) return null;
            Type current = property.serializedObject.targetObject.GetType();
            string[] parts = property.propertyPath.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "Array" && i + 1 < parts.Length && parts[i + 1].StartsWith("data[", StringComparison.Ordinal))
                {
                    current = current.IsArray ? current.GetElementType() :
                        current.IsGenericType && typeof(IList).IsAssignableFrom(current) ? current.GetGenericArguments()[0] : null;
                    i++;
                }
                else current = Find(current, parts[i])?.FieldType;
                if (current == null) return null;
            }
            return current;
        }

        private static FieldInfo Find(Type type, string name)
        {
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }
    }
}
