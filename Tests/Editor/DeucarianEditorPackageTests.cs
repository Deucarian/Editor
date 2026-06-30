using System;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorPackageTests
    {
        private static readonly string[] KnownPackageKeys =
        {
            "package-installer",
            "theming",
            "logging",
            "object-loading",
            "api-helper",
            "session",
            "selection",
            "generic-ui-items",
            "editor"
        };

        [Test]
        public void PackageConstants_AreCorrect()
        {
            Assert.AreEqual("com.deucarian.editor", DeucarianEditorPackageConstants.PackageName);
            Assert.AreEqual("Deucarian Editor", DeucarianEditorPackageConstants.DisplayName);
            Assert.AreEqual("1.0.0", DeucarianEditorPackageConstants.Version);
            Assert.AreEqual("Tools/Deucarian", DeucarianEditorPackageConstants.MenuRoot);
            Assert.AreEqual("Tools/Deucarian", DeucarianEditorPackageConstants.PackageToolMenuRoot);
        }

        [Test]
        public void PackageIconLookup_DoesNotThrowForKnownKeys()
        {
            foreach (string key in KnownPackageKeys)
            {
                Texture2D icon = null;
                Assert.DoesNotThrow(() => icon = DeucarianEditorIcons.GetPackageIcon(key));
                Assert.NotNull(icon, key);
                Assert.IsTrue(DeucarianEditorIcons.IsKnownPackageKey(key), key);
            }
        }

        [Test]
        public void FallbackIconAndContent_ReturnNonNull()
        {
            Assert.NotNull(DeucarianEditorIcons.GetFallbackIcon("Missing"));
            Assert.NotNull(DeucarianEditorIcons.GetPackageContent("missing-package", "Missing", "Tooltip"));
        }

        [Test]
        public void StyleAccessors_DoNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                Assert.NotNull(DeucarianEditorStyles.PackageHeaderTitle);
                Assert.NotNull(DeucarianEditorStyles.PackageHeaderSubtitle);
                Assert.NotNull(DeucarianEditorStyles.SectionTitle);
                Assert.NotNull(DeucarianEditorStyles.SectionBox);
                Assert.NotNull(DeucarianEditorStyles.MutedLabel);
                Assert.NotNull(DeucarianEditorStyles.StatusBadge);
                Assert.NotNull(DeucarianEditorStyles.ToolbarButton);
                Assert.NotNull(DeucarianEditorStyles.FooterVersionText);
            });
        }

        [Test]
        public void UiToolkitResourceHelpers_AreAvailable()
        {
            Assert.AreEqual(
                "Packages/com.deucarian.editor/Editor/Assets/Styles/DeucarianEditor.uss",
                DeucarianEditorUIResources.SharedStyleSheetPath);
            Assert.AreEqual(
                "Packages/com.deucarian.editor/Editor/Assets/Logos/DeucarianPlaceholderLogo.png",
                DeucarianEditorUIResources.PlaceholderLogoPath);
            Assert.AreEqual(
                "Packages/com.deucarian.editor/Editor/Assets/Images/DeucarianInstallerBackground.png",
                DeucarianEditorUIResources.InstallerBackgroundPath);
            Assert.AreEqual(
                "Packages/com.deucarian.editor/Editor/Assets/Images/DeucarianPackageInstallerPlaceholderHero.png",
                DeucarianEditorUIResources.PackageInstallerPlaceholderHeroPath);
            Assert.AreEqual(
                "Packages/com.deucarian.editor/Editor/Assets/Icons/DeucarianPackagePlaceholderIcon.png",
                DeucarianEditorUIResources.PackagePlaceholderIconPath);
            Assert.NotNull(typeof(DeucarianEditorUIResources).GetMethod("LoadAsset"));
            Assert.NotNull(typeof(DeucarianEditorUIResources).GetMethod("TryAddSharedStyleSheet"));
        }

        [Test]
        public void VisualShellHelpers_AreAvailable()
        {
            Assert.AreEqual(0.72f, DeucarianEditorVisualShell.MainPanel.a, 0.001f);
            Assert.AreEqual(0.62f, DeucarianEditorVisualShell.NestedSurface.a, 0.001f);
            Assert.AreEqual(0.68f, DeucarianEditorVisualShell.HeaderPanel.a, 0.001f);
            Assert.NotNull(typeof(DeucarianEditorVisualShell).GetMethod("CreateWindowShell"));
            Assert.NotNull(typeof(DeucarianEditorVisualShell).GetMethod("DrawFrostedSurface"));
            Assert.NotNull(typeof(DeucarianEditorVisualShell).GetMethod("DrawInsetSurface"));
            Assert.NotNull(typeof(DeucarianEditorTheme).GetProperty("GlassPanel"));
            Assert.NotNull(typeof(DeucarianEditorSpacing).GetField("SidebarWidth"));
            Assert.NotNull(typeof(DeucarianEditorTextures).GetMethod("Background"));
            Assert.NotNull(typeof(DeucarianEditorWindowChrome).GetMethod("ConfigureFixedWallpaper"));
            Assert.NotNull(typeof(DeucarianEditorCards).GetMethod("DrawCard"));
            Assert.NotNull(typeof(DeucarianEditorSidebar).GetMethod("DrawItem"));
            Assert.NotNull(typeof(DeucarianEditorButtons).GetMethod("Primary"));
            Assert.NotNull(typeof(DeucarianEditorStatusPanel).GetMethod("DrawStatusBar"));
        }

        [Test]
        public void AmbientGlassHelpers_AreOwnedByEditor()
        {
            Assert.AreEqual("deucarian-ambient-lighting-layer", DeucarianEditorAmbientGlass.AmbientLayerName);
            Assert.AreEqual("deucarian-fixed-wallpaper-layer", DeucarianEditorWindowChrome.BackgroundLayerClass);
            Assert.AreEqual("deucarian-readability-overlay", DeucarianEditorWindowChrome.ReadabilityOverlayClass);

            try
            {
                DeucarianEditorAmbientMotionSettings.SetModeForTests(DeucarianEditorAmbientMotionMode.On);
                Assert.AreEqual(1f, DeucarianEditorAmbientMotionSettings.MotionScale);

                DeucarianEditorAmbientMotionSettings.SetModeForTests(DeucarianEditorAmbientMotionMode.Reduced);
                Assert.That(DeucarianEditorAmbientMotionSettings.MotionScale, Is.GreaterThan(0f).And.LessThan(1f));

                DeucarianEditorAmbientMotionSettings.SetModeForTests(DeucarianEditorAmbientMotionMode.Off);
                Assert.AreEqual(0f, DeucarianEditorAmbientMotionSettings.MotionScale);
            }
            finally
            {
                DeucarianEditorAmbientMotionSettings.SetModeForTests(null);
            }
        }

        [Test]
        public void StatusBadgeHelpers_DoNotThrow()
        {
            foreach (DeucarianEditorStatus status in Enum.GetValues(typeof(DeucarianEditorStatus)))
            {
                Assert.DoesNotThrow(() => DeucarianEditorStatusBadge.GetColor(status));
                Assert.DoesNotThrow(() => DeucarianEditorStatusBadge.GetContent(status.ToString(), status));
                Assert.DoesNotThrow(() => DeucarianEditorStatusBadge.CreateStyle(status));
                Assert.IsTrue(DeucarianEditorStatusBadge.IsValid(status));
            }

            Assert.NotNull(typeof(DeucarianEditorStatusBadge).GetMethod(
                "Draw",
                new[] { typeof(Rect), typeof(string), typeof(DeucarianEditorStatus), typeof(GUIStyle) }));
            Assert.NotNull(typeof(DeucarianEditorStatusBadge).GetMethod(
                "Draw",
                new[] { typeof(Rect), typeof(GUIContent), typeof(DeucarianEditorStatus), typeof(GUIStyle) }));
        }

        [Test]
        public void AssetFieldHelperApi_IsAvailable()
        {
            Assert.NotNull(typeof(DeucarianEditorFields).GetMethod("DrawAssetFieldWithSelectButton"));
            Assert.NotNull(typeof(DeucarianEditorAccordion).GetMethod("DrawFoldoutCard"));
            Assert.NotNull(typeof(DeucarianEditorFoldoutCard).GetMethod("Draw"));
            Assert.NotNull(typeof(DeucarianEditorFieldRow).GetMethod("TextField"));
            Assert.NotNull(typeof(DeucarianEditorObjectFieldRow).GetMethod("Draw"));
            Assert.NotNull(typeof(DeucarianEditorMiniToolbar).GetMethod("Button"));
        }

        [Test]
        public void UxStandards_BuildExpectedMenuPath()
        {
            Assert.AreEqual(
                "Tools/Deucarian/Theming/Open Theme Manager",
                DeucarianEditorUxStandards.GetPackageMenuPath("Theming", "Open Theme Manager"));
        }

        [Test]
        public void AccordionStateKeys_AreStableAndSanitized()
        {
            string first = DeucarianEditorAccordion.BuildStateKey("Attack", "attack.example.fire-orb", "Status Effects");
            string second = DeucarianEditorAccordion.BuildStateKey(" Attack ", "attack.example.fire-orb", "Status Effects");

            Assert.AreEqual(first, second);
            Assert.AreEqual("status_effects", DeucarianEditorAccordion.NormalizeSegmentForTests("Status Effects"));
            StringAssert.StartsWith("Deucarian.Editor.Accordion.", first);
        }

        [Test]
        public void ResponsiveLayout_ComputesExpectedBreakpoints()
        {
            DeucarianEditorResponsiveLayoutState wide = DeucarianEditorResponsiveLayout.Calculate(1400f, 800f);
            DeucarianEditorResponsiveLayoutState medium = DeucarianEditorResponsiveLayout.Calculate(940f, 800f);
            DeucarianEditorResponsiveLayoutState narrow = DeucarianEditorResponsiveLayout.Calculate(640f, 800f);

            Assert.IsTrue(wide.Wide);
            Assert.IsFalse(wide.PreviewStacked);
            Assert.IsFalse(medium.Wide);
            Assert.IsTrue(medium.PreviewStacked);
            Assert.IsTrue(narrow.Narrow);
            Assert.Less(narrow.SidebarWidth, DeucarianEditorSpacing.SidebarWidth);
        }

        [Test]
        public void WorkflowControlHelpers_AreAvailable()
        {
            Assert.NotNull(typeof(DeucarianEditorSearchField).GetMethod("Draw"));
            Assert.NotNull(typeof(DeucarianEditorSegmentedControl).GetMethod("DrawPageChips"));
            Assert.NotNull(typeof(DeucarianEditorStatusChipRow).GetMethod("Draw", new[] { typeof(DeucarianEditorStatusChip[]) }));
            Assert.NotNull(typeof(DeucarianEditorIconToolbar).GetMethod("BuildContent"));
            Assert.NotNull(typeof(DeucarianEditorSplitPane).GetMethod("Calculate"));
            Assert.NotNull(typeof(DeucarianEditorWizardHeader).GetMethod("Draw"));
            Assert.NotNull(typeof(DeucarianEditorEventTimeline).GetMethod("Draw"));
            Assert.NotNull(typeof(DeucarianEditorCompactObjectCard).GetMethod("Draw"));
            Assert.NotNull(typeof(DeucarianEditorPreviewLabChrome).GetMethod("Begin"));
            Assert.NotNull(typeof(DeucarianEditorDiagnosticsDrawer).GetMethod("Draw"));
        }

        [Test]
        public void SplitPane_CalculatesStableUsableWidths()
        {
            DeucarianEditorSplitPaneWidths widths = DeucarianEditorSplitPane.Calculate(1040f, 280f, 360f, 340f);

            Assert.That(widths.Left, Is.GreaterThanOrEqualTo(180f));
            Assert.That(widths.Center, Is.GreaterThanOrEqualTo(260f));
            Assert.That(widths.Right, Is.GreaterThanOrEqualTo(240f));
            Assert.That(widths.Left + widths.Center + widths.Right, Is.GreaterThan(900f));
        }
    }
}
