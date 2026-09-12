using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Deucarian.Editor.Definitions
{
    internal static class DeucarianDefinitionAssemblies
    {
        internal static string[] References(Type spec)
        {
            var references = new HashSet<string>(StringComparer.Ordinal) { "Deucarian.Editor" };
            Collect(spec, references, new HashSet<Type>());
            return references.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        }
        private static void Collect(Type type, HashSet<string> references, HashSet<Type> visited)
        {
            if (!visited.Add(type)) return;
            if (type.IsArray) { Collect(type.GetElementType(), references, visited); return; }
            string assembly = type.Assembly.GetName().Name;
            if (assembly.StartsWith("Deucarian.", StringComparison.Ordinal)) references.Add(assembly);
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || typeof(UnityEngine.Object).IsAssignableFrom(type)) return;
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) Collect(field.FieldType, references, visited);
        }
    }
}
