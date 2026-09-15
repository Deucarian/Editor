using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    [Serializable]
    public sealed class DeucarianDefinitionRecord
    {
        public string schema;
        public string sourceGuid;
        public string assetGuid;
        public string lastHash;
        public string identity;
    }

    [Serializable]
    internal sealed class DeucarianDefinitionIndex
    {
        internal const string Path = "ProjectSettings/DeucarianDefinitions.json";
        public List<DeucarianDefinitionRecord> records = new List<DeucarianDefinitionRecord>();
        internal static DeucarianDefinitionIndex Read() => File.Exists(Path)
            ? JsonUtility.FromJson<DeucarianDefinitionIndex>(File.ReadAllText(Path)) ?? new DeucarianDefinitionIndex()
            : new DeucarianDefinitionIndex();
        internal void Save()
        {
            records.Sort((a, b) => string.CompareOrdinal(a.sourceGuid, b.sourceGuid));
            string value = JsonUtility.ToJson(this, true) + "\n";
            if (!File.Exists(Path) || File.ReadAllText(Path) != value) File.WriteAllText(Path, value);
        }
    }
}
