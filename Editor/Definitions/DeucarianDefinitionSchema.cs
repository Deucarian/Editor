using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Editor-only values shared by a definition's source document and asset editor.</summary>
    [Serializable]
    public abstract class DeucarianDefinitionSpec
    {
        public string Id;
        public string Name;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DefinitionFieldAttribute : Attribute
    {
        public DefinitionFieldAttribute(string path) { Path = path; }
        public string Path { get; }
    }

    /// <summary>Domain-owned mapping. Only editor code discovers these adapters.</summary>
    public abstract class DeucarianDefinitionSchema
    {
        public abstract string Id { get; }
        public abstract string DisplayName { get; }
        public abstract Type AssetType { get; }
        public abstract Type SpecType { get; }
        public virtual string IdentityPropertyPath => "id";
        public abstract DeucarianDefinitionSpec Read(ScriptableObject asset);
        public abstract void Apply(ScriptableObject asset, DeucarianDefinitionSpec spec);
        public abstract void Validate(DeucarianDefinitionSpec spec);
        public virtual void ValidateReady(DeucarianDefinitionSpec spec) => Validate(spec);
        public virtual void ValidateAssetReady(ScriptableObject asset) => ValidateReady(Read(asset));
        public virtual void RefreshCatalog(bool validateOnly = false) { }
        public virtual void Preview(ScriptableObject asset) { Selection.activeObject = asset; }
        public virtual bool CanPreview => false;
        public virtual DeucarianDefinitionSpec Create(string name)
        {
            var spec = (DeucarianDefinitionSpec)Activator.CreateInstance(SpecType);
            spec.Id = Guid.NewGuid().ToString("N");
            spec.Name = name;
            return spec;
        }

        public static IReadOnlyList<DeucarianDefinitionSchema> Discover()
        {
            var schemas = new List<DeucarianDefinitionSchema>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var type in TypeCache.GetTypesDerivedFrom<DeucarianDefinitionSchema>())
            {
                if (!type.IsVisible || type.IsAbstract || type.ContainsGenericParameters || type.GetConstructor(Type.EmptyTypes) == null) continue;
                var schema = (DeucarianDefinitionSchema)Activator.CreateInstance(type);
                if (!ids.Add(schema.Id)) throw new InvalidOperationException("Duplicate definition schema: " + schema.Id);
                schemas.Add(schema);
            }
            schemas.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return schemas;
        }
    }

    /// <summary>Maps explicit serialized paths; never evaluates declaration code or player reflection.</summary>
    public abstract class DeucarianSerializedDefinitionSchema<TAsset, TSpec> : DeucarianDefinitionSchema
        where TAsset : ScriptableObject where TSpec : DeucarianDefinitionSpec, new()
    {
        public override Type AssetType => typeof(TAsset);
        public override Type SpecType => typeof(TSpec);
        protected virtual string IdPath => "id";
        protected virtual string NamePath => "displayName";
        public override string IdentityPropertyPath => IdPath;

        public override DeucarianDefinitionSpec Read(ScriptableObject asset)
        {
            var spec = new TSpec();
            using (var serialized = new SerializedObject(asset))
            {
                spec.Id = Required(serialized, IdPath).stringValue;
                spec.Name = Required(serialized, NamePath).stringValue;
                DeucarianDefinitionSections.Read(serialized, spec);
            }
            return spec;
        }

        public override void Apply(ScriptableObject asset, DeucarianDefinitionSpec value)
        {
            Validate(value);
            using (var serialized = new SerializedObject(asset))
            {
                Required(serialized, IdPath).stringValue = value.Id;
                Required(serialized, NamePath).stringValue = value.Name;
                DeucarianDefinitionSections.Write(serialized, value);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(asset);
        }

        public override void Validate(DeucarianDefinitionSpec spec)
        {
            if (!(spec is TSpec)) throw new ArgumentException("Expected " + typeof(TSpec).Name + ".");
            if (string.IsNullOrWhiteSpace(spec.Id) || spec.Id != spec.Id.Trim()) throw new ArgumentException(DisplayName + " needs a stable, non-empty ID.");
            if (string.IsNullOrWhiteSpace(spec.Name)) throw new ArgumentException(DisplayName + " needs a name. Enter the name used by its generated code key.");
        }

        private static SerializedProperty Required(SerializedObject value, string path) => value.FindProperty(path) ??
            throw new InvalidOperationException("Definition schema has an unknown serialized field: " + path);
    }
}
