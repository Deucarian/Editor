using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Native Undo/prefab-aware bindings inside the shared field layout.</summary>
    public sealed class DeucarianEditorSerializedForm : IDisposable
    {
        private readonly SerializedObject serialized;
        private bool disposed;
        private readonly bool editable;
        public VisualElement Root { get; }

        public DeucarianEditorSerializedForm(VisualElement root, UnityEngine.Object target)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            serialized = new SerializedObject(target != null ? target : throw new ArgumentNullException(nameof(target)));
            editable = !DeucarianEditorAssetCatalog.IsPackageAsset(target);
        }

        public VisualElement Property(string path, string label = null)
        {
            var property = Find(path);
            VisualElement input;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean: input = Bind(new DeucarianEditorSwitch(), property); break;
                case SerializedPropertyType.Float: input = Bind(new FloatField(), property); break;
                case SerializedPropertyType.Integer: input = Bind(new IntegerField(), property); break;
                case SerializedPropertyType.String: input = Bind(new TextField(), property); break;
                case SerializedPropertyType.Color: input = Bind(new DeucarianEditorColorField(), property); break;
                case SerializedPropertyType.Vector2: input = Bind(new Vector2Field(), property); break;
                case SerializedPropertyType.Vector3: input = Bind(new Vector3Field(), property); break;
                default:
                    var native = new PropertyField(property, label ?? property.displayName) { name = path };
                    native.AddToClassList("dw-native-property");
                    native.Bind(serialized);
                    native.SetEnabled(editable);
                    Root.Add(native);
                    return native;
            }
            var row = DeucarianEditorWorkspaceControls.Field(label ?? property.displayName, input);
            if (input is Toggle) row.AddToClassList("dw-switch-field");
            Root.Add(row);
            return input;
        }

        public Slider Slider(string path, string label, float minimum, float maximum)
        {
            var input = Bind(new DeucarianEditorSlider(minimum, maximum) { showInputField = true }, Find(path));
            Root.Add(DeucarianEditorWorkspaceControls.Field(label, input));
            return input;
        }

        public ObjectField Asset(string path, string label, Type objectType, bool sceneObjects = false)
        {
            var input = Bind(new ObjectField { objectType = objectType, allowSceneObjects = sceneObjects }, Find(path));
            Root.Add(DeucarianEditorWorkspaceControls.Field(label, input));
            return input;
        }

        public void Remaining(params string[] excluded)
        {
            var properties = serialized.GetIterator();
            bool children = true;
            while (properties.NextVisible(children))
            {
                children = false;
                if (properties.name == "m_Script" || Array.IndexOf(excluded, properties.name) >= 0) continue;
                Property(properties.propertyPath);
            }
        }

        private SerializedProperty Find(string path)
        {
            if (disposed) throw new ObjectDisposedException(nameof(DeucarianEditorSerializedForm));
            return serialized.FindProperty(path) ?? throw new ArgumentException("Unknown serialized field: " + path, nameof(path));
        }

        private T Bind<T>(T field, SerializedProperty property) where T : VisualElement, IBindable
        {
            field.name = property.propertyPath;
            field.tooltip = property.tooltip;
            field.BindProperty(property);
            field.SetEnabled(editable);
            return field;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Root.Unbind();
            serialized.Dispose();
        }
    }
}
