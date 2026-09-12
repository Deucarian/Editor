using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    /// <summary>A declarative section stored in a referenced ScriptableObject; new sections become subassets.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DefinitionSectionAttribute : Attribute
    {
        public DefinitionSectionAttribute(string path, Type assetType) { Path = path; AssetType = assetType; }
        public string Path { get; }
        public Type AssetType { get; }
    }

    internal static class DeucarianDefinitionSections
    {
        internal static void Read(SerializedObject serialized, object spec)
        {
            foreach (var field in spec.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var mapping = field.GetCustomAttribute<DefinitionFieldAttribute>();
                var section = field.GetCustomAttribute<DefinitionSectionAttribute>();
                if (mapping != null) field.SetValue(spec, DeucarianDefinitionProperties.Read(Required(serialized, mapping.Path), field.FieldType));
                else if (section != null)
                {
                    var asset = Required(serialized, section.Path).objectReferenceValue as ScriptableObject;
                    if (asset == null) { field.SetValue(spec, null); continue; }
                    var value = Activator.CreateInstance(field.FieldType);
                    using (var child = new SerializedObject(asset)) Read(child, value);
                    field.SetValue(spec, value);
                }
            }
        }

        internal static void Write(SerializedObject serialized, object spec)
        {
            foreach (var field in spec.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var mapping = field.GetCustomAttribute<DefinitionFieldAttribute>();
                var section = field.GetCustomAttribute<DefinitionSectionAttribute>();
                if (mapping != null) DeucarianDefinitionProperties.Write(Required(serialized, mapping.Path), field.FieldType, field.GetValue(spec));
                else if (section != null)
                {
                    var property = Required(serialized, section.Path);
                    var value = field.GetValue(spec);
                    if (value == null) { property.objectReferenceValue = null; continue; }
                    var asset = property.objectReferenceValue as ScriptableObject;
                    if (asset == null)
                    {
                        if (!AssetDatabase.Contains(serialized.targetObject)) throw new InvalidOperationException("Save the definition asset before creating its sections.");
                        asset = ScriptableObject.CreateInstance(section.AssetType);
                        asset.name = field.Name;
                        AssetDatabase.AddObjectToAsset(asset, serialized.targetObject);
                        property.objectReferenceValue = asset;
                    }
                    if (!section.AssetType.IsInstanceOfType(asset)) throw new InvalidOperationException("Choose a " + section.AssetType.Name + " for " + field.Name + ".");
                    using (var child = new SerializedObject(asset))
                    {
                        Write(child, value);
                        if (child.ApplyModifiedPropertiesWithoutUndo()) EditorUtility.SetDirty(asset);
                    }
                }
            }
        }
        private static SerializedProperty Required(SerializedObject value, string path) => value.FindProperty(path) ?? throw new InvalidOperationException("Definition schema has an unknown serialized field: " + path);
    }
}
