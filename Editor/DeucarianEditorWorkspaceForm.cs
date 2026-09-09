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

        public IntegerField Integer(string id, string label, Func<int> read, Action<int> write)
        {
            var field = new IntegerField();
            Bind(id, label, field, read, write);
            return field;
        }

        public Toggle Toggle(string id, string label, Func<bool> read, Action<bool> write)
        {
            var field = new Toggle();
            Bind(id, label, field, read, write);
            return field;
        }

        public ObjectField Asset(string id, string label, Type type, Func<Object> read, Action<Object> write)
        {
            var field = new ObjectField { objectType = type, allowSceneObjects = false };
            Bind(id, label, field, read, write);
            return field;
        }

        public PopupField<string> Choice(string id, string label, IReadOnlyList<string> choices, Func<int> read, Action<int> write)
        {
            var field = new PopupField<string>(new List<string>(choices), 0) { name = id };
            Root.Add(DeucarianEditorWorkspaceControls.Field(label, field));
            synchronizers.Add(() => field.SetValueWithoutNotify(choices[Clamp(read(), choices.Count)]));
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
