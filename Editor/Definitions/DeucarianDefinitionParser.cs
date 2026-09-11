using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Bounded C# data grammar: literals, enum members, exact asset references and initializers.</summary>
    internal sealed class DeucarianDefinitionParser
    {
        private readonly string source;
        private int position;
        internal DeucarianDefinitionParser(string source) { this.source = source; }
        internal object Read(Type type)
        {
            object value = Value(type, 0);
            Space();
            if (position != source.Length) throw Error("Unexpected code after the definition initializer");
            return value;
        }

        private object Value(Type type, int depth)
        {
            if (depth > 32) throw Error("Definition nesting exceeds 32 levels");
            Space();
            if (Take("null"))
            {
                if (type.IsValueType) throw Error(type.Name + " cannot be null");
                return null;
            }
            if (type == typeof(string)) return String();
            if (type == typeof(bool)) { if (Take("true")) return true; Need("false"); return false; }
            if (type.IsEnum)
            {
                string prefix = DeucarianDefinitionSource.TypeName(type) + ".";
                var members = new List<string>();
                do
                {
                    string token = Atom();
                    if (!token.StartsWith(prefix, StringComparison.Ordinal)) throw Error("Use a fully qualified " + type.Name + " member");
                    string member = token.Substring(prefix.Length);
                    if (!Enum.GetNames(type).Contains(member)) throw Error("Unknown " + type.Name + " member: " + member);
                    members.Add(member);
                } while (Take("|"));
                if (members.Count > 1 && !type.IsDefined(typeof(FlagsAttribute), false)) throw Error("Combine values only on flags enums");
                return Enum.Parse(type, string.Join(",", members));
            }
            if (type.IsPrimitive)
            {
                string atom = Atom().TrimEnd('f', 'F', 'd', 'D', 'l', 'L');
                try
                {
                    object number = Convert.ChangeType(atom, type, CultureInfo.InvariantCulture);
                    if (number is float f && (float.IsNaN(f) || float.IsInfinity(f)) || number is double d && (double.IsNaN(d) || double.IsInfinity(d))) throw Error("Use finite numeric values");
                    return number;
                }
                catch (FormatException) { throw Error("Expected a " + type.Name + " literal"); }
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                Need("global::Deucarian.Editor.Definitions.DeucarianDefinitionAssets.Load<" + DeucarianDefinitionSource.TypeName(type) + ">");
                Need("("); string guid = String(); Need(","); long id = (long)Value(typeof(long), depth + 1); Need(")");
                return DeucarianDefinitionAssets.Load(type, guid, id);
            }
            Need("new"); Need(DeucarianDefinitionSource.TypeName(type)); Need("{");
            if (type.IsArray)
            {
                var values = new List<object>();
                while (!Take("}"))
                {
                    if (values.Count >= 10000) throw Error("A definition array exceeds 10000 entries");
                    values.Add(Value(type.GetElementType(), depth + 1));
                    if (!Take(",")) { Need("}"); break; }
                }
                var array = Array.CreateInstance(type.GetElementType(), values.Count);
                for (int i = 0; i < values.Count; i++) array.SetValue(values[i], i);
                return array;
            }
            var result = Activator.CreateInstance(type);
            var fields = DeucarianDefinitionSource.Fields(type).ToDictionary(x => x.Name, StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            while (!Take("}"))
            {
                string name = Atom();
                if (!fields.TryGetValue(name, out var field)) throw Error("Unknown " + type.Name + " field: " + name);
                if (!seen.Add(name)) throw Error("Duplicate definition field: " + name);
                Need("="); field.SetValue(result, Value(field.FieldType, depth + 1));
                if (!Take(",")) { Need("}"); break; }
            }
            return result;
        }

        private string String()
        {
            Need("\""); var result = new StringBuilder();
            while (position < source.Length)
            {
                char ch = source[position++];
                if (ch == '"') return result.ToString();
                if (ch == '\\')
                {
                    if (position == source.Length) throw Error("Incomplete string escape");
                    char escape = source[position++];
                    switch (escape)
                    {
                        case 'n': ch = '\n'; break; case 'r': ch = '\r'; break; case 't': ch = '\t'; break;
                        case '\\': ch = '\\'; break; case '"': ch = '"'; break;
                        case 'u':
                            if (position + 4 > source.Length || !ushort.TryParse(source.Substring(position, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort code)) throw Error("Invalid Unicode escape");
                            ch = (char)code; position += 4; break;
                        default: throw Error("Unsupported string escape");
                    }
                }
                else if (ch == '\r' || ch == '\n') throw Error("Escape newlines inside C# strings");
                result.Append(ch);
            }
            throw Error("Unterminated string");
        }

        private string Atom()
        {
            Space(); int start = position;
            while (position < source.Length && !char.IsWhiteSpace(source[position]) && ",{}=()|".IndexOf(source[position]) < 0) position++;
            if (start == position) throw Error("Expected a declaration value");
            return source.Substring(start, position - start);
        }
        private bool Take(string token)
        {
            Space();
            if (position + token.Length > source.Length || string.CompareOrdinal(source, position, token, 0, token.Length) != 0) return false;
            position += token.Length; return true;
        }
        private void Need(string token) { if (!Take(token)) throw Error("Expected '" + token + "'"); }
        private void Space()
        {
            while (position < source.Length)
            {
                if (char.IsWhiteSpace(source[position])) { position++; continue; }
                if (position + 1 < source.Length && source[position] == '/' && source[position + 1] == '/')
                { while (position < source.Length && source[position] != '\n') position++; continue; }
                break;
            }
        }
        private FormatException Error(string message) => new FormatException(message + " near character " + position + ". Edit supported declaration values or use the definition editor.");
    }
}
