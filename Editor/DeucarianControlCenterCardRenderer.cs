using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterCardRenderer
    {
        internal static VisualElement Create(
            DeucarianControlCenterCard card,
            string focusedTargetId,
            Action<DeucarianControlCenterAction> execute)
        {
            var result = new VisualElement
            {
                name = "control-center-card-" + card.Id
            };
            DeucarianControlCenterVisuals.StylePanel(result);
            result.style.flexGrow = 1f;
            result.style.flexBasis = 280f;
            result.style.minWidth = 0f;
            result.style.marginRight = 6f;
            result.style.marginBottom = 6f;
            result.style.paddingLeft = 0f;
            result.style.paddingRight = 16f;
            result.style.paddingTop = 10f;
            result.style.paddingBottom = 10f;
            if (string.Equals(card.Id, focusedTargetId, StringComparison.Ordinal))
            {
                DeucarianControlCenterVisuals.SetBorderColor(result, DeucarianEditorTheme.Accent);
                result.style.borderLeftWidth = 3f;
            }

            VisualElement heading = new VisualElement();
            heading.AddToClassList("dw-card-heading");
            heading.style.flexDirection = FlexDirection.Row;
            heading.style.flexWrap = Wrap.Wrap;
            heading.style.justifyContent = Justify.SpaceBetween;
            heading.Add(DeucarianControlCenterVisuals.CreateLabel(card.Title, true));
            if (card.StatusText.Length > 0)
            {
                Label badge = DeucarianControlCenterVisuals.CreateLabel(card.StatusText, false);
                badge.AddToClassList("dw-card-status--" + card.Status.ToString().ToLowerInvariant());
                heading.Add(badge);
            }

            result.Add(heading);
            if (card.Description.Length > 0)
            {
                result.Add(DeucarianControlCenterVisuals.CreateMutedLabel(card.Description));
            }

            foreach (string detail in card.Details)
            {
                result.Add(DeucarianControlCenterVisuals.CreateMutedLabel("• " + detail));
            }

            if (card.Actions.Count > 0)
            {
                var actions = new VisualElement();
                actions.style.flexDirection = FlexDirection.Row;
                actions.style.flexWrap = Wrap.Wrap;
                actions.style.flexGrow = 1f;
                actions.style.alignItems = Align.FlexEnd;
                actions.style.marginTop = 8f;
                foreach (DeucarianControlCenterAction action in card.Actions)
                {
                    DeucarianControlCenterAction captured = action;
                    var button = DeucarianEditorWorkspaceControls.Button(action.Label, () => execute(captured));
                    button.name = "control-center-action-" + action.Id;
                    button.style.marginRight = 5f;
                    actions.Add(button);
                }

                result.Add(actions);
            }

            return result;
        }
    }
}
