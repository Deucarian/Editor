using System;
using UnityEngine;

namespace Deucarian.Editor.Definitions
{
    /// <summary>Per-page authoring state. Store it on the owning window, never globally by schema.</summary>
    [Serializable]
    public sealed class DeucarianDefinitionPanelState
    {
        public string SchemaId;
        public string SelectedAssetGuid;
        public string Search = string.Empty;
        public string CreateName = "NewDefinition";
        public Vector2 ListScroll;
        public Vector2 DetailsScroll;
    }
}
