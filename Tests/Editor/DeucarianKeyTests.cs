using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianKeyTests
    {
        [Test]
        public void SourceGenerationIsDeterministicAndRejectsAmbiguousSymbols()
        {
            var source = new SampleSource();
            string first = DeucarianKeySourceText.Create(source);
            Assert.That(first, Is.EqualTo(DeucarianKeySourceText.Create(source)));
            Assert.That(first, Does.Contain("Alpha").And.Contain("stable.id"));
            source.Choices[0] = new DeucarianKeyChoice("stable.id", "Renamed");
            string renamed = DeucarianKeySourceText.Create(source);
            Assert.That(renamed, Does.Contain("Renamed").And.Contain("stable.id").And.Not.Contain(" Alpha "));
            source.Choices.Add(new DeucarianKeyChoice("other.id", "Renamed"));
            Assert.That(Assert.Throws<InvalidOperationException>(() => DeucarianKeySourceText.Create(source)).Message,
                Does.Contain("different display names"));
            source.Choices.Clear();
            Assert.That(DeucarianKeySourceText.Create(source), Does.Not.Contain("stable.id"));
        }

        [Test]
        public void EditorOnlyDefinitionsAreExcludedAndDuplicatesExplainTheRepair()
        {
            Assert.That(DeucarianKeyChoices.Read(typeof(ExampleKey), typeof(ExampleSetAttribute)), Is.Empty);
            var error = Assert.Throws<InvalidOperationException>(() =>
                DeucarianKeyChoices.Read(typeof(ExampleKey), typeof(ExampleSetAttribute), true));
            Assert.That(error.Message, Does.Contain("duplicates").And.Contain("Keep one definition"));
        }

        [Test]
        public void NestedArrayAndClosedGenericFieldsResolveForTheSamePicker()
        {
            var carrier = ScriptableObject.CreateInstance<KeyCarrier>();
            try
            {
                using (var serialized = new SerializedObject(carrier))
                {
                    var field = serialized.FindProperty("Rows.Array.data[0].Key");
                    Assert.That(DeucarianKeyPropertyType.Resolve(field), Is.EqualTo(typeof(GenericKey<int>)));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(carrier); }
        }

        private sealed class SampleSource : DeucarianAssetKeySource
        {
            public readonly List<DeucarianKeyChoice> Choices = new List<DeucarianKeyChoice>
                { new DeucarianKeyChoice("stable.id", "Alpha") };
            public override Type KeyType => typeof(ExampleKey);
            public override Type DefinitionSetAttribute => typeof(ExampleSetAttribute);
            public override string GeneratedClassName => "ExampleKeys";
            public override IReadOnlyList<DeucarianKeyChoice> ReadDefinitions() => Choices;
        }
    }

    [Serializable] public class ExampleKey { public string Id => "duplicate.id"; }
    [AttributeUsage(AttributeTargets.Class)] public sealed class ExampleSetAttribute : Attribute { }
    [ExampleSet] public static class DuplicateDefinitions
    {
        public static ExampleKey First => new ExampleKey();
        public static ExampleKey Second => new ExampleKey();
    }
    [Serializable] public class GenericKey<T> { [SerializeField] private string definitionId = "test"; }
    [Serializable] public class KeyRow { public GenericKey<int> Key = new GenericKey<int>(); }
    public sealed class KeyCarrier : ScriptableObject { public KeyRow[] Rows = { new KeyRow() }; }
}
