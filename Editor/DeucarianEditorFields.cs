using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.Editor
{
    public static class DeucarianEditorFields
    {
        public static T DrawAssetFieldWithSelectButton<T>(
            string label,
            T value,
            string selectButtonLabel = "Select",
            Action<T> onValueChanged = null,
            Action<T> onSelectClicked = null,
            Func<T> onFindClicked = null)
            where T : Object
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            T nextValue = EditorGUILayout.ObjectField(label ?? string.Empty, value, typeof(T), false) as T;
            if (EditorGUI.EndChangeCheck())
            {
                NotifyValueChanged(value, nextValue, onValueChanged);
            }

            bool hasValue = nextValue != null;
            bool canClick = hasValue || onFindClicked != null;
            string buttonLabel = hasValue ? selectButtonLabel : "Find";
            if (string.IsNullOrWhiteSpace(buttonLabel))
            {
                buttonLabel = hasValue ? "Select" : "Find";
            }

            using (new EditorGUI.DisabledScope(!canClick))
            {
                if (GUILayout.Button(buttonLabel, GUILayout.Width(72)))
                {
                    if (hasValue)
                    {
                        SelectAndPing(nextValue);
                        if (onSelectClicked != null)
                        {
                            onSelectClicked(nextValue);
                        }
                    }
                    else if (onFindClicked != null)
                    {
                        T foundValue = onFindClicked();
                        if (foundValue != null)
                        {
                            NotifyValueChanged(nextValue, foundValue, onValueChanged);
                            nextValue = foundValue;
                            SelectAndPing(nextValue);
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            return nextValue;
        }

        public static string DrawReadonlyTextField(string label, string value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                return EditorGUILayout.TextField(label ?? string.Empty, value ?? string.Empty);
            }
        }

        public static string DrawCopyableTextField(string label, string value, string buttonLabel = "Copy")
        {
            EditorGUILayout.BeginHorizontal();
            string result = DrawReadonlyTextField(label, value);

            if (GUILayout.Button(string.IsNullOrWhiteSpace(buttonLabel) ? "Copy" : buttonLabel, GUILayout.Width(72)))
            {
                EditorGUIUtility.systemCopyBuffer = value ?? string.Empty;
            }

            EditorGUILayout.EndHorizontal();
            return result;
        }

        public static void DrawPathWithOpenButton(string label, string path, string buttonLabel = "Open")
        {
            EditorGUILayout.BeginHorizontal();
            DrawReadonlyTextField(label, path);

            bool canOpen = !string.IsNullOrWhiteSpace(path) && (File.Exists(path) || Directory.Exists(path));
            using (new EditorGUI.DisabledScope(!canOpen))
            {
                if (GUILayout.Button(string.IsNullOrWhiteSpace(buttonLabel) ? "Open" : buttonLabel, GUILayout.Width(72)))
                {
                    EditorUtility.RevealInFinder(path);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void NotifyValueChanged<T>(T previousValue, T nextValue, Action<T> onValueChanged)
            where T : Object
        {
            if (!ReferenceEquals(previousValue, nextValue) && onValueChanged != null)
            {
                onValueChanged(nextValue);
            }
        }

        private static void SelectAndPing(Object asset)
        {
            if (asset == null)
            {
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
