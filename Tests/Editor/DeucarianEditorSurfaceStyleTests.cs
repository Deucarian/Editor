using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
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

        [UnityTest]
        public IEnumerator OwnedControlStylesDoNotMutateUnityStyles()
        {
            Exception failure = null;
            bool checkedStyles = false;
            var window = ScriptableObject.CreateInstance<SurfaceStyleTestWindow>();
            window.rootVisualElement.Add(new IMGUIContainer(() =>
            {
                if (checkedStyles) return;
                checkedStyles = true;
                try { AssertOwnedControlStyles(); }
                catch (Exception exception) { failure = exception; }
            }));
            try
            {
                window.Show();
                for (int frame = 0; frame < 10 && !checkedStyles; frame++) yield return null;
                Assert.That(checkedStyles, Is.True, "Inspect native styles during an actual IMGUI draw.");
                if (failure != null) throw failure;
            }
            finally { window.Close(); }
        }

        private static void AssertOwnedControlStyles()
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

        [UnityTest]
        public IEnumerator OperationFooterContainsItsActionsAtSupportedWidths()
        {
            var window = ScriptableObject.CreateInstance<SurfaceStyleTestWindow>();
            var root = window.rootVisualElement;
            root.AddToClassList("deucarian-editor");
            root.AddToClassList(DeucarianEditorTheme.CurrentClass);
            DeucarianEditorUIResources.TryAddSharedStyleSheet(root);
            var footer = DeucarianEditorWorkbenchSurfaces.CreateFooter("", "Ready", "", "Details", null, "1.7.0");
            root.Add(footer.Root);
            try
            {
                window.Show();
                foreach (int width in new[] { 820, 1480 })
                {
                    window.position = new Rect(50, 50, width, 400);
                    for (int frame = 0; frame < 5; frame++) yield return null;
                    Assert.That(footer.Root.resolvedStyle.height, Is.EqualTo(46).Within(.5));
                    Assert.That(footer.Action.resolvedStyle.height, Is.EqualTo(36).Within(.5));
                    Assert.That(footer.Action.worldBound.yMin, Is.GreaterThanOrEqualTo(footer.Root.worldBound.yMin));
                    Assert.That(footer.Action.worldBound.yMax, Is.LessThanOrEqualTo(footer.Root.worldBound.yMax));
                    Assert.That(footer.Action.worldBound.xMax, Is.LessThanOrEqualTo(footer.Root.worldBound.xMax));
                }
            }
            finally { window.Close(); }
        }

        public sealed class SurfaceStyleTestWindow : EditorWindow { }
    }
}
