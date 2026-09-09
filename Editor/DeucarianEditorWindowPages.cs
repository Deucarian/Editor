using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Compatibility bridge for existing window controllers. New tools can contribute plain pages.</summary>
    public static class DeucarianEditorWindowPages
    {
        public static IDeucarianEditorPage Create<T>(Action<T, VisualElement> build,
            Action<T, string> activate = null, Action<T> deactivate = null,
            Action<T> update = null) where T : EditorWindow
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            var controller = ScriptableObject.CreateInstance<T>();
            controller.hideFlags = HideFlags.HideAndDontSave;
            var root = new VisualElement();
            try
            {
                build(controller, root);
                return new DeucarianEditorPage(root,
                    route => activate?.Invoke(controller, route),
                    () => deactivate?.Invoke(controller),
                    bounds => { controller.position = bounds; update?.Invoke(controller); },
                    () => Release(controller));
            }
            catch
            {
                Release(controller);
                throw;
            }
        }

        public static T GetStandalone<T>(string title = null) where T : EditorWindow
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<T>())
                if (!IsPageController(candidate)) return candidate;
            var window = ScriptableObject.CreateInstance<T>();
            if (!string.IsNullOrEmpty(title)) window.titleContent = new GUIContent(title);
            return window;
        }

        public static bool IsPageController(EditorWindow window) =>
            window != null && (window.hideFlags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave;

        private static void Release(EditorWindow controller)
        {
            // Never shown: Close() requires a native parent in Unity 2021. Release the
            // editor-only controller synchronously, including while the game is playing.
            if (controller != null) UnityEngine.Object.DestroyImmediate(controller);
        }
    }
}
