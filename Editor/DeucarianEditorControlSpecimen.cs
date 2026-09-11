using UnityEngine;
using UnityEngine.UIElements;
using Ui = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.Editor
{
    /// <summary>A detached controls specimen shared by spatial, theme and component editor previews.</summary>
    public sealed class DeucarianEditorControlSpecimen : VisualElement
    {
        private readonly VisualElement menu;
        private readonly Button[] items;
        private Color? accent;

        public DeucarianEditorControlSpecimen()
        {
            name = "control-specimen";
            AddToClassList("dw-control-specimen");
            var grid = new DeucarianEditorSpatialPreview(false);
            grid.AddToClassList("dw-control-grid");
            grid.pickingMode = PickingMode.Ignore;
            Add(grid);
            menu = Ui.Panel("control-specimen-menu");
            var home = Ui.IconButton("Home", DeucarianEditorIconIds.Home, () => Select(0));
            var settings = Ui.IconButton("Settings", DeucarianEditorIconIds.Settings, () => Select(1));
            var profile = Ui.IconButton("Profile", DeucarianEditorIconIds.Users, () => Select(2));
            items = new[] { home, settings, profile };
            foreach (var item in items) menu.Add(item);
            Add(menu);
            var sliderPanel = Ui.Panel("control-specimen-slider");
            sliderPanel.Add(new DeucarianEditorSlider(0, 1) { value = 0.5f });
            Add(sliderPanel);
            var select = Ui.IconButton("Select", DeucarianEditorIconIds.Optional, () => Select(0));
            select.AddToClassList("dw-specimen-select");
            Add(select);
            tooltip = "Interactive editor specimen · no scene objects or application actions";
            Select(0);
        }

        private void Select(int index)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i].EnableInClassList("dw-selected", i == index);
                if (accent.HasValue)
                {
                    Color border = i == index ? accent.Value : Color.clear;
                    items[i].style.borderLeftColor = items[i].style.borderRightColor = items[i].style.borderTopColor = items[i].style.borderBottomColor = border;
                }
            }
        }

        public void SetColors(Color surface, Color highlight, Color text)
        {
            accent = highlight;
            foreach (var panel in this.Query(className: "dw-panel").ToList()) panel.style.backgroundColor = surface;
            foreach (var label in this.Query<Label>().ToList()) label.style.color = text;
            foreach (var icon in this.Query(className: "dw-icon").ToList()) icon.style.unityBackgroundImageTintColor = text;
            this.Q("slider-fill").style.backgroundColor = highlight;
            for (int i = 0; i < items.Length; i++) if (items[i].ClassListContains("dw-selected")) { Select(i); break; }
        }
    }
}
