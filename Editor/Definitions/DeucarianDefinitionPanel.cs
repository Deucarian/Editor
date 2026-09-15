using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Reusable definition management embedded inside a package's existing Lab.</summary>
    public sealed class DeucarianDefinitionPanel : IDisposable
    {
        private readonly DeucarianDefinitionSchema schema;
        private readonly VisualElement rows;
        private readonly VisualElement details;
        private readonly Label status;
        private readonly Action<ScriptableObject> preview;
        private readonly DeucarianDefinitionPanelState state;
        private IDisposable editing;
        private ScriptableObject selectedAsset;
        private DeucarianEditorSerializedForm serialized;
        private readonly List<DeucarianEditorSerializedForm> sections = new List<DeucarianEditorSerializedForm>();
        private bool disposed;

        public DeucarianDefinitionPanel(VisualElement root, DeucarianDefinitionSchema schema, Action<ScriptableObject> preview = null, DeucarianDefinitionPanelState state = null)
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.preview = preview;
            this.state = state ?? new DeucarianDefinitionPanelState();
            if (!string.IsNullOrEmpty(this.state.SchemaId) && this.state.SchemaId != schema.Id)
                throw new ArgumentException("Definition panel state belongs to a different schema.", nameof(state));
            this.state.SchemaId = schema.Id;
            var tools = new DeucarianEditorWorkspaceForm(root);
            tools.Text("definition-search", "Search definitions", () => this.state.Search, value => { this.state.Search = value; RefreshList(); });
            tools.Text("definition-name", "New definition name", () => this.state.CreateName, value => this.state.CreateName = value);
            tools.Action("definition-create", "Create definition", () => Run(() => Select(DeucarianDefinitionSync.Create(schema, this.state.CreateName))), () => !string.IsNullOrWhiteSpace(this.state.CreateName), true);
            status = DeucarianEditorWorkspaceControls.Label("Definitions are shared by code calls and Inspector components.", "dw-muted");
            root.Add(status);
            rows = DeucarianEditorWorkspaceControls.Scroll("definition-list");
            details = DeucarianEditorWorkspaceControls.Scroll("definition-details");
            root.Add(DeucarianEditorWorkspaceControls.Split(rows, details));
            EditorApplication.projectChanged += RefreshList;
            RefreshList();
            string selectedGuid = this.state.SelectedAssetGuid;
            Select(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(selectedGuid ?? string.Empty), schema.AssetType) as ScriptableObject);
            var listScroll = (ScrollView)rows;
            var detailScroll = (ScrollView)details;
            Vector2 savedList = this.state.ListScroll, savedDetails = this.state.DetailsScroll;
            root.schedule.Execute(() =>
            {
                if (disposed) return;
                listScroll.scrollOffset = savedList; detailScroll.scrollOffset = savedDetails;
            });
            listScroll.verticalScroller.valueChanged += _ => this.state.ListScroll = listScroll.scrollOffset;
            detailScroll.verticalScroller.valueChanged += _ => this.state.DetailsScroll = detailScroll.scrollOffset;
            listScroll.horizontalScroller.valueChanged += _ => this.state.ListScroll = listScroll.scrollOffset;
            detailScroll.horizontalScroller.valueChanged += _ => this.state.DetailsScroll = detailScroll.scrollOffset;
        }

        public DeucarianDefinitionPanelState CaptureState() => state;

        public void Select(ScriptableObject asset) => Select(asset, false);

        private void Select(ScriptableObject asset, bool rebuild)
        {
            if (!rebuild && asset != null && asset == selectedAsset) return;
            editing?.Dispose(); editing = null;
            selectedAsset = asset;
            state.SelectedAssetGuid = asset == null ? string.Empty : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            serialized?.Dispose(); serialized = null; details.Clear();
            foreach (var section in sections) section.Dispose();
            sections.Clear();
            if (asset == null) return;
            editing = DeucarianDefinitionImport.BeginEditing(schema, asset);
            var actions = new DeucarianEditorWorkspaceForm(details);
            var record = DeucarianDefinitionSync.FindAsset(asset);
            actions.Note(() => record == null
                ? "Edit reusable defaults here. Enable code editing to create a synchronized declaration."
                : "Edit reusable defaults here. Save and synchronize updates their code together when you are ready; changing fields does not interrupt this editor.");
            if (record == null)
                actions.Action("definition-adopt", "Enable code editing", () => Run(() => { DeucarianDefinitionSync.Adopt(schema, asset); Select(asset, true); }));
            else
            {
                actions.Action("definition-code", "Open definition code", () => AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(record.sourceGuid))));
                actions.Action("definition-sync", "Save and synchronize", () => Run(() => DeucarianDefinitionSync.SynchronizeSource(schema, AssetDatabase.GUIDToAssetPath(record.sourceGuid))));
            }
            if (preview != null || schema.CanPreview) actions.Action("definition-preview", "Preview", () => Run(() => { if (preview != null) preview(asset); else schema.Preview(asset); }));
            actions.Action("definition-duplicate", "Duplicate", () => Run(() => Duplicate(asset)));
            if (record != null)
            {
                var advanced = actions.Section("Source and references", true);
                advanced.Note(() => AssetDatabase.GUIDToAssetPath(record.sourceGuid));
                advanced.Action("definition-references", "Find asset references", () => Run(() => FindReferences(asset), false));
                advanced.Action("definition-keep-code", "Resolve conflict: keep code", () => Run(() => { DeucarianDefinitionSync.Resolve(schema, record, true); Select(asset, true); }));
                advanced.Action("definition-keep-asset", "Resolve conflict: keep asset", () => Run(() => { DeucarianDefinitionSync.Resolve(schema, record, false); Select(asset, true); }));
                advanced.Action("definition-delete", "Delete definition", () => Run(() =>
                {
                    if (!EditorUtility.DisplayDialog("Delete " + asset.name + "?", "Move the definition and its editable code to Trash. Existing code calls and serialized selections must be updated.", "Move to Trash", "Cancel")) return;
                    DeucarianDefinitionSync.Delete(schema, record); Select(null); RefreshList();
                }));
            }
            serialized = new DeucarianEditorSerializedForm(details, asset);
            serialized.Remaining(schema.IdentityPropertyPath.Split('.')[0]);
            AddSections(actions, asset, schema.SpecType);
            RefreshList();
        }

        private void AddSections(DeucarianEditorWorkspaceForm form, ScriptableObject asset, Type specType)
        {
            using (var value = new SerializedObject(asset))
                foreach (var field in specType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var mapping = field.GetCustomAttribute<DefinitionSectionAttribute>();
                    if (mapping == null) continue;
                    var child = value.FindProperty(mapping.Path)?.objectReferenceValue as ScriptableObject;
                    if (child == null) continue;
                    var container = form.Section(ObjectNames.NicifyVariableName(field.Name), true);
                    var section = new DeucarianEditorSerializedForm(container.Root, child);
                    sections.Add(section); section.Remaining();
                    AddSections(container, child, field.FieldType);
                }
        }

        private void RefreshList()
        {
            if (disposed) return;
            rows.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:" + schema.AssetType.Name, new[] { "Assets" }).OrderBy(x => AssetDatabase.GUIDToAssetPath(x), StringComparer.Ordinal))
            {
                var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), schema.AssetType) as ScriptableObject;
                if (asset == null) continue;
                string label = schema.Read(asset).Name;
                if (string.IsNullOrEmpty(label)) label = asset.name;
                if (!string.IsNullOrEmpty(state.Search) && label.IndexOf(state.Search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                var row = DeucarianEditorWorkspaceControls.Button(label, () => Select(asset));
                row.EnableInClassList("dw-selected", asset == selectedAsset);
                rows.Add(row);
            }
            foreach (var record in DeucarianDefinitionSync.Records.Where(x => x.schema == schema.Id && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(x.assetGuid)) == null))
                rows.Add(DeucarianEditorWorkspaceControls.Button("Missing asset: " + record.identity, () =>
                {
                    Select(null);
                    var missing = new DeucarianEditorWorkspaceForm(details);
                    missing.Note(() => "Restore this asset and its .meta file from version control, or remove the remaining declaration and catalog entry.");
                    missing.Action("definition-remove-missing", "Remove missing definition", () => Run(() => { DeucarianDefinitionSync.Delete(schema, record); Select(null); RefreshList(); }));
                }));
            var errors = DeucarianDefinitionImport.Errors.Where(x => DeucarianDefinitionImport.BelongsTo(x.Key, schema.Id)).Select(x => x.Value).Distinct().ToArray();
            if (errors.Length > 0) status.text = string.Join("\n", errors);
        }

        private void Duplicate(ScriptableObject asset)
        {
            Select(DeucarianDefinitionSync.Duplicate(schema, asset));
        }
        private void FindReferences(ScriptableObject asset)
        {
            string target = AssetDatabase.GetAssetPath(asset);
            var paths = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith("Assets/", StringComparison.Ordinal) && x != target && !Directory.Exists(x))
                .Where(x => AssetDatabase.GetDependencies(x, false).Contains(target)).ToArray();
            status.text = paths.Length == 0 ? "No direct asset references found. Generated-key call sites are checked by the compiler and serialized-key validator." : "Direct asset references:\n" + string.Join("\n", paths);
        }
        private void Run(Action action, bool showSaved = true)
        {
            try { action(); if (showSaved) status.text = "Definition saved. Unity compiles only when its code or typed accessors change."; }
            catch (Exception error) { status.text = error.Message; }
        }
        public void Dispose()
        {
            if (disposed) return; disposed = true;
            editing?.Dispose(); editing = null;
            EditorApplication.projectChanged -= RefreshList; serialized?.Dispose();
            foreach (var section in sections) section.Dispose();
            sections.Clear();
        }
    }
}
