using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Deterministic, editable C# declarations. The parser accepts data, never executable methods.</summary>
    public static class DeucarianDefinitionSource
    {
        public const string Header = "// <deucarian-definition schema=\"";
        internal const string Begin = "// definition-value";
        internal const string End = "// end-definition-value";

        public static string Write(DeucarianDefinitionSchema schema, DeucarianDefinitionSpec spec)
        {
            schema.Validate(spec);
            string name = Identifier(spec.Name);
            return Header + schema.Id + "\" />\n" +
                "// Editable declaration. Use the package definition editor or edit the values below.\n" +
                "namespace Deucarian.ProjectDefinitions." + Identifier(schema.Id) + "\n{\n" +
                "    public static class " + name + "\n    {\n" +
                "        public static " + TypeName(schema.SpecType) + " Value =>\n" +
                "        " + Begin + "\n        " + Value(spec, schema.SpecType, 2) + ";\n" +
                "        " + End + "\n    }\n}\n";
        }

        public static string SchemaId(string source)
        {
            var match = Regex.Match(source ?? string.Empty, "^// <deucarian-definition schema=\"([a-zA-Z0-9.-]+)\" />");
            if (!match.Success) throw new FormatException("This is not an editable Deucarian definition. Create one from its package's Create menu or definition editor.");
            return match.Groups[1].Value;
        }

        public static DeucarianDefinitionSpec Read(DeucarianDefinitionSchema schema, string source)
        {
            if (SchemaId(source) != schema.Id) throw new FormatException("Definition schema does not match " + schema.DisplayName + ".");
            int begin = source.IndexOf(Begin, StringComparison.Ordinal);
            int end = source.LastIndexOf(End, StringComparison.Ordinal);
            if (begin < 0 || end <= begin) throw new FormatException("Keep the definition-value markers around the declaration initializer.");
            string expression = source.Substring(begin + Begin.Length, end - begin - Begin.Length).Trim();
            if (!expression.EndsWith(";", StringComparison.Ordinal)) throw new FormatException("End the definition initializer with a semicolon.");
            var spec = (DeucarianDefinitionSpec)new DeucarianDefinitionParser(expression.Substring(0, expression.Length - 1)).Read(schema.SpecType);
            schema.Validate(spec);
            // Only the dedicated declarative frame is owned. Reject helpers instead of overwriting them.
            string expected = Write(schema, spec);
            string before = expected.Substring(0, expected.IndexOf(Begin, StringComparison.Ordinal));
            string after = expected.Substring(expected.LastIndexOf(End, StringComparison.Ordinal));
            string sourceFrame = Regex.Replace(source.Substring(0, begin), @"public\s+static\s+class\s+[a-zA-Z_][a-zA-Z0-9_]*", "public static class " + Identifier(spec.Name));
            if (Whitespace(sourceFrame) != Whitespace(before) || Whitespace(source.Substring(end)) != Whitespace(after))
                throw new FormatException("Keep this file's declaration frame intact. Edit its values or use the definition editor; put application methods in another file.");
            return spec;
        }

        internal static string Whitespace(string value) => Regex.Replace(value, @"\s+", string.Empty);
        public static string Identifier(string value)
        {
            string result = Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9_]", "_");
            if (string.IsNullOrEmpty(result)) return "Definition";
            return "Definition_" + result;
        }
        internal static string TypeName(Type type) => type.IsArray ? TypeName(type.GetElementType()) + "[]" : "global::" + type.FullName.Replace('+', '.');
        internal static FieldInfo[] Fields(Type type) => type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name, StringComparer.Ordinal).ToArray();
        internal static string Quote(string value)
        {
            var text = new StringBuilder("\"");
            foreach (char ch in value)
                switch (ch)
                {
                    case '\\': text.Append("\\\\"); break;
                    case '"': text.Append("\\\""); break;
                    case '\n': text.Append("\\n"); break;
                    case '\r': text.Append("\\r"); break;
                    case '\t': text.Append("\\t"); break;
                    default: if (char.IsControl(ch)) text.Append("\\u" + ((int)ch).ToString("x4")); else text.Append(ch); break;
                }
            return text.Append('"').ToString();
        }

        internal static string Value(object value, Type type, int indent)
        {
            if (value == null) return "null";
            if (type == typeof(string)) return Quote((string)value);
            if (type == typeof(bool)) return (bool)value ? "true" : "false";
            if (type.IsEnum)
            {
                var members = value.ToString().Split(',').Select(x => x.Trim()).ToArray();
                if (members.Any(x => !Enum.GetNames(type).Contains(x))) throw new ArgumentException("Select named " + type.Name + " values before synchronizing the definition.");
                return string.Join(" | ", members.Select(x => TypeName(type) + "." + x));
            }
            if (type == typeof(float))
            {
                if (float.IsNaN((float)value) || float.IsInfinity((float)value)) throw new ArgumentException("Definitions require finite numeric values.");
                return ((float)value).ToString("R", CultureInfo.InvariantCulture) + "f";
            }
            if (type == typeof(double))
            {
                if (double.IsNaN((double)value) || double.IsInfinity((double)value)) throw new ArgumentException("Definitions require finite numeric values.");
                return ((double)value).ToString("R", CultureInfo.InvariantCulture) + "d";
            }
            if (type == typeof(long)) return value + "L";
            if (type.IsPrimitive) return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                var asset = (UnityEngine.Object)value;
                if (asset == null) return "null";
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long id) || string.IsNullOrEmpty(guid))
                    throw new ArgumentException("Definitions can reference project assets, not scene objects: " + asset.name);
                return "global::Deucarian.Editor.Definitions.DeucarianDefinitionAssets.Load<" + TypeName(type) + ">(" + Quote(guid) + ", " + id + "L)";
            }
            string padding = new string(' ', indent * 4);
            var builder = new StringBuilder("new " + TypeName(type) + "\n" + padding + "{\n");
            if (type.IsArray)
                foreach (var item in (Array)value) builder.Append(padding).Append("    ").Append(Value(item, type.GetElementType(), indent + 1)).Append(",\n");
            else
                foreach (var field in Fields(type)) builder.Append(padding).Append("    ").Append(field.Name).Append(" = ").Append(Value(field.GetValue(value), field.FieldType, indent + 1)).Append(",\n");
            return builder.Append(padding).Append('}').ToString();
        }
    }
}
