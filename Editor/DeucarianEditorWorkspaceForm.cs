using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.Editor
{
    /// <summary>Editor-owned field layout; consumers bind values and commands, not styles.</summary>
    public sealed class DeucarianEditorWorkspaceForm
    {
        private readonly List<Action> synchronizers = new List<Action>();
        private readonly VisualElement primaryActions;
        public VisualElement Root { get; }

        public DeucarianEditorWorkspaceForm(VisualElement root) : this(root, null) { }

        internal DeucarianEditorWorkspaceForm(VisualElement root, VisualElement primaryActions)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            this.primaryActions = primaryActions;
        }

        public TextField Text(string id, string label, Func<string> read, Action<string> write, bool multiline = false)
        {
            var field = new TextField { multiline = multiline };
            if (multiline) field.AddToClassList("dw-multiline");
            Bind(id, label, field, read, write);
            return field;
        }

        public FloatField Number(string id, string label, Func<float> read, Action<float> write)
        {
            var field = new FloatField();
            Bind(id, label, field, read, write);
            return field;
        }

        public DeucarianEditorColorField Color(string id, string label, Func<UnityEngine.Color> read, Action<UnityEngine.Color> write)
        {
            var field = new DeucarianEditorColorField();
            Bind(id, label, field, read, write);
            return field;
        }

        public DoubleField Decimal(string id, string label, Func<double> read, Action<double> write)
        {
            var field = new DoubleField();
            Bind(id, label, field, read, write);
            return field;
        }

        public PopupField<string> Enum<T>(string id, string label, Func<T> read, Action<T> write) where T : struct, System.Enum
        {
            var values = (T[])System.Enum.GetValues(typeof(T));
            return Choice(id, label, Array.ConvertAll(values, value => UnityEditor.ObjectNames.NicifyVariableName(value.ToString())),
                () => Array.IndexOf(values, read()), index => write(values[index]));
        }

        public IntegerField Integer(string id, string label, Func<int> read, Action<int> write)
        {
            var field = new IntegerField();
            Bind(id, label, field, read, write);
            return field;
        }

        public FloatField NumberWithSlider(string id, string label, float dragMinimum, float dragMaximum, Func<float> read, Action<float> write)
        {
            var field = new FloatField();
            BindSlider(id, label, field, new DeucarianEditorSlider(dragMinimum, dragMaximum), read, write);
            return field;
        }

        public IntegerField IntegerWithSlider(string id, string label, int dragMinimum, int dragMaximum, Func<int> read, Action<int> write)
        {
            var field = new IntegerField();
            BindSlider(id, label, field, new DeucarianEditorIntegerSlider(dragMinimum, dragMaximum), read, write);
            return field;
        }

        private void BindSlider<T>(string id, string label, BaseField<T> field, BaseField<T> slider, Func<T> read, Action<T> write)
        {
            var pair = DeucarianEditorWorkspaceControls.Region(null, "dw-range-pair");
            field.name = id; slider.name = id + "-slider";
            field.tooltip = "Enter any value; the slider provides a convenient drag range.";
            pair.Add(slider); pair.Add(field); Root.Add(DeucarianEditorWorkspaceControls.Field(label, pair));
            synchronizers.Add(() => { field.SetValueWithoutNotify(read()); slider.SetValueWithoutNotify(read()); });
            field.RegisterValueChangedCallback(evt => { write(evt.newValue); slider.SetValueWithoutNotify(read()); });
            slider.RegisterValueChangedCallback(evt => { write(evt.newValue); field.SetValueWithoutNotify(read()); });
            Refresh();
        }

        public DeucarianEditorStepper Stepper(string id, string label, int minimum, int maximum, Func<int> read, Action<int> write)
        {
            var field = new DeucarianEditorStepper(minimum, maximum);
            Bind(id, label, field, read, write);
            return field;
        }

        public Toggle Toggle(string id, string label, Func<bool> read, Action<bool> write)
        {
            var field = new DeucarianEditorSwitch();
            Bind(id, label, field, read, write);
            field.parent.AddToClassList("dw-switch-field");
            return field;
        }

        public Vector3Field Vector(string id, string label, Func<UnityEngine.Vector3> read, Action<UnityEngine.Vector3> write)
        {
            var field = new Vector3Field();
            Bind(id, label, field, read, write);
            return field;
        }

        public Slider Slider(string id, string label, float minimum, float maximum, Func<float> read, Action<float> write)
        {
            var field = new DeucarianEditorSlider(minimum, maximum) { showInputField = true };
            Bind(id, label, field, read, write);
            return field;
        }

        public SliderInt IntegerSlider(string id, string label, int minimum, int maximum, Func<int> read, Action<int> write)
        {
            var field = new DeucarianEditorIntegerSlider(minimum, maximum) { showInputField = true };
            Bind(id, label, field, read, write);
            return field;
        }

        public ObjectField Asset(string id, string label, Type type, Func<Object> read, Action<Object> write)
        {
            var field = new ObjectField { objectType = type, allowSceneObjects = false };
            Bind(id, label, field, read, write);
            return field;
        }

        public PopupField<string> Choice(string id, string label, IReadOnlyList<string> choices, Func<int> read, Action<int> write,
            IReadOnlyList<string> icons = null)
        {
            if (choices == null || choices.Count == 0) throw new ArgumentException("At least one choice is required.", nameof(choices));
            if (icons != null && icons.Count != choices.Count) throw new ArgumentException("Icons must correspond to choices.", nameof(icons));
            var field = new PopupField<string>(new List<string>(choices), 0) { name = id };
            Root.Add(DeucarianEditorWorkspaceControls.Field(label, field));
            synchronizers.Add(() => field.SetValueWithoutNotify(choices[Clamp(read(), choices.Count)]));
            if (icons != null)
            {
                var icon = DeucarianEditorWorkspaceControls.Icon(icons[Clamp(read(), choices.Count)]);
                icon.AddToClassList("dw-choice-icon");
                field.Q(className: "unity-base-field__input").Insert(0, icon);
                synchronizers.Add(() => icon.style.backgroundImage = new StyleBackground(DeucarianEditorIcons.GetIcon(icons[Clamp(read(), choices.Count)])));
            }
            field.RegisterValueChangedCallback(_ => write(field.index));
            Refresh();
            return field;
        }

        public DeucarianEditorChoiceBar Segments(string id, string label, IReadOnlyList<string> choices, Func<int> read, Action<int> write)
        {
            var field = new DeucarianEditorChoiceBar(choices, Clamp(read(), choices.Count)) { name = id };
            Root.Add(DeucarianEditorWorkspaceControls.Field(label, field));
            synchronizers.Add(() => field.SetValueWithoutNotify(Clamp(read(), choices.Count)));
            field.Changed += write;
            return field;
        }

        public Button Action(string id, string label, Action execute, Func<bool> enabled = null, bool primary = false)
        {
            var button = DeucarianEditorWorkspaceControls.Button(label, execute, primary);
            button.name = id;
            button.AddToClassList("dw-form-action");
            (primary && primaryActions != null ? primaryActions : Root).Add(button);
            if (enabled != null) synchronizers.Add(() => button.SetEnabled(enabled()));
            Refresh();
            return button;
        }

        public DeucarianEditorWorkspaceForm Section(string title, bool collapsible = false)
        {
            VisualElement container;
            if (collapsible)
            {
                var foldout = new Foldout { text = title, value = false };
                foldout.AddToClassList("dw-foldout");
                container = foldout;
            }
            else
            {
                container = DeucarianEditorWorkspaceControls.Region(null, "dw-form-section");
                container.Add(DeucarianEditorWorkspaceControls.Label(title, "dw-section-title"));
            }
            Root.Add(container);
            var form = new DeucarianEditorWorkspaceForm(container);
            synchronizers.Add(form.Refresh);
            return form;
        }

        public void Note(Func<string> read)
        {
            var label = DeucarianEditorWorkspaceControls.Label(read(), "dw-muted");
            label.AddToClassList("dw-note");
            Root.Add(label);
            synchronizers.Add(() => label.text = read() ?? string.Empty);
        }

        public Label ReadOnly(string id, string caption, Func<string> read)
        {
            var value = DeucarianEditorWorkspaceControls.Label(read(), "dw-readonly");
            value.name = id;
            Root.Add(DeucarianEditorWorkspaceControls.Field(caption, value));
            synchronizers.Add(() => value.text = read() ?? string.Empty);
            return value;
        }

        public void VisibleWhen(VisualElement field, Func<bool> visible)
            => synchronizers.Add(() => field.parent.style.display = visible() ? DisplayStyle.Flex : DisplayStyle.None);

        public void EnabledWhen(Func<bool> enabled) => synchronizers.Add(() =>
        {
            bool active = enabled();
            Root.SetEnabled(active);
            primaryActions?.SetEnabled(active);
        });
        public void Refresh() { foreach (var synchronize in synchronizers) synchronize(); }

        private void Bind<T>(string id, string label, BaseField<T> field, Func<T> read, Action<T> write)
        {
            field.name = id;
            field.SetValueWithoutNotify(read());
            Root.Add(DeucarianEditorWorkspaceControls.Field(label, field));
            synchronizers.Add(() => field.SetValueWithoutNotify(read()));
            field.RegisterValueChangedCallback(evt => write(evt.newValue));
        }

        private static int Clamp(int value, int count) => Math.Max(0, Math.Min(count - 1, value));
    }
}
