using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    internal static class DeucarianEditorRangeGUI
    {
        internal static float Slider(Rect rect, float value, float minimum, float maximum, bool integer)
        {
            var styles = DeucarianEditorWorkbenchGUI.InputStyles;
            float numberWidth = Mathf.Min(64, rect.width * .4f);
            var number = new Rect(rect.xMax - numberWidth, rect.y, numberWidth, rect.height);
            var slider = new Rect(rect.x, rect.center.y - 2, Mathf.Max(0, rect.width - numberWidth - 14), 4);
            if (slider.width > 14)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(slider, DeucarianEditorSurfacePalette.Border);
                    var fill = slider;
                    fill.width *= Mathf.InverseLerp(minimum, maximum, value);
                    EditorGUI.DrawRect(fill, GUI.enabled ? DeucarianEditorSurfacePalette.Primary : DeucarianEditorSurfacePalette.Muted);
                }
                value = GUI.HorizontalSlider(slider, value, minimum, maximum, styles.SliderTrack, styles.SliderThumb);
            }
            value = integer ? EditorGUI.IntField(number, Mathf.RoundToInt(value), styles.Text)
                : EditorGUI.FloatField(number, value, styles.Text);
            return Mathf.Clamp(value, minimum, maximum);
        }

        internal static bool Switch(Rect rect, bool value)
        {
            var styles = DeucarianEditorWorkbenchGUI.InputStyles;
            var track = new Rect(rect.x, rect.center.y - 14, 54, 28);
            value = GUI.Toggle(track, value, GUIContent.none, styles.Switch);
            if (Event.current.type == EventType.Repaint)
                GUI.DrawTexture(new Rect(track.x + (value ? 29 : 3), track.y + 3, 22, 22), styles.SliderThumb.normal.background);
            return value;
        }
    }
}
