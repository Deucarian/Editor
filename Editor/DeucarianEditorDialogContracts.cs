using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public enum DeucarianEditorDialogActionStyle
    {
        Primary,
        Secondary,
        Destructive
    }

    public enum DeucarianEditorDialogCompletionReason
    {
        Action,
        Escape,
        WindowClosed
    }

    /// <summary>Immutable action descriptor for a shared editor dialog.</summary>
    public sealed class DeucarianEditorDialogAction
    {
        public DeucarianEditorDialogAction(
            string id,
            string label,
            string iconId,
            DeucarianEditorDialogActionStyle style = DeucarianEditorDialogActionStyle.Secondary)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A dialog action requires a stable ID.", nameof(id));
            }

            Id = id.Trim();
            Label = string.IsNullOrWhiteSpace(label) ? Id : label.Trim();
            IconId = string.IsNullOrWhiteSpace(iconId) ? DeucarianEditorIconIds.Package : iconId.Trim();
            Style = Enum.IsDefined(typeof(DeucarianEditorDialogActionStyle), style)
                ? style
                : DeucarianEditorDialogActionStyle.Secondary;
        }

        public string Id { get; }

        public string Label { get; }

        public string IconId { get; }

        public DeucarianEditorDialogActionStyle Style { get; }
    }

    /// <summary>Content and keyboard behavior for a shared editor dialog.</summary>
    public sealed class DeucarianEditorDialogOptions
    {
        public DeucarianEditorDialogOptions(
            string title,
            string message,
            string iconId,
            IReadOnlyList<DeucarianEditorDialogAction> actions)
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Deucarian" : title.Trim();
            Message = message ?? string.Empty;
            IconId = string.IsNullOrWhiteSpace(iconId) ? DeucarianEditorIconIds.Info : iconId.Trim();
            Actions = actions ?? Array.Empty<DeucarianEditorDialogAction>();
        }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Details { get; set; }

        public string IconId { get; set; }

        public IReadOnlyList<DeucarianEditorDialogAction> Actions { get; set; }

        public string DefaultActionId { get; set; }

        public string CancelActionId { get; set; }
    }

    public readonly struct DeucarianEditorDialogResult
    {
        public DeucarianEditorDialogResult(
            string actionId,
            DeucarianEditorDialogCompletionReason reason,
            bool wasCanceled)
        {
            ActionId = actionId ?? string.Empty;
            Reason = reason;
            WasCanceled = wasCanceled;
        }

        public string ActionId { get; }

        public DeucarianEditorDialogCompletionReason Reason { get; }

        public bool WasCanceled { get; }
    }
}
