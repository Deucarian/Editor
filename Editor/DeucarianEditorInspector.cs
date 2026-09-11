using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Composes shared presentation around an inspector without replacing native property editing.</summary>
    public static class DeucarianEditorInspector
    {
        public static PropertyField Property(VisualElement root, SerializedObject serialized, string path, string label = null)
        {
            var property = serialized.FindProperty(path) ?? throw new ArgumentException("Unknown serialized field: " + path, nameof(path));
            var field = new PropertyField(property, label ?? property.displayName) { name = path };
            field.AddToClassList("dw-native-property"); root.Add(field); field.Bind(serialized);
            DeucarianEditorNativeControlStyling.Bind(field, serialized);
            return field;
        }

        public static VisualElement Properties(VisualElement root, SerializedObject serialized, params string[] excluded)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (serialized == null) throw new ArgumentNullException(nameof(serialized));
            var fields = new VisualElement { name = "deucarian-inspector-properties" };
            root.Add(fields);
            var property = serialized.GetIterator();
            bool children = true;
            while (property.NextVisible(children))
            {
                children = false;
                if (property.name == "m_Script" || Array.IndexOf(excluded, property.name) >= 0) continue;
                var field = new PropertyField(property.Copy()) { name = property.propertyPath };
                field.AddToClassList("dw-native-property");
                fields.Add(field);
            }
            fields.Bind(serialized);
            DeucarianEditorNativeControlStyling.Bind(fields, serialized);
            return fields;
        }

        public static void Observe(VisualElement root, SerializedObject serialized, Action refresh)
        {
            if (refresh == null) throw new ArgumentNullException(nameof(refresh));
            root.TrackSerializedObjectValue(serialized, _ => refresh());
            refresh();
        }

        public static VisualElement CreateToolkit(string title = null)
        {
            var root = new VisualElement { name = "deucarian-inspector" };
            root.AddToClassList("deucarian-editor");
            root.AddToClassList("deucarian-workspace-host");
            root.AddToClassList("deucarian-workspace");
            root.AddToClassList("dw-inspector");
            root.AddToClassList(DeucarianEditorTheme.CurrentClass);
            DeucarianEditorAppearance.Bind(root);
            DeucarianEditorUIResources.TryAddSharedStyleSheet(root);
            DeucarianEditorUIResources.TryAddStyleSheet(root, DeucarianEditorWorkspace.StyleSheetPath);
            DeucarianEditorUIResources.TryAddStyleSheet(root, DeucarianEditorUIResources.StylesPath + "/DeucarianFeatures.uss");
            root.Add(new DeucarianEditorWorkspaceBackdrop());
            if (!string.IsNullOrWhiteSpace(title))
                root.Add(DeucarianEditorWorkspaceControls.Label(title, "dw-section-title"));
            return root;
        }

        public static VisualElement Create(Action draw)
        {
            if (draw == null) throw new ArgumentNullException(nameof(draw));
            var root = new VisualElement { name = "deucarian-inspector" };
            root.AddToClassList("deucarian-editor");
            root.AddToClassList("deucarian-inspector");
            root.AddToClassList(DeucarianEditorTheme.CurrentClass);
            DeucarianEditorUIResources.TryAddSharedStyleSheet(root);
            root.Add(new IMGUIContainer(draw) { name = "deucarian-inspector-content" });
            return root;
        }
    }
}
