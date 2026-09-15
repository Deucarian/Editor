using System;
using System.IO;
using Deucarian.Editor.Definitions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianDefinitionWorkflowTests
    {
        private const string Folder = "Assets/DeucarianDefinitions/editor-workflow-tests";
        private readonly Schema schema = new Schema();
        private byte[] previousIndex;
        private bool automaticDisabled;
        private bool initialized;
        private bool parentExisted;

        private sealed class Schema : DeucarianSerializedDefinitionSchema<DefinitionWorkflowAsset, DefinitionWorkflowSpec>
        {
            public override string Id => "editor-workflow-tests";
            public override string DisplayName => "Workflow test";
        }

        [SetUp]
        public void SetUp()
        {
            Assert.That(Directory.Exists(Folder), Is.False, "Refuse to replace pre-existing test assets.");
            parentExisted = Directory.Exists("Assets/DeucarianDefinitions");
            previousIndex = File.Exists(DeucarianDefinitionIndex.Path) ? File.ReadAllBytes(DeucarianDefinitionIndex.Path) : null;
            automaticDisabled = SessionState.GetBool(DeucarianKeyGeneration.AutomaticRefreshDisabledSessionKey, false);
            SessionState.SetBool(DeucarianKeyGeneration.AutomaticRefreshDisabledSessionKey, true);
            EditorApplication.LockReloadAssemblies();
            initialized = true;
        }

        [TearDown]
        public void TearDown()
        {
            if (!initialized) return;
            try
            {
                AssetDatabase.DeleteAsset(Folder);
                if (!parentExisted && Directory.Exists("Assets/DeucarianDefinitions") && Directory.GetFileSystemEntries("Assets/DeucarianDefinitions").Length == 0)
                    AssetDatabase.DeleteAsset("Assets/DeucarianDefinitions");
                if (previousIndex == null) File.Delete(DeucarianDefinitionIndex.Path);
                else File.WriteAllBytes(DeucarianDefinitionIndex.Path, previousIndex);
            }
            finally
            {
                SessionState.SetBool(DeucarianKeyGeneration.AutomaticRefreshDisabledSessionKey, automaticDisabled);
                EditorApplication.UnlockReloadAssemblies();
                initialized = false;
            }
        }

        [Test]
        public void RepeatedSynchronizationDoesNotWriteAnyAgreedInput()
        {
            var asset = DeucarianDefinitionSync.Create(schema, "Example");
            var record = DeucarianDefinitionSync.FindAsset(asset);
            string source = AssetDatabase.GUIDToAssetPath(record.sourceGuid);
            string assembly = Folder + "/Editor/Deucarian.Definitions." + schema.Id + ".asmdef";
            string[] paths = { source, assembly, AssetDatabase.GetAssetPath(asset), DeucarianDefinitionIndex.Path };
            var marker = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            foreach (string path in paths) File.SetLastWriteTimeUtc(path, marker);

            DeucarianDefinitionSync.SynchronizeSource(schema, source);
            DeucarianDefinitionSync.SynchronizeSource(schema, source);

            foreach (string path in paths) Assert.That(File.GetLastWriteTimeUtc(path), Is.EqualTo(marker), path);
            Assert.That(EditorUtility.IsDirty(asset), Is.False);
        }

        [Test]
        public void CodeEditAppliesOnceWithoutFormattingRewrite()
        {
            var asset = DeucarianDefinitionSync.Create(schema, "Example");
            string path = AssetDatabase.GUIDToAssetPath(DeucarianDefinitionSync.FindAsset(asset).sourceGuid);
            string source = File.ReadAllText(path).Replace("Original", "Code edit").Replace("\n", "\r\n");
            File.WriteAllText(path, source);
            var marker = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(path, marker);

            DeucarianDefinitionSync.SynchronizeSource(schema, path);

            Assert.That(((DefinitionWorkflowSpec)schema.Read(asset)).Message, Is.EqualTo("Code edit"));
            Assert.That(File.ReadAllText(path), Is.EqualTo(source));
            Assert.That(File.GetLastWriteTimeUtc(path), Is.EqualTo(marker));
        }

        [Test]
        public void ConflictingEditsPreserveBothSidesUntilExplicitResolution()
        {
            var asset = DeucarianDefinitionSync.Create(schema, "Example");
            var record = DeucarianDefinitionSync.FindAsset(asset);
            string path = AssetDatabase.GUIDToAssetPath(record.sourceGuid);
            var spec = (DefinitionWorkflowSpec)schema.Read(asset);
            spec.Message = "Asset edit"; schema.Apply(asset, spec);
            string source = File.ReadAllText(path).Replace("Original", "Code edit");
            File.WriteAllText(path, source);

            Assert.Throws<InvalidOperationException>(() => DeucarianDefinitionSync.SynchronizeSource(schema, path));
            Assert.That(((DefinitionWorkflowSpec)schema.Read(asset)).Message, Is.EqualTo("Asset edit"));
            Assert.That(File.ReadAllText(path), Is.EqualTo(source));
            DeucarianDefinitionSync.Resolve(schema, record, false);
            Assert.That(((DefinitionWorkflowSpec)DeucarianDefinitionSource.Read(schema, File.ReadAllText(path))).Message, Is.EqualTo("Asset edit"));
        }

        [Test]
        public void DuplicateProjectsFinalValuesWithoutSavingUnrelatedDirtyAssets()
        {
            var original = DeucarianDefinitionSync.Create(schema, "Example");
            var value = (DefinitionWorkflowSpec)schema.Read(original);
            value.Message = "Unsaved original edit"; schema.Apply(original, value);

            var copy = DeucarianDefinitionSync.Duplicate(schema, original);
            var copied = (DefinitionWorkflowSpec)schema.Read(copy);
            string source = AssetDatabase.GUIDToAssetPath(DeucarianDefinitionSync.FindAsset(copy).sourceGuid);

            Assert.That(copied.Message, Is.EqualTo(value.Message));
            Assert.That(copied.Id, Is.Not.EqualTo(value.Id));
            Assert.That(copied.Name, Is.EqualTo("ExampleCopy"));
            Assert.That(File.ReadAllText(source), Is.EqualTo(DeucarianDefinitionSource.Write(schema, copied)));
            Assert.That(EditorUtility.IsDirty(original), Is.True, "Creating a different definition must not save the user's other asset edits.");
        }

        [Test]
        public void PanelStateRestoresSelectionAndDraftWithoutSharingAnotherPanel()
        {
            var asset = DeucarianDefinitionSync.Create(schema, "Example");
            var state = new DeucarianDefinitionPanelState
            {
                SchemaId = schema.Id, SelectedAssetGuid = DeucarianDefinitionSync.FindAsset(asset).assetGuid,
                Search = "Exam", CreateName = "Unfinished name"
            };
            string json = JsonUtility.ToJson(state);
            var restored = JsonUtility.FromJson<DeucarianDefinitionPanelState>(json);
            var root = new VisualElement();
            using (var panel = new DeucarianDefinitionPanel(root, schema, state: restored))
            using (var independent = new DeucarianDefinitionPanel(new VisualElement(), schema))
            {
                Assert.That(root.Q<TextField>("definition-name").value, Is.EqualTo("Unfinished name"));
                Assert.That(root.Q<Button>("definition-sync"), Is.Not.Null);
                Assert.That(panel.CaptureState().SelectedAssetGuid, Is.EqualTo(state.SelectedAssetGuid));
                Assert.That(independent.CaptureState().SelectedAssetGuid, Is.Empty);
                var field = root.Q<TextField>("message");
                panel.Select(asset);
                Assert.That(root.Q<TextField>("message"), Is.SameAs(field), "Selecting the current definition must preserve its bound controls.");
            }
        }

        [TestCase("Assets/Example.asset", true)]
        [TestCase("Assets/DeucarianGeneratedKeys/ExampleKey/Keys.g.cs", false)]
        [TestCase("Assets/DeucarianDefinitions/example/Editor/Example.definition.cs", false)]
        [TestCase("Packages/com.example/Example.asset", false)]
        public void KeyImportsIgnoreTheirOwnOutputsAndDeclarationImports(string path, bool expected)
        {
            Assert.That(DeucarianKeyImportRefresh.AffectsDefinitions(path), Is.EqualTo(expected));
        }

        [Test]
        public void GeneratedContentComparisonIgnoresCheckoutLineEndingsOnly()
        {
            Assert.That(DeucarianKeySourceText.SameContent("first\r\nsecond\r\n", "first\nsecond\n"), Is.True);
            Assert.That(DeucarianKeySourceText.SameContent("first\r\nchanged\r\n", "first\nsecond\n"), Is.False);
        }
    }
}
