using System.Linq;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Shared authoring page; all domain data and validation come from installed schema adapters.</summary>
    public sealed class DeucarianDefinitionWindow : EditorWindow, IDeucarianEditorReloadState
    {
        private const string ToolId = "deucarian.editor.definitions";
        [SerializeField] private string selectedSchema;
        [SerializeField] private List<DeucarianDefinitionPanelState> panelStates = new List<DeucarianDefinitionPanelState>();
        private DeucarianEditorWorkspace workspace;
        private DeucarianDefinitionPanel panel;
        private DeucarianEditorPageSession navigation;

        public string CaptureReloadState() => JsonUtility.ToJson(new ReloadState
        {
            schema = selectedSchema, panels = panelStates
        });

        public void RestoreReloadState(string state)
        {
            ReloadState value = null;
            try { if (!string.IsNullOrEmpty(state)) value = JsonUtility.FromJson<ReloadState>(state); }
            catch (ArgumentException) { /* A stale session snapshot must not prevent opening Definitions. */ }
            selectedSchema = value?.schema;
            panelStates = value?.panels?.Where(x => x != null && !string.IsNullOrEmpty(x.SchemaId))
                .GroupBy(x => x.SchemaId, StringComparer.Ordinal).Select(x => x.First()).ToList()
                ?? new List<DeucarianDefinitionPanelState>();
        }

        [Serializable]
        private sealed class ReloadState
        {
            public string schema;
            public List<DeucarianDefinitionPanelState> panels;
        }

        [InitializeOnLoadMethod]
        private static void Register() => DeucarianToolRegistry.Register(new DeucarianToolDescriptor(ToolId,
            "Definitions", "Create reusable definitions in code or the Inspector and generate typed keys.",
            DeucarianControlCenterArea.Developer, () => Open(), "com.deucarian.editor",
            DeucarianEditorIconIds.Document, new[] { "definitions", "assets", "code", "keys" }, createPage: CreatePage));

        public static void Open(string schemaId = null)
        {
            var window = DeucarianEditorWindowPages.GetStandalone<DeucarianDefinitionWindow>("Definitions");
            if (schemaId != null) window.selectedSchema = schemaId;
            window.CreateGUI();
            DeucarianEditorWorkspace.ConfigureWindow(window);
            window.Show(); window.Focus();
        }
        public void CreateGUI()
        {
            navigation?.Dispose();
            navigation = new DeucarianEditorPageSession(this, ToolId, BuildPage);
        }
        internal static IDeucarianEditorPage CreatePage() => DeucarianEditorWindowPages.Create<DeucarianDefinitionWindow>((window, root) => window.BuildPage(root));
        private void BuildPage(VisualElement root)
        {
            panel?.Dispose(); workspace?.Dispose(); root.Clear();
            workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "Definitions";
            workspace.Subtitle.text = "Create once, then use the same definition from C# or an Inspector dropdown.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, ToolId, filterNavigation: false);
            var schemas = DeucarianDefinitionSchema.Discover();
            if (schemas.Count == 0) { workspace.Content.Add(DeucarianEditorWorkspaceControls.Label("Install a package with reusable definitions to begin.", "dw-muted")); return; }
            int selected = 0;
            for (int i = 0; i < schemas.Count; i++) if (schemas[i].Id == selectedSchema) selected = i;
            var choices = new DeucarianEditorWorkspaceForm(workspace.Scope);
            choices.Choice("definition-domain", "Package definitions", schemas.Select(x => x.DisplayName).ToArray(), () => selected,
                value => { selected = value; selectedSchema = schemas[value].Id; ShowPanel(schemas[value]); });
            selectedSchema = schemas[selected].Id;
            ShowPanel(schemas[selected]);
        }
        private void ShowPanel(DeucarianDefinitionSchema schema)
        {
            panel?.Dispose(); workspace.Content.Clear();
            var state = panelStates.Find(x => x.SchemaId == schema.Id);
            if (state == null) { state = new DeucarianDefinitionPanelState { SchemaId = schema.Id }; panelStates.Add(state); }
            panel = new DeucarianDefinitionPanel(workspace.Content, schema, state: state);
        }
        private void OnDisable() { panel?.Dispose(); panel = null; navigation?.Dispose(); navigation = null; workspace?.Dispose(); workspace = null; }
    }
}
