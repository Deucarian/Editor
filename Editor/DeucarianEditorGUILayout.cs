using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorGUILayout
    {
        public static void Space(int pixels = 6)
        {
            GUILayout.Space(Mathf.Max(0, pixels));
        }

        public static void Separator(int thickness = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, Mathf.Max(1, thickness));
            EditorGUI.DrawRect(rect, DeucarianEditorColors.Border);
        }

        public static void HorizontalLine()
        {
            Separator();
        }

        public static void BeginCompactToolbarRow()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        }

        public static void EndCompactToolbarRow()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
