using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    internal static class DeucarianDefinitionProperties
    {
        internal static object Read(SerializedProperty property, Type type)
        {
            if (type == typeof(string)) return property.stringValue;
            if (type == typeof(bool)) return property.boolValue;
            if (type == typeof(float)) return property.floatValue;
            if (type == typeof(double)) return property.doubleValue;
            if (type == typeof(int)) return property.intValue;
            if (type == typeof(long)) return property.longValue;
            if (type.IsEnum) return Enum.ToObject(type, property.intValue);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return property.objectReferenceValue;
            if (type == typeof(Color)) return property.colorValue;
            if (type == typeof(Vector2)) return property.vector2Value;
            if (type == typeof(Vector3)) return property.vector3Value;
            if (type == typeof(Vector4)) return property.vector4Value;
            if (type == typeof(Quaternion)) return property.quaternionValue;
            if (type.IsArray)
            {
                var element = type.GetElementType();
                var array = Array.CreateInstance(element, property.arraySize);
                for (int i = 0; i < array.Length; i++) array.SetValue(Read(property.GetArrayElementAtIndex(i), element), i);
                return array;
            }
            var value = Activator.CreateInstance(type);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string name = field.GetCustomAttribute<DefinitionFieldAttribute>()?.Path ?? field.Name;
                var child = property.FindPropertyRelative(name) ?? throw new ArgumentException("Unknown definition field: " + name);
                field.SetValue(value, Read(child, field.FieldType));
            }
            return value;
        }

        internal static void Write(SerializedProperty property, Type type, object value)
        {
            if (type == typeof(string)) property.stringValue = (string)value ?? string.Empty;
            else if (type == typeof(bool)) property.boolValue = (bool)value;
            else if (type == typeof(float)) property.floatValue = (float)value;
            else if (type == typeof(double)) property.doubleValue = (double)value;
            else if (type == typeof(int)) property.intValue = (int)value;
            else if (type == typeof(long)) property.longValue = (long)value;
            else if (type.IsEnum) property.intValue = Convert.ToInt32(value);
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type)) property.objectReferenceValue = (UnityEngine.Object)value;
            else if (type == typeof(Color)) property.colorValue = (Color)value;
            else if (type == typeof(Vector2)) property.vector2Value = (Vector2)value;
            else if (type == typeof(Vector3)) property.vector3Value = (Vector3)value;
            else if (type == typeof(Vector4)) property.vector4Value = (Vector4)value;
            else if (type == typeof(Quaternion)) property.quaternionValue = (Quaternion)value;
            else if (type.IsArray)
            {
                var array = (Array)value ?? Array.CreateInstance(type.GetElementType(), 0);
                property.arraySize = array.Length;
                for (int i = 0; i < array.Length; i++) Write(property.GetArrayElementAtIndex(i), type.GetElementType(), array.GetValue(i));
            }
            else
            {
                if (value == null) throw new ArgumentException("Inline definition values cannot be null: " + property.propertyPath);
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    string name = field.GetCustomAttribute<DefinitionFieldAttribute>()?.Path ?? field.Name;
                    var child = property.FindPropertyRelative(name) ?? throw new ArgumentException("Unknown definition field: " + name);
                    Write(child, field.FieldType, field.GetValue(value));
                }
            }
        }
    }
}
