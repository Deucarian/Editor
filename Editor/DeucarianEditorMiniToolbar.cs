using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorMiniToolbar
    {
        private static GUIStyle miniButtonStyle;

        public static bool Button(string label, bool enabled = true, params GUILayoutOption[] options)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                return GUILayout.Button(label ?? string.Empty, MiniButtonStyle, options);
            }
        }

        public static bool PingButton(Object target)
        {
            bool clicked = Button("Ping", target != null, GUILayout.Width(48f), GUILayout.Height(22f));
            if (clicked && target != null)
            {
                EditorGUIUtility.PingObject(target);
            }

            return clicked;
        }

        public static bool SelectButton(Object target)
        {
            bool clicked = Button("Select", target != null, GUILayout.Width(56f), GUILayout.Height(22f));
            if (clicked && target != null)
            {
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
            }

            return clicked;
        }

        private static GUIStyle MiniButtonStyle
        {
            get
            {
                if (miniButtonStyle == null)
                {
                    miniButtonStyle = new GUIStyle(DeucarianEditorButtons.SecondaryStyle)
                    {
                        fixedHeight = 22f,
                        fontSize = 10,
                        padding = new RectOffset(6, 6, 2, 3),
                        margin = new RectOffset(2, 0, 0, 0)
                    };
                }

                return miniButtonStyle;
            }
        }
    }
}
