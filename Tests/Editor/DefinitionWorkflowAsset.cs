using System;
using Deucarian.Editor.Definitions;
using UnityEngine;

namespace Deucarian.Editor.Tests
{
    public sealed class DefinitionWorkflowAsset : ScriptableObject
    {
        public string id;
        public string displayName;
        public string message;
    }

    [Serializable]
    public sealed class DefinitionWorkflowSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("message")] public string Message = "Original";
    }
}
