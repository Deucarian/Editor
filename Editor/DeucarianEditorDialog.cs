using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Opens a branded, responsive utility dialog and completes exactly once.</summary>
    public static class DeucarianEditorDialog
    {
        public const float NarrowWidth = 440f;
        public const float MinimumWidth = 360f;
        public const float MaximumWidth = 760f;
        public const float MinimumHeight = 220f;
        public const float MaximumHeight = 680f;

        public static EditorWindow Show(
            DeucarianEditorDialogOptions options,
            Action<DeucarianEditorDialogResult> completed)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            EditorWindow owner = EditorWindow.focusedWindow;
            DeucarianEditorDialogWindow window =
                ScriptableObject.CreateInstance<DeucarianEditorDialogWindow>();
            window.Initialize(options, completed);
            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.maxSize = new Vector2(MaximumWidth, MaximumHeight);
            window.position = CenterOnOwner(owner, CalculateRecommendedSize(options));
            window.ShowUtility();
            return window;
        }

        public static Vector2 CalculateRecommendedSize(DeucarianEditorDialogOptions options)
        {
            if (options == null)
            {
                return new Vector2(500f, 280f);
            }

            int titleLength = (options.Title ?? string.Empty).Length;
            int messageLength = (options.Message ?? string.Empty).Length;
            int detailsLength = (options.Details ?? string.Empty).Length;
            int actionCharacters = (options.Actions ?? Array.Empty<DeucarianEditorDialogAction>())
                .Where(action => action != null)
                .Sum(action => action.Label.Length + 6);
            float width = Mathf.Clamp(
                420f + Mathf.Max(titleLength - 30, 0) * 2.4f + Mathf.Max(actionCharacters - 42, 0) * 1.6f,
                MinimumWidth,
                MaximumWidth);
            float contentWidth = Mathf.Max(34f, width - 128f);
            int charactersPerLine = Mathf.Max(24, Mathf.FloorToInt(contentWidth / 7f));
            int messageLines = EstimateWrappedLineCount(options.Message, charactersPerLine);
            int detailLines = EstimateWrappedLineCount(options.Details, charactersPerLine);
            float height = 174f + messageLines * 18f + Mathf.Min(280f, detailLines * 15f);
            if (detailsLength > 0)
            {
                height += 28f;
            }
            else if (messageLength == 0)
            {
                height += 18f;
            }

            return new Vector2(width, Mathf.Clamp(height, MinimumHeight, MaximumHeight));
        }

        private static int EstimateWrappedLineCount(string value, int charactersPerLine)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int lineCount = 0;
            string[] explicitLines = value.Replace("\r\n", "\n").Split('\n');
            foreach (string line in explicitLines)
            {
                lineCount += Mathf.Max(1, Mathf.CeilToInt(line.Length / (float)charactersPerLine));
            }

            return lineCount;
        }

        private static Rect CenterOnOwner(EditorWindow owner, Vector2 size)
        {
            Rect ownerRect = owner != null
                ? owner.position
                : new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height);
            return new Rect(
                ownerRect.center.x - size.x * 0.5f,
                ownerRect.center.y - size.y * 0.5f,
                size.x,
                size.y);
        }
    }

    internal sealed class DeucarianEditorDialogWindow : EditorWindow
    {
        internal const string RootClass = "deucarian-dialog";
        internal const string NarrowClass = "deucarian-dialog--narrow";
        internal const string IconName = "deucarian-dialog-icon";
        internal const string MessageName = "deucarian-dialog-message";
        internal const string DetailsName = "deucarian-dialog-details";
        internal const string ActionsName = "deucarian-dialog-actions";

        private DeucarianEditorDialogOptions _options;
        private DeucarianEditorDialogAction[] _actions = Array.Empty<DeucarianEditorDialogAction>();
        private Action<DeucarianEditorDialogResult> _completedCallback;
        private bool _completed;

        internal void Initialize(
            DeucarianEditorDialogOptions options,
            Action<DeucarianEditorDialogResult> completed)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _actions = NormalizeActions(options.Actions);
            _completedCallback = completed;
            titleContent = new GUIContent(
                string.IsNullOrWhiteSpace(options.Title) ? "Deucarian" : options.Title.Trim(),
                DeucarianEditorIcons.GetIcon(options.IconId));
        }

        public void CreateGUI()
        {
            if (_options == null)
            {
                return;
            }

            VisualElement root = rootVisualElement;
            VisualElement content = DeucarianEditorVisualShell.CreateWindowShell(root);
            root.AddToClassList(RootClass);
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            VisualElement card = DeucarianEditorVisualShell.CreatePanel("deucarian-dialog__card");
            content.Add(card);

            VisualElement body = new VisualElement { name = "deucarian-dialog-body" };
            body.AddToClassList("deucarian-dialog__body");
            card.Add(body);

            var icon = new Image
            {
                name = IconName,
                image = DeucarianEditorIcons.GetIcon(_options.IconId),
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            icon.AddToClassList("deucarian-dialog__icon");
            body.Add(icon);

            var copyScroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "deucarian-dialog-copy-scroll"
            };
            copyScroll.AddToClassList("deucarian-dialog__copy-scroll");
            body.Add(copyScroll);

            VisualElement copy = new VisualElement { name = "deucarian-dialog-copy" };
            copy.AddToClassList("deucarian-dialog__copy");
            copyScroll.Add(copy);

            var title = new Label(string.IsNullOrWhiteSpace(_options.Title) ? "Deucarian" : _options.Title.Trim())
            {
                name = "deucarian-dialog-title",
                pickingMode = PickingMode.Ignore
            };
            title.AddToClassList("deucarian-dialog__title");
            copy.Add(title);

            var message = new Label(_options.Message ?? string.Empty)
            {
                name = MessageName,
                pickingMode = PickingMode.Ignore
            };
            message.AddToClassList("deucarian-dialog__message");
            copy.Add(message);

            if (!string.IsNullOrWhiteSpace(_options.Details))
            {
                var detailsPanel = new VisualElement
                {
                    name = DetailsName
                };
                detailsPanel.AddToClassList("deucarian-dialog__details");
                var details = new Label(_options.Details.Trim())
                {
                    name = "deucarian-dialog-details-text",
                    pickingMode = PickingMode.Ignore
                };
                details.AddToClassList("deucarian-dialog__details-text");
                detailsPanel.Add(details);
                copy.Add(detailsPanel);
            }

            VisualElement actions = new VisualElement { name = ActionsName };
            actions.AddToClassList("deucarian-dialog__actions");
            card.Add(actions);

            foreach (DeucarianEditorDialogAction action in _actions)
            {
                DeucarianEditorDialogAction capturedAction = action;
                Button button = DeucarianEditorIconTextButton.Create(
                    action.IconId,
                    action.Label,
                    () => CompleteAction(capturedAction),
                    action.Label,
                    false);
                button.name = "deucarian-dialog-action-" + action.Id;
                button.userData = action.Id;
                button.AddToClassList("deucarian-dialog__action");
                button.AddToClassList(GetActionStyleClass(action.Style));
                ConfigureActionForArbitraryLabel(button);
                actions.Add(button);
            }

            root.schedule.Execute(() =>
            {
                UpdateResponsiveClass(root.resolvedStyle.width);
                FindDefaultButton()?.Focus();
            });
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateResponsiveClass(evt.newRect.width);
        }

        private void UpdateResponsiveClass(float width)
        {
            rootVisualElement.EnableInClassList(
                NarrowClass,
                width > 0f && width < DeucarianEditorDialog.NarrowWidth);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                DeucarianEditorDialogAction action = FindAction(_options.DefaultActionId) ??
                    _actions.FirstOrDefault(candidate => candidate.Style == DeucarianEditorDialogActionStyle.Primary) ??
                    _actions.FirstOrDefault();
                if (action != null)
                {
                    evt.StopImmediatePropagation();
                    CompleteAction(action);
                }
                return;
            }

            if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopImmediatePropagation();
                DeucarianEditorDialogAction cancelAction = FindAction(_options.CancelActionId);
                Complete(
                    cancelAction != null ? cancelAction.Id : string.Empty,
                    DeucarianEditorDialogCompletionReason.Escape,
                    true,
                    true);
            }
        }

        private Button FindDefaultButton()
        {
            DeucarianEditorDialogAction action = FindAction(_options.DefaultActionId) ??
                _actions.FirstOrDefault(candidate => candidate.Style == DeucarianEditorDialogActionStyle.Primary) ??
                _actions.FirstOrDefault();
            return action == null
                ? null
                : rootVisualElement.Q<Button>("deucarian-dialog-action-" + action.Id);
        }

        private DeucarianEditorDialogAction FindAction(string actionId)
        {
            return string.IsNullOrWhiteSpace(actionId)
                ? null
                : _actions.FirstOrDefault(action =>
                    string.Equals(action.Id, actionId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private void CompleteAction(DeucarianEditorDialogAction action)
        {
            if (action == null)
            {
                return;
            }

            Complete(
                action.Id,
                DeucarianEditorDialogCompletionReason.Action,
                !string.IsNullOrWhiteSpace(_options.CancelActionId) &&
                string.Equals(action.Id, _options.CancelActionId, StringComparison.OrdinalIgnoreCase),
                true);
        }

        private static void ConfigureActionForArbitraryLabel(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.style.height = StyleKeyword.Auto;
            button.style.minHeight = DeucarianEditorLayoutMetrics.CommandControlHeight;
            button.style.maxHeight = StyleKeyword.None;
            button.style.maxWidth = Length.Percent(100f);
            button.style.flexShrink = 1f;
            button.style.paddingTop = 5f;
            button.style.paddingBottom = 5f;

            VisualElement content = button.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.ContentClass);
            if (content != null)
            {
                content.style.height = StyleKeyword.Auto;
                content.style.minHeight = DeucarianEditorIconTextButton.TextHeight;
                content.style.maxHeight = StyleKeyword.None;
                content.style.flexGrow = 1f;
                content.style.flexShrink = 1f;
            }

            Label label = button.Q<Label>(
                className: DeucarianEditorIconTextButton.LabelClass);
            if (label != null)
            {
                label.style.height = StyleKeyword.Auto;
                label.style.minHeight = DeucarianEditorIconTextButton.TextHeight;
                label.style.maxHeight = StyleKeyword.None;
                label.style.flexGrow = 1f;
                label.style.flexShrink = 1f;
                label.style.whiteSpace = WhiteSpace.Normal;
                label.style.overflow = Overflow.Hidden;
                label.style.textOverflow = TextOverflow.Ellipsis;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
        }

        private void Complete(
            string actionId,
            DeucarianEditorDialogCompletionReason reason,
            bool wasCanceled,
            bool closeWindow)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            Action<DeucarianEditorDialogResult> callback = _completedCallback;
            _completedCallback = null;

            try
            {
                callback?.Invoke(new DeucarianEditorDialogResult(actionId, reason, wasCanceled));
            }
            finally
            {
                if (closeWindow)
                {
                    Close();
                }
            }
        }

        private void OnDestroy()
        {
            DeucarianEditorDialogAction cancelAction = _options == null
                ? null
                : FindAction(_options.CancelActionId);
            Complete(
                cancelAction != null ? cancelAction.Id : string.Empty,
                DeucarianEditorDialogCompletionReason.WindowClosed,
                true,
                false);
        }

        private static DeucarianEditorDialogAction[] NormalizeActions(
            IReadOnlyList<DeucarianEditorDialogAction> actions)
        {
            DeucarianEditorDialogAction[] normalized = (actions ?? Array.Empty<DeucarianEditorDialogAction>())
                .Where(action => action != null)
                .GroupBy(action => action.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();
            return normalized.Length > 0
                ? normalized
                : new[]
                {
                    new DeucarianEditorDialogAction(
                        "close",
                        "Close",
                        DeucarianEditorIconIds.Clear,
                        DeucarianEditorDialogActionStyle.Primary)
                };
        }

        private static string GetActionStyleClass(DeucarianEditorDialogActionStyle style)
        {
            switch (style)
            {
                case DeucarianEditorDialogActionStyle.Primary:
                    return "deucarian-dialog__action--primary";
                case DeucarianEditorDialogActionStyle.Destructive:
                    return "deucarian-dialog__action--destructive";
                default:
                    return "deucarian-dialog__action--secondary";
            }
        }
    }
}
