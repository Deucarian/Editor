using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorSurfaceStyleTests
    {
        [Test]
        public void SharedSurfaceColorsMatchWorkspaceAndLegacyStyleSheets()
        {
            string workspace = File.ReadAllText(DeucarianEditorWorkspace.StyleSheetPath);
            string shared = File.ReadAllText(DeucarianEditorUIResources.StylesPath + "/DeucarianEditor.uss");
            foreach (var color in new[] { DeucarianEditorSurfacePalette.Background, DeucarianEditorSurfacePalette.Field,
                DeucarianEditorSurfacePalette.Text, DeucarianEditorSurfacePalette.Muted,
                DeucarianEditorSurfacePalette.Accent, DeucarianEditorSurfacePalette.Border })
            {
                string hex = "#" + ColorUtility.ToHtmlStringRGB(color).ToLowerInvariant();
                StringAssert.Contains(hex, workspace);
                StringAssert.Contains(hex, shared);
            }
            Assert.That(DeucarianEditorVisualShell.DeepBackground, Is.EqualTo(DeucarianEditorSurfacePalette.Background));
            Assert.That(DeucarianEditorColors.BodyText, Is.EqualTo(DeucarianEditorSurfacePalette.Text));
        }

        [Test]
        public void OwnedControlStylesDoNotMutateUnityStyles()
        {
            int textSize = EditorStyles.textField.fontSize;
            int buttonSize = EditorStyles.miniButton.fontSize;
            Color textColor = EditorStyles.textField.normal.textColor;
            var input = DeucarianEditorWorkbenchGUI.InputStyles;
            Assert.That(input.Text, Is.Not.SameAs(EditorStyles.textField));
            Assert.That(input.Text.fontSize, Is.EqualTo(16));
            Assert.That(input.NativeCaption.fontSize, Is.EqualTo(EditorStyles.miniLabel.fontSize));
            Assert.That(DeucarianEditorButtons.SecondaryStyle.fixedHeight, Is.EqualTo(36));
            Assert.That(DeucarianEditorButtons.PrimaryStyle.fixedHeight, Is.EqualTo(42));
            Assert.That(DeucarianEditorButtons.PrimaryStyle.normal.background,
                Is.Not.SameAs(DeucarianEditorButtons.SecondaryStyle.normal.background));
            Assert.That(EditorStyles.textField.fontSize, Is.EqualTo(textSize));
            Assert.That(EditorStyles.textField.normal.textColor, Is.EqualTo(textColor));
            Assert.That(EditorStyles.miniButton.fontSize, Is.EqualTo(buttonSize));
        }

        [Test]
        public void InspectorCompositionIsLazyAndDoesNotAddWindowNavigation()
        {
            int draws = 0;
            var root = DeucarianEditorInspector.Create(() => draws++);
            Assert.That(root.ClassListContains("deucarian-inspector"), Is.True);
            Assert.That(root.Q<IMGUIContainer>("deucarian-inspector-content"), Is.Not.Null);
            Assert.That(root.Q("workspace-navigation"), Is.Null);
            Assert.That(draws, Is.Zero, "Constructing an inspector must not perform its actions.");
            Assert.Throws<ArgumentNullException>(() => DeucarianEditorInspector.Create(null));
        }
    }
}
