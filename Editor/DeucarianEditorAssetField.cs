using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.Editor
{
    /// <summary>Shared asset selection UI. The owner supplies defaults and creation policy.</summary>
    public sealed class DeucarianEditorAssetField
    {
        private readonly Func<Object> read;
        private readonly Action<Object> write;
        private readonly Func<Object> defaultValue;
        private readonly DeucarianEditorAssetCatalog catalog;
        private readonly Label origin;
        private readonly Button customize, useDefault;
        public VisualElement Root { get; }
        public ObjectField Input { get; }

        public DeucarianEditorAssetField(string id, Type type, Func<Object> read, Action<Object> write,
            Func<Object> create = null, Func<Object, Object> customize = null, Func<Object> defaultValue = null,
            bool allowSceneObjects = false, bool allowSelection = true)
        {
            this.read = read ?? throw new ArgumentNullException(nameof(read));
            this.write = write ?? throw new ArgumentNullException(nameof(write));
            this.defaultValue = defaultValue;
            catalog = new DeucarianEditorAssetCatalog(type);
            Root = DeucarianEditorWorkspaceControls.Region(id + "-asset-control", "dw-asset-control");
            Input = new ObjectField { name = id, objectType = type, allowSceneObjects = allowSceneObjects };
            Input.AddToClassList("dw-single-line");
            Input.SetEnabled(allowSelection);
            Input.RegisterValueChangedCallback(evt => { write(evt.newValue); Refresh(); });
            var row = DeucarianEditorWorkspaceControls.Region(null, "dw-asset-input-row");
            row.Add(Input);
            var choose = DeucarianEditorWorkspaceControls.Button("Choose…", Choose);
            choose.name = id + "-choose";
            if (allowSelection) row.Add(choose);
            Root.Add(row);
            var actions = DeucarianEditorWorkspaceControls.Actions();
            origin = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-asset-origin");
            actions.Add(origin);
            if (create != null) actions.Add(Action(id + "-create", "Create…", () => SelectCreated(create())));
            if (customize != null)
            {
                this.customize = Action(id + "-customize", "Customize…", () => SelectCreated(customize(read())));
                actions.Add(this.customize);
            }
            if (defaultValue != null)
            {
                useDefault = Action(id + "-default", "Use default", () => SelectCreated(defaultValue()));
                actions.Add(useDefault);
            }
            Root.Add(actions);
            Root.RegisterCallback<AttachToPanelEvent>(_ => { catalog.Invalidate(); EditorApplication.projectChanged -= OnProjectChanged; EditorApplication.projectChanged += OnProjectChanged; });
            Root.RegisterCallback<DetachFromPanelEvent>(_ => EditorApplication.projectChanged -= OnProjectChanged);
            Refresh();
        }

        private static Button Action(string id, string label, System.Action execute)
        {
            var result = DeucarianEditorWorkspaceControls.Button(label, () =>
            {
                try { execute(); }
                catch (Exception exception) { DeucarianEditorActionErrors.Show(label, exception); }
            }, DeucarianEditorButtonRole.Quiet);
            result.name = id; return result;
        }

        private void SelectCreated(Object value)
        {
            if (value == null) return;
            write(value); catalog.Invalidate(); Refresh();
        }

        private void OnProjectChanged() { catalog.Invalidate(); Refresh(); }

        public void Refresh()
        {
            Object value = read();
            Input.SetValueWithoutNotify(value);
            Object fallback = defaultValue?.Invoke();
            origin.text = DeucarianEditorAssetCatalog.Origin(value) + (value != null && value == fallback ? " · default" : "");
            origin.tooltip = value != null ? AssetDatabase.GetAssetPath(value) : "Choose from project and installed package assets.";
            customize?.SetEnabled(value != null);
            useDefault?.SetEnabled(fallback != null && fallback != value);
        }

        private void Choose()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), read() == null, () => { write(null); Refresh(); });
            menu.AddSeparator("");
            foreach (Object asset in catalog.Find())
            {
                Object candidate = asset;
                string path = AssetDatabase.GetAssetPath(asset);
                menu.AddItem(new GUIContent(path + (AssetDatabase.IsSubAsset(asset) ? "/" + asset.name : "")),
                    read() == candidate, () => { write(candidate); Refresh(); });
            }
            if (catalog.Find().Count == 0) menu.AddDisabledItem(new GUIContent("No matching project or package assets"));
            menu.ShowAsContext();
        }
    }
}
