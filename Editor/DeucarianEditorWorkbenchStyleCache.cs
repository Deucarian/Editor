using UnityEditor;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorWorkbenchStyleCache
    {
        private DeucarianEditorWorkbenchStyles styles;
        private bool proSkin;

        internal DeucarianEditorWorkbenchStyles Current
        {
            get
            {
                bool currentSkin = EditorGUIUtility.isProSkin;
                if (styles == null || currentSkin != proSkin)
                {
                    styles = new DeucarianEditorWorkbenchStyles();
                    proSkin = currentSkin;
                }

                return styles;
            }
        }

        internal void Clear()
        {
            styles = null;
        }
    }
}
