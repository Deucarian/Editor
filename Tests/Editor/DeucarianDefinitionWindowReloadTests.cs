using System;
using System.Collections.Generic;
using Deucarian.Editor.Definitions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianDefinitionWindowReloadTests
    {
        [Test]
        public void DestroyedDefinitionControllerRestoresSchemaAndEachPanelsDraft()
        {
            var value = new ReloadState
            {
                schema = "second-schema",
                panels = new List<DeucarianDefinitionPanelState>
                {
                    new DeucarianDefinitionPanelState { SchemaId = "first-schema", SelectedAssetGuid = "first-asset", CreateName = "First unfinished name" },
                    new DeucarianDefinitionPanelState { SchemaId = "second-schema", SelectedAssetGuid = "second-asset", CreateName = "Second unfinished name", Search = "filtered", ListScroll = new Vector2(0, 42), DetailsScroll = new Vector2(0, 120) }
                }
            };
            DeucarianDefinitionWindow firstController = null;
            var original = DeucarianEditorWindowPages.Create<DeucarianDefinitionWindow>((controller, root) => firstController = controller);
            string saved;
            try
            {
                var state = (IDeucarianEditorReloadState)original;
                state.RestoreReloadState(JsonUtility.ToJson(value));
                saved = state.CaptureReloadState();
            }
            finally { original.Dispose(); }
            Assert.That(firstController == null, Is.True, "The old hidden controller must actually be destroyed.");

            using (var recreated = DeucarianEditorWindowPages.Create<DeucarianDefinitionWindow>((controller, root) =>
            {
                root.Clear();
                root.Add(new Label(JsonUtility.FromJson<ReloadState>(controller.CaptureReloadState()).schema));
            }))
            {
                var state = (IDeucarianEditorReloadState)recreated;
                state.RestoreReloadState(saved);
                var restored = JsonUtility.FromJson<ReloadState>(state.CaptureReloadState());
                Assert.That(recreated.Root.Q<Label>().text, Is.EqualTo("second-schema"));
                Assert.That(restored.panels.Count, Is.EqualTo(2));
                Assert.That(restored.panels[0].CreateName, Is.EqualTo("First unfinished name"));
                Assert.That(restored.panels[1].SelectedAssetGuid, Is.EqualTo("second-asset"));
                Assert.That(restored.panels[1].Search, Is.EqualTo("filtered"));
                Assert.That(restored.panels[1].DetailsScroll.y, Is.EqualTo(120));
                Assert.That(restored.panels[1].ListScroll.y, Is.EqualTo(42));
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{invalid json")]
        public void MissingOrInvalidSnapshotsResetSafely(string snapshot)
        {
            using (var page = DeucarianEditorWindowPages.Create<DeucarianDefinitionWindow>((controller, root) => { }))
            {
                var state = (IDeucarianEditorReloadState)page;
                state.RestoreReloadState("{\"schema\":\"old\",\"panels\":[{\"SchemaId\":\"old\"}]}");
                Assert.DoesNotThrow(() => state.RestoreReloadState(snapshot));
                var restored = JsonUtility.FromJson<ReloadState>(state.CaptureReloadState());
                Assert.That(restored.schema, Is.Null.Or.Empty);
                Assert.That(restored.panels, Is.Empty);
            }
        }

        [Serializable]
        private sealed class ReloadState
        {
            public string schema;
            public List<DeucarianDefinitionPanelState> panels;
        }
    }
}
