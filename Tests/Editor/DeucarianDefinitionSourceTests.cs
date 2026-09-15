using System;
using Deucarian.Editor.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianDefinitionSourceTests
    {
        [Serializable]
        public sealed class Spec : DeucarianDefinitionSpec
        {
            public string Message;
            public float Number;
            public bool Enabled;
            public SampleMode Mode;
            public SampleFlags Flags;
            public Vector3 Position;
            public int[] Values = Array.Empty<int>();
        }
        public enum SampleMode { First, Second }
        [Flags] public enum SampleFlags { None = 0, First = 1, Second = 2 }
        private sealed class Schema : DeucarianDefinitionSchema
        {
            public override string Id => "test";
            public override string DisplayName => "Test";
            public override Type AssetType => typeof(ScriptableObject);
            public override Type SpecType => typeof(Spec);
            public override DeucarianDefinitionSpec Read(ScriptableObject asset) => throw new NotSupportedException();
            public override void Apply(ScriptableObject asset, DeucarianDefinitionSpec spec) => throw new NotSupportedException();
            public override void Validate(DeucarianDefinitionSpec spec) { }
        }

        [Test]
        public void LiteralsEnumsVectorsArraysAndEscapesRoundTripDeterministically()
        {
            var schema = new Schema();
            var original = new Spec { Id = "unchanging", Name = "ConnectionLost", Message = "a \"quote\"\\path\nnext\t\u0001 \u00e9", Number = -1.25f, Enabled = true, Mode = SampleMode.Second, Position = new Vector3(2, 3, 4), Values = new[] { -1, 2, 3 } };
            string source = DeucarianDefinitionSource.Write(schema, original);
            var read = (Spec)DeucarianDefinitionSource.Read(schema, source);
            Assert.That(read.Message, Is.EqualTo(original.Message));
            Assert.That(read.Position, Is.EqualTo(original.Position));
            Assert.That(read.Values, Is.EqualTo(original.Values));
            Assert.That(read.Mode, Is.EqualTo(original.Mode));
            Assert.That(DeucarianDefinitionSource.Write(schema, read), Is.EqualTo(source));
        }

        [Test]
        public void FlagsRoundTripAndInvalidNumericValuesFailBeforeWritingCode()
        {
            var schema = new Schema();
            var spec = new Spec { Id = "stable", Name = "Example", Flags = SampleFlags.First | SampleFlags.Second };
            var read = (Spec)DeucarianDefinitionSource.Read(schema, DeucarianDefinitionSource.Write(schema, spec));
            Assert.That(read.Flags, Is.EqualTo(spec.Flags));
            spec.Number = float.NaN;
            Assert.Throws<ArgumentException>(() => DeucarianDefinitionSource.Write(schema, spec));
            spec.Number = 1; spec.Mode = (SampleMode)99;
            Assert.Throws<ArgumentException>(() => DeucarianDefinitionSource.Write(schema, spec));
        }

        [Test]
        public void ACodeEditChangesOnlyItsDeclaredValue()
        {
            var schema = new Schema();
            string source = DeucarianDefinitionSource.Write(schema, new Spec { Id = "stable", Name = "Example", Message = "Before" });
            var value = (Spec)DeucarianDefinitionSource.Read(schema, source.Replace("\"Before\"", "\"After\""));
            Assert.That(value.Message, Is.EqualTo("After"));
            Assert.That(value.Id, Is.EqualTo("stable"));
        }

        [Test]
        public void ExecutableExpressionsAndUnknownFieldsAreRejected()
        {
            var schema = new Schema();
            string source = DeucarianDefinitionSource.Write(schema, new Spec { Id = "stable", Name = "Example", Message = "Safe" });
            Assert.Throws<FormatException>(() => DeucarianDefinitionSource.Read(schema, source.Replace("\"Safe\"", "global::System.IO.File.ReadAllText(\"anything\")")));
            Assert.Throws<FormatException>(() => DeucarianDefinitionSource.Read(schema, source.Replace("Message =", "Unknown =")));
            Assert.Throws<FormatException>(() => DeucarianDefinitionSource.Read(schema, source.Replace("Message = \"Safe\",", "Message = \"Safe\", Message = \"Again\",")));
        }

        [Test]
        public void ApplicationMethodsInADeclarationAreNotSilentlyOverwritten()
        {
            var schema = new Schema();
            string source = DeucarianDefinitionSource.Write(schema, new Spec { Id = "stable", Name = "Example" });
            Assert.Throws<FormatException>(() => DeucarianDefinitionSource.Read(schema, source.Replace("// end-definition-value", "// end-definition-value\npublic static void Other() {}")));
        }
    }
}
