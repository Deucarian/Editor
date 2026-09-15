using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>One color value, with a native picker and an editable hexadecimal representation.</summary>
    public sealed class DeucarianEditorColorField : BaseField<Color>
    {
        private readonly ColorField swatch;
        private readonly TextField hexadecimal;

        public DeucarianEditorColorField() : base(null, new VisualElement())
        {
            AddToClassList("dw-color-field");
            var input = this.Q(className: inputUssClassName);
            swatch = new ColorField { showEyeDropper = true, showAlpha = true };
            swatch.AddToClassList("dw-color-swatch");
            hexadecimal = new TextField { isDelayed = true, tooltip = "Hex color: #RRGGBB or #RRGGBBAA" };
            hexadecimal.AddToClassList("dw-color-hex");
            input.Add(swatch); input.Add(hexadecimal);
            swatch.RegisterValueChangedCallback(evt => { evt.StopPropagation(); value = evt.newValue; });
            hexadecimal.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                string text = (evt.newValue ?? "").Trim();
                if (!text.StartsWith("#")) text = "#" + text;
                if (ColorUtility.TryParseHtmlString(text, out var parsed)) value = parsed;
                else hexadecimal.SetValueWithoutNotify(Format(value));
            });
            SetValueWithoutNotify(Color.white);
        }

        public override void SetValueWithoutNotify(Color newValue)
        {
            base.SetValueWithoutNotify(newValue);
            swatch?.SetValueWithoutNotify(newValue);
            hexadecimal?.SetValueWithoutNotify(Format(newValue));
        }

        private static string Format(Color color) => "#" + (color.a >= 1 ? ColorUtility.ToHtmlStringRGB(color) : ColorUtility.ToHtmlStringRGBA(color));
    }
}
