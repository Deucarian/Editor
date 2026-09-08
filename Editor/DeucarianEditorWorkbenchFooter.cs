using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorWorkbenchFooter
    {
        internal DeucarianEditorWorkbenchFooter(
            VisualElement root,
            VisualElement status,
            VisualElement statusContent,
            Label statusIcon,
            Image statusImage,
            VisualElement statusGap,
            Label statusLabel,
            Label summary,
            VisualElement spacer,
            VisualElement actions,
            Button action,
            Label version)
        {
            Root = root;
            Status = status;
            StatusContent = statusContent;
            StatusIcon = statusIcon;
            StatusImage = statusImage;
            StatusGap = statusGap;
            StatusLabel = statusLabel;
            Summary = summary;
            Spacer = spacer;
            Actions = actions;
            Action = action;
            Version = version;
        }

        public VisualElement Root { get; }
        public VisualElement Status { get; }
        public VisualElement StatusContent { get; }
        public Label StatusIcon { get; }
        public Image StatusImage { get; }
        public VisualElement StatusGap { get; }
        public Label StatusLabel { get; }
        public Label Summary { get; }
        public VisualElement Spacer { get; }
        public VisualElement Actions { get; }
        public Button Action { get; }
        public Label Version { get; }
    }
}
