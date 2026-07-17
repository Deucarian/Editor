using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorPackageTests
    {
        private static readonly string[] KnownPackageKeys =
        {
            "package-installer",
            "theming",
            "diagnostics",
            "logging",
            "object-loading",
            "api-helper",
            "session",
            "selection",
            "generic-ui-items",
            "editor"
        };

        private static readonly string[] KnownIconIds =
        {
            DeucarianEditorIconIds.Warning,
            DeucarianEditorIconIds.Undo,
            DeucarianEditorIconIds.Check,
            DeucarianEditorIconIds.Wrench,
            DeucarianEditorIconIds.CreateFolder,
            DeucarianEditorIconIds.CreatePackage,
            DeucarianEditorIconIds.OpenFolder,
            DeucarianEditorIconIds.Palette,
            DeucarianEditorIconIds.History,
            DeucarianEditorIconIds.Refresh,
            DeucarianEditorIconIds.Monitor,
            DeucarianEditorIconIds.Copy,
            DeucarianEditorIconIds.Info,
            DeucarianEditorIconIds.Reset,
            DeucarianEditorIconIds.Logging,
            DeucarianEditorIconIds.ChevronDown,
            DeucarianEditorIconIds.ChevronRight
        };

        [Test]
        public void PackageConstants_AreCorrect()
        {
            Assert.AreEqual("com.deucarian.editor", DeucarianEditorPackageConstants.PackageName);
            Assert.AreEqual("Deucarian Editor", DeucarianEditorPackageConstants.DisplayName);
            Assert.AreEqual("1.0.2", DeucarianEditorPackageConstants.Version);
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
        public void CuratedLucideIcons_LoadAndUnknownIdsFallback()
        {
            foreach (string iconId in KnownIconIds)
            {
                Assert.IsTrue(DeucarianEditorIcons.IsKnownIconId(iconId), iconId);
                Assert.NotNull(DeucarianEditorIcons.GetIcon(iconId), iconId);
                GUIContent content = DeucarianEditorIcons.GetIconContent(iconId, "Label", "Tooltip");
                Assert.NotNull(content.image, iconId);
                Assert.AreEqual("Label", content.text);
                Assert.AreEqual("Tooltip", content.tooltip);
            }

            Assert.IsFalse(DeucarianEditorIcons.IsKnownIconId("missing"));
            Assert.NotNull(DeucarianEditorIcons.GetIcon("missing"));
        }

        [Test]
        public void EditorPackage_DoesNotDependOnVectorGraphics()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(
                "Packages/com.deucarian.editor/package.json");
            string packagePath = package == null
                ? Path.GetFullPath("package.json")
                : Path.Combine(package.resolvedPath, "package.json");
            string json = File.ReadAllText(packagePath);
            StringAssert.DoesNotContain("com.unity.vectorgraphics", json);
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
        public void SidebarItemLabelStyle_UsesSupportedClipping()
        {
            FieldInfo clippingField = typeof(DeucarianEditorSidebar).GetField(
                "ItemLabelClipping",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(clippingField);
            Assert.AreEqual((int)TextClipping.Clip, clippingField.GetRawConstantValue());
        }

        [Test]
        public void VisualShellBackground_UsesScaleAndCropStyling()
        {
            VisualElement root = new VisualElement();
            Texture2D texture = new Texture2D(2, 2);

            try
            {
                Assert.NotNull(DeucarianEditorVisualShell.CreateWindowShell(root, texture));

                VisualElement background = root.Q<VisualElement>("deucarian-window-background");

                Assert.NotNull(background);
                AssertScaleAndCropBackground(background);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void FixedWallpaperBackground_UsesScaleAndCropStyling()
        {
            VisualElement root = new VisualElement();
            VisualElement background = new VisualElement { name = "deucarian-window-background" };
            VisualElement overlay = new VisualElement { name = "deucarian-window-overlay" };
            root.Add(background);
            root.Add(overlay);

            DeucarianEditorWindowChrome.ConfigureFixedWallpaper(root);

            AssertScaleAndCropBackground(background);
            AssertScaleAndCropBackground(overlay);
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

        [TestCase(899f, DeucarianEditorLayoutMode.Narrow)]
        [TestCase(900f, DeucarianEditorLayoutMode.Compact)]
        [TestCase(1179f, DeucarianEditorLayoutMode.Compact)]
        [TestCase(1180f, DeucarianEditorLayoutMode.Wide)]
        public void WorkbenchResponsiveLayout_UsesInstallerBreakpoints(
            float width,
            DeucarianEditorLayoutMode expected)
        {
            Assert.AreEqual(expected, DeucarianEditorResponsiveLayout.ResolveMode(width));
            Assert.AreEqual(900f, DeucarianEditorResponsiveLayout.WorkbenchNarrowBreakpoint);
            Assert.AreEqual(1180f, DeucarianEditorResponsiveLayout.WorkbenchWideBreakpoint);
        }

        [Test]
        public void WorkbenchResponsiveClasses_AreExclusiveAndIdempotent()
        {
            var element = new VisualElement();

            DeucarianEditorResponsiveLayout.ApplyResponsiveClasses(element, 899f);
            DeucarianEditorResponsiveLayout.ApplyResponsiveClasses(element, 899f);

            Assert.IsTrue(element.ClassListContains(DeucarianEditorResponsiveLayout.NarrowClass));
            Assert.AreEqual(
                1,
                element.GetClasses().Count(className =>
                    className == DeucarianEditorResponsiveLayout.NarrowClass));

            DeucarianEditorResponsiveLayout.ApplyResponsiveClasses(element, 1180f);

            Assert.IsTrue(element.ClassListContains(DeucarianEditorResponsiveLayout.WideClass));
            Assert.IsFalse(element.ClassListContains(DeucarianEditorResponsiveLayout.CompactClass));
            Assert.IsFalse(element.ClassListContains(DeucarianEditorResponsiveLayout.NarrowClass));
        }

        [Test]
        public void Workbench_ComposesExpectedHierarchyAndWallpaperHost()
        {
            var root = new VisualElement();
            var options = new DeucarianEditorWorkbenchOptions
            {
                IncludeHeader = true,
                IncludeToolbar = true,
                IncludeDrawer = true,
                IncludeFooter = true,
                HeaderPackageKey = "diagnostics",
                HeaderTitle = "Deucarian Diagnostics",
                HeaderSubtitle = "Inspect local runtime health.",
                ToolbarLayout = DeucarianEditorWorkbenchToolbarLayout.StableActionLanes,
                DrawerMode = DeucarianEditorWorkbenchDrawerMode.Overlay,
                TopSafeFadeName = "workbench-safe-fade"
            };

            using (DeucarianEditorWorkbench workbench = DeucarianEditorWorkbench.Create(root, options))
            {
                Assert.NotNull(workbench.ShellContent);
                Assert.NotNull(workbench.Header);
                Assert.NotNull(workbench.Toolbar);
                Assert.NotNull(workbench.Main);
                Assert.AreSame(workbench.Main, workbench.Content.parent);
                Assert.AreSame(workbench.Main, workbench.Drawer.parent);
                Assert.IsTrue(workbench.Drawer.ClassListContains(
                    DeucarianEditorWorkbenchSurfaces.OverlayDrawerHostClass));
                Assert.IsTrue(workbench.Toolbar.ClassListContains(
                    DeucarianEditorWorkbenchToolbar.StableActionLanesClass));
                Assert.IsTrue(workbench.Header.ClassListContains(
                    DeucarianEditorPackageHeader.RootClass));
                Assert.AreSame(workbench.ShellContent, workbench.Header.parent);
                Assert.Less(
                    workbench.ShellContent.IndexOf(workbench.Header),
                    workbench.ShellContent.IndexOf(workbench.Toolbar));
                Assert.AreSame(workbench.ShellContent, workbench.Footer.parent);
                Assert.AreSame(
                    workbench.ShellContent,
                    root.Q<VisualElement>("deucarian-window-background").parent);
                Assert.AreSame(
                    workbench.ShellContent,
                    root.Q<VisualElement>("deucarian-window-overlay").parent);
                Assert.AreSame(
                    workbench.ShellContent,
                    root.Q<VisualElement>("workbench-safe-fade").parent);

                Assert.AreEqual(DeucarianEditorLayoutMode.Narrow, workbench.ApplyResponsiveLayout(899f));
                Assert.AreEqual(DeucarianEditorLayoutMode.Narrow, workbench.ApplyResponsiveLayout(899f));
            }
        }

        [Test]
        public void PackageHeaderFactory_UsesCanonicalCompositionAndMetrics()
        {
            VisualElement header = DeucarianEditorPackageHeader.Create(
                "theming",
                "Deucarian Theming",
                "Compose and activate the project theme.");

            Assert.IsTrue(header.ClassListContains(DeucarianEditorPackageHeader.RootClass));
            Assert.NotNull(header.Q<Image>(className: DeucarianEditorPackageHeader.IconClass));
            Assert.AreEqual(
                "Deucarian Theming",
                header.Q<Label>(className: DeucarianEditorPackageHeader.TitleClass).text);
            Assert.AreEqual(
                "Compose and activate the project theme.",
                header.Q<Label>(className: DeucarianEditorPackageHeader.SubtitleClass).text);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PackageHeaderIconSize,
                DeucarianEditorPackageHeader.IconSize);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PackageHeaderIconTextGap,
                DeucarianEditorPackageHeader.IconTextGap);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PackageHeaderHorizontalPadding,
                DeucarianEditorStyles.PackageHeaderBox.padding.left);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PackageHeaderVerticalPadding,
                DeucarianEditorStyles.PackageHeaderBox.padding.top);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PackageHeaderBottomMargin,
                DeucarianEditorStyles.PackageHeaderBox.margin.bottom);
        }

        [Test]
        public void WorkbenchToolbarFactories_ApplySharedContractClasses()
        {
            VisualElement toolbar = DeucarianEditorWorkbenchToolbar.CreateToolbar();
            VisualElement commandBar = DeucarianEditorCommandBar.Create(
                DeucarianEditorWorkbenchToolbarLayout.StableActionLanes);
            DeucarianEditorCommandBarLanes commandLanes =
                DeucarianEditorCommandBar.CreateLanes(commandBar);
            VisualElement commandNavigation = DeucarianEditorCommandBar.CreateNavigationGroup();
            VisualElement commandActions = DeucarianEditorCommandBar.CreateActionGroup();
            Button commandToggle = DeucarianEditorCommandBar.CreateToggle("Theme", null, true);
            Button commandAction = DeucarianEditorCommandBar.CreateAction(
                DeucarianEditorIconIds.Refresh,
                "Refresh",
                null);
            VisualElement commandState = DeucarianEditorCommandBar.CreateState(
                DeucarianEditorIconIds.Check,
                "Active");
            VisualElement stableToolbar = DeucarianEditorWorkbenchToolbar.CreateToolbar(
                DeucarianEditorWorkbenchToolbarLayout.StableActionLanes);
            VisualElement compactToolbar = DeucarianEditorWorkbenchToolbar.CreateToolbar(
                DeucarianEditorWorkbenchToolbarLayout.CompactSingleLine);
            Button standard = DeucarianEditorWorkbenchToolbar.CreateActionButton("Refresh", null);
            Button emphasized = DeucarianEditorWorkbenchToolbar.CreateActionButton("Apply", null, true);
            Button toggle = DeucarianEditorWorkbenchToolbar.CreateToggleButton("Stable", null, true);
            Button iconAction = DeucarianEditorWorkbenchToolbar.CreateIconActionButton(
                DeucarianEditorIconIds.Refresh,
                "Refresh",
                null,
                false,
                "Reload");
            Button genericIconAction = DeucarianEditorIconTextButton.Create(
                DeucarianEditorIconIds.Copy,
                "Copy",
                null,
                "Copy value");
            Label summary = DeucarianEditorWorkbenchToolbar.CreateSummary("3 packages");
            VisualElement spacer = DeucarianEditorWorkbenchToolbar.CreateSpacer();

            Assert.IsTrue(toolbar.ClassListContains(DeucarianEditorWorkbenchToolbar.ToolbarClass));
            Assert.IsTrue(commandBar.ClassListContains(DeucarianEditorCommandBar.RootClass));
            Assert.AreSame(commandBar, commandLanes.Root);
            Assert.AreSame(commandLanes.Leading, commandBar.ElementAt(0));
            Assert.AreSame(commandLanes.Summary, commandBar.ElementAt(1));
            Assert.AreSame(commandLanes.Trailing, commandBar.ElementAt(2));
            Assert.IsTrue(commandLanes.Leading.ClassListContains(
                DeucarianEditorCommandBar.LeadingLaneClass));
            Assert.IsTrue(commandLanes.Summary.ClassListContains(
                DeucarianEditorCommandBar.SummaryLaneClass));
            Assert.IsTrue(commandLanes.Trailing.ClassListContains(
                DeucarianEditorCommandBar.TrailingLaneClass));
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.TextLineHeight,
                commandLanes.Summary.style.height.value.value);
            Assert.IsTrue(commandNavigation.ClassListContains(
                DeucarianEditorCommandBar.NavigationGroupClass));
            Assert.IsTrue(commandActions.ClassListContains(
                DeucarianEditorCommandBar.ActionGroupClass));
            Assert.IsTrue(commandToggle.ClassListContains(DeucarianEditorCommandBar.ToggleClass));
            Assert.IsTrue(commandAction.ClassListContains(DeucarianEditorCommandBar.ActionClass));
            Assert.IsTrue(commandState.ClassListContains(DeucarianEditorCommandBar.StateClass));
            Assert.IsTrue(stableToolbar.ClassListContains(
                DeucarianEditorWorkbenchToolbar.StableActionLanesClass));
            Assert.IsTrue(compactToolbar.ClassListContains(
                DeucarianEditorWorkbenchToolbar.CompactSingleLineClass));
            Assert.IsTrue(standard.ClassListContains(DeucarianEditorWorkbenchToolbar.StandardActionClass));
            Assert.IsTrue(emphasized.ClassListContains(DeucarianEditorWorkbenchToolbar.EmphasizedActionClass));
            Assert.IsTrue(toggle.ClassListContains(DeucarianEditorWorkbenchToolbar.ToggleClass));
            Assert.IsTrue(toggle.ClassListContains(DeucarianEditorWorkbenchToolbar.ToggleActiveClass));
            Assert.IsTrue(iconAction.ClassListContains(DeucarianEditorWorkbenchToolbar.IconActionClass));
            Assert.IsTrue(iconAction.ClassListContains(DeucarianEditorIconTextButton.RootClass));
            Assert.IsTrue(genericIconAction.ClassListContains(DeucarianEditorIconTextButton.RootClass));
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.CommandControlHeight,
                genericIconAction.style.height.value.value);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.CommandControlHeight,
                genericIconAction.style.minHeight.value.value);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.CommandControlHeight,
                genericIconAction.style.maxHeight.value.value);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.IconTextGap,
                genericIconAction.Q<VisualElement>(
                    className: DeucarianEditorIconTextButton.GapClass).style.width.value.value);
            Assert.AreEqual("Refresh", standard.text);
            Assert.AreEqual("Stable", toggle.text);
            Assert.AreEqual(string.Empty, commandAction.text);
            Assert.AreEqual(
                "Refresh",
                commandAction.Q<Label>(
                    className: DeucarianEditorIconTextButton.LabelClass).text);
            Assert.AreEqual(string.Empty, commandToggle.text);
            Assert.AreEqual(
                "Theme",
                commandToggle.Q<Label>(
                    className: DeucarianEditorIconTextButton.LabelClass).text);
            Assert.NotNull(iconAction.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.ContentClass));
            Assert.AreEqual(
                "deucarian-workbench-toolbar__icon",
                DeucarianEditorWorkbenchToolbar.IconClass);
            Assert.AreEqual(
                "deucarian-workbench-toolbar__icon-label",
                DeucarianEditorWorkbenchToolbar.IconLabelClass);
            Assert.NotNull(iconAction.Q<Image>(className: DeucarianEditorWorkbenchToolbar.IconClass));
            Assert.NotNull(iconAction.Q<Image>(className: DeucarianEditorIconTextButton.IconClass));
            VisualElement iconContent = iconAction.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.ContentClass);
            VisualElement iconGap = iconAction.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.GapClass);
            Assert.NotNull(iconGap);
            Assert.AreEqual(3, iconContent.childCount);
            Assert.AreSame(iconGap, iconContent.ElementAt(1));
            Assert.AreEqual(8f, iconGap.style.width.value.value);
            Assert.AreEqual(8f, iconAction.style.paddingLeft.value.value);
            Assert.AreEqual(8f, iconAction.style.paddingRight.value.value);
            Assert.AreEqual(0f, iconAction.style.flexShrink.value);
            Assert.AreEqual(PickingMode.Ignore, iconContent.pickingMode);
            Assert.AreEqual(
                PickingMode.Ignore,
                iconContent.Q<Image>(className: DeucarianEditorIconTextButton.IconClass).pickingMode);
            Assert.AreEqual(PickingMode.Ignore, iconGap.pickingMode);
            Assert.AreEqual(
                PickingMode.Ignore,
                iconContent.Q<Label>(className: DeucarianEditorIconTextButton.LabelClass).pickingMode);
            Assert.AreEqual(
                "Refresh",
                iconAction.Q<Label>(className: DeucarianEditorWorkbenchToolbar.IconLabelClass).text);
            Assert.AreEqual(
                DeucarianEditorTheme.Text,
                iconAction.Q<Label>(className: DeucarianEditorWorkbenchToolbar.IconLabelClass).style.color.value);
            Assert.AreEqual("Reload", iconAction.tooltip);
            Assert.IsTrue(summary.ClassListContains(DeucarianEditorWorkbenchToolbar.SummaryClass));
            Assert.IsTrue(spacer.ClassListContains(DeucarianEditorWorkbenchToolbar.SpacerClass));
            Assert.IsFalse(standard.ClassListContains(DeucarianEditorIconTextButton.RootClass));
            Assert.IsFalse(toggle.ClassListContains(DeucarianEditorIconTextButton.RootClass));
            Assert.IsNull(standard.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.GapClass));
            Assert.NotNull(commandState.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.ContentClass));
            Assert.NotNull(commandState.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.GapClass));
            DeucarianEditorCommandBar.SetMinimumWidth(commandAction, 160f);
            Assert.AreEqual(160f, commandAction.style.minWidth.value.value);
            Assert.AreEqual(0f, commandAction.style.flexShrink.value);

            DeucarianEditorWorkbenchToolbar.SetToggleActive(toggle, false);
            Assert.IsFalse(toggle.ClassListContains(DeucarianEditorWorkbenchToolbar.ToggleActiveClass));

            DeucarianEditorWorkbenchToolbar.SetIconActionButtonText(iconAction, "Reload now");
            Assert.AreEqual(
                "Reload now",
                iconAction.Q<Label>(className: DeucarianEditorWorkbenchToolbar.IconLabelClass).text);

            VisualElement navigation = DeucarianEditorWorkbenchToolbar.CreateGroup();
            VisualElement actions = DeucarianEditorWorkbenchToolbar.CreateGroup(true);
            VisualElement slot = DeucarianEditorWorkbenchToolbar.CreateReservedActionSlot(124f);
            DeucarianEditorWorkbenchToolbar.SetReservedAction(slot, iconAction, false);
            Assert.IsTrue(navigation.ClassListContains(
                DeucarianEditorWorkbenchToolbar.NavigationGroupClass));
            Assert.IsTrue(actions.ClassListContains(
                DeucarianEditorWorkbenchToolbar.ActionGroupClass));
            Assert.AreEqual(124f, slot.style.width.value.value);
            Assert.AreEqual(Visibility.Hidden, slot.style.visibility.value);
            Assert.AreSame(slot, iconAction.parent);
            DeucarianEditorWorkbenchToolbar.SetReservedActionVisible(slot, true);
            Assert.AreEqual(Visibility.Visible, slot.style.visibility.value);
        }

        [Test]
        public void LayoutMetrics_ExposeCanonicalSurfaceAndCommandGeometry()
        {
            Assert.AreEqual(10, DeucarianEditorLayoutMetrics.PageHorizontalPadding);
            Assert.AreEqual(8, DeucarianEditorLayoutMetrics.PageVerticalPadding);
            Assert.AreEqual(10, DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding);
            Assert.AreEqual(8, DeucarianEditorLayoutMetrics.SurfaceVerticalPadding);
            Assert.AreEqual(8, DeucarianEditorLayoutMetrics.SurfaceSpacing);
            Assert.AreEqual(10, DeucarianEditorLayoutMetrics.FooterHorizontalPadding);
            Assert.AreEqual(0, DeucarianEditorLayoutMetrics.FooterVerticalPadding);
            Assert.AreEqual(34, DeucarianEditorLayoutMetrics.FooterHeight);
            Assert.AreEqual(28, DeucarianEditorLayoutMetrics.CommandControlHeight);
            Assert.AreEqual(8, DeucarianEditorLayoutMetrics.CommandControlHorizontalPadding);
            Assert.AreEqual(18, DeucarianEditorLayoutMetrics.TextLineHeight);
            Assert.AreEqual(14, DeucarianEditorLayoutMetrics.IconSize);
            Assert.AreEqual(8, DeucarianEditorLayoutMetrics.IconTextGap);
            Assert.AreEqual(46, DeucarianEditorLayoutMetrics.CommandBarSingleRowHeight);
            Assert.AreEqual(78, DeucarianEditorLayoutMetrics.CommandBarTwoRowHeight);

            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.CommandControlHeight,
                DeucarianEditorSpacing.ControlHeight);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.CommandControlHorizontalPadding,
                DeucarianEditorIconTextButton.HorizontalPadding);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.TextLineHeight,
                DeucarianEditorIconTextButton.TextHeight);
        }

        [UnityTest]
        public IEnumerator CommandBarLanes_AttachedLayoutCentersControlsAtSupportedWidths()
        {
            if (Application.isBatchMode ||
                SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                // EditorWindow.Show requires a graphics device. The graphical Editor
                // path below remains the resolved-layout verification; batchmode
                // verifies the exact factory and stylesheet geometry that feeds it.
                AssertHeadlessCommandBarGeometryContract();
                yield break;
            }

            LayoutHostWindow host = ScriptableObject.CreateInstance<LayoutHostWindow>();
            try
            {
                host.position = new Rect(80f, 80f, 960f, 240f);
                host.Show();

                VisualElement page = DeucarianEditorVisualShell.CreateWindowShell(
                    host.rootVisualElement);
                Assert.NotNull(page);
                page.style.height = 180f;

                VisualElement stableBar = DeucarianEditorCommandBar.Create(
                    DeucarianEditorWorkbenchToolbarLayout.StableActionLanes);
                DeucarianEditorCommandBarLanes stableLanes =
                    DeucarianEditorCommandBar.CreateLanes(stableBar);
                Button stableLeading = DeucarianEditorCommandBar.CreateToggle(
                    "Theme",
                    null,
                    true,
                    DeucarianEditorIconIds.Palette);
                Button stableTrailing = DeucarianEditorCommandBar.CreateAction(
                    DeucarianEditorIconIds.Check,
                    "Active",
                    null);
                stableLanes.Leading.Add(stableLeading);
                stableLanes.Summary.text = "Theme summary";
                stableLanes.Trailing.Add(stableTrailing);
                page.Add(stableBar);

                page.style.width = 920f;
                DeucarianEditorResponsiveLayout.ApplyModeClasses(
                    page,
                    DeucarianEditorLayoutMode.Compact);
                host.Repaint();
                yield return null;
                yield return null;

                AssertOneRowCommandBarGeometry(
                    stableBar,
                    stableLanes,
                    stableLeading,
                    stableTrailing);

                page.style.width = 520f;
                DeucarianEditorResponsiveLayout.ApplyModeClasses(
                    page,
                    DeucarianEditorLayoutMode.Narrow);
                host.Repaint();
                yield return null;
                yield return null;

                Assert.That(
                    stableBar.resolvedStyle.height,
                    Is.EqualTo(DeucarianEditorLayoutMetrics.CommandBarTwoRowHeight).Within(0.5f));
                Assert.That(
                    stableLanes.Summary.resolvedStyle.display,
                    Is.EqualTo(DisplayStyle.None));
                Assert.That(
                    stableLanes.Leading.resolvedStyle.height,
                    Is.EqualTo(DeucarianEditorLayoutMetrics.CommandControlHeight).Within(0.5f));
                Assert.That(
                    stableLanes.Trailing.resolvedStyle.height,
                    Is.EqualTo(DeucarianEditorLayoutMetrics.CommandControlHeight).Within(0.5f));
                AssertCenteredY(stableLanes.Leading.worldBound, stableLeading.worldBound, "stacked leading button");
                AssertCenteredY(stableLanes.Trailing.worldBound, stableTrailing.worldBound, "stacked trailing button");
                Assert.That(
                    (stableLanes.Leading.worldBound.center.y + stableLanes.Trailing.worldBound.center.y) * 0.5f,
                    Is.EqualTo(stableBar.worldBound.center.y).Within(0.5f));
                AssertIconTextCentered(stableLeading);
                AssertIconTextCentered(stableTrailing);

                stableBar.RemoveFromHierarchy();

                VisualElement compactBar = DeucarianEditorCommandBar.Create(
                    DeucarianEditorWorkbenchToolbarLayout.CompactSingleLine);
                DeucarianEditorCommandBarLanes compactLanes =
                    DeucarianEditorCommandBar.CreateLanes(compactBar);
                Button compactLeading = DeucarianEditorCommandBar.CreateToggle(
                    "Runtime Overlay Off",
                    null,
                    false,
                    DeucarianEditorIconIds.Monitor);
                Button compactTrailing = DeucarianEditorCommandBar.CreateAction(
                    DeucarianEditorIconIds.Refresh,
                    "Refresh",
                    null);
                compactLanes.Leading.Add(compactLeading);
                compactLanes.Summary.text = "0 sections · 12:00 UTC";
                compactLanes.Trailing.Add(compactTrailing);
                page.Add(compactBar);

                page.style.width = 420f;
                host.Repaint();
                yield return null;
                yield return null;

                AssertOneRowCommandBarGeometry(
                    compactBar,
                    compactLanes,
                    compactLeading,
                    compactTrailing);
            }
            finally
            {
                if (host != null)
                {
                    host.Close();
                    if (host != null)
                    {
                        UnityEngine.Object.DestroyImmediate(host);
                    }
                }
            }
        }

        [Test]
        public void WorkbenchDrawerAndFooterFactories_AreDomainNeutralAndStable()
        {
            DeucarianEditorWorkbenchDrawer drawer = DeucarianEditorWorkbenchSurfaces.CreateDrawer(true);
            Assert.IsTrue(drawer.Root.ClassListContains(DeucarianEditorWorkbenchSurfaces.DrawerExpandedClass));
            Assert.IsFalse(drawer.Root.ClassListContains(DeucarianEditorWorkbenchSurfaces.DrawerCollapsedClass));
            Assert.NotNull(drawer.ScrollView);
            Assert.NotNull(drawer.Content);

            DeucarianEditorWorkbenchSurfaces.SetDrawerExpanded(drawer.Root, false);
            Assert.IsFalse(drawer.Root.ClassListContains(DeucarianEditorWorkbenchSurfaces.DrawerExpandedClass));
            Assert.IsTrue(drawer.Root.ClassListContains(DeucarianEditorWorkbenchSurfaces.DrawerCollapsedClass));

            DeucarianEditorWorkbenchFooter footer = DeucarianEditorWorkbenchSurfaces.CreateFooter(
                "i", "Idle", "Nothing running", "Details", null, DeucarianEditorPackageConstants.Version);
            Assert.AreEqual(5, footer.Root.childCount);
            Assert.AreSame(footer.Actions, footer.Action.parent);
            Assert.NotNull(footer.StatusContent);
            Assert.NotNull(footer.StatusGap);
            Assert.IsTrue(footer.StatusContent.ClassListContains(
                DeucarianEditorIconTextButton.ContentClass));
            Assert.IsTrue(footer.Action.ClassListContains(DeucarianEditorWorkbenchSurfaces.FooterActionClass));
            Button secondary = DeucarianEditorWorkbenchSurfaces.AddFooterAction(
                footer,
                DeucarianEditorIconIds.Wrench,
                "Tools",
                null,
                "Open tools",
                112f);
            Assert.NotNull(secondary);
            Assert.AreEqual(2, footer.Actions.childCount);
            Assert.AreEqual(112f, secondary.style.width.value.value);
            Assert.IsTrue(secondary.ClassListContains(DeucarianEditorIconTextButton.RootClass));
            Assert.NotNull(secondary.Q<Image>(className: DeucarianEditorWorkbenchToolbar.IconClass));
            DeucarianEditorWorkbenchSurfaces.SetFooterIcon(footer, DeucarianEditorIconIds.Info);
            Assert.AreEqual(DisplayStyle.None, footer.StatusIcon.style.display.value);
            Assert.AreEqual(DisplayStyle.Flex, footer.StatusImage.style.display.value);
            Assert.NotNull(footer.StatusImage.image);
            DeucarianEditorWorkbenchSurfaces.SetFooterStatus(footer, DeucarianEditorStatus.Success);
            Assert.IsTrue(footer.StatusIcon.ClassListContains(DeucarianEditorWorkbenchSurfaces.FooterStatusSuccessClass));

            VisualElement column = DeucarianEditorWorkbenchSurfaces.CreateDrawerColumn("Create");
            Button drawerAction = DeucarianEditorWorkbenchSurfaces.CreateDrawerAction(
                DeucarianEditorIconIds.CreateFolder,
                "Theme family...",
                null,
                "Create a family");
            column.Add(drawerAction);
            Assert.IsTrue(column.ClassListContains(
                DeucarianEditorWorkbenchSurfaces.DrawerColumnClass));
            Assert.IsTrue(drawerAction.ClassListContains(
                DeucarianEditorWorkbenchSurfaces.DrawerActionClass));
            Assert.IsTrue(drawerAction.ClassListContains(DeucarianEditorIconTextButton.RootClass));
            Assert.IsTrue(drawerAction.ClassListContains(DeucarianEditorIconTextButton.LeadingClass));
            Assert.NotNull(drawerAction.Q<Image>(className: DeucarianEditorWorkbenchToolbar.IconClass));

            Button reset = DeucarianEditorSettingsActions.CreateResetToDefaultsButton(null);
            Assert.IsTrue(reset.ClassListContains(DeucarianEditorIconTextButton.RootClass));
            Assert.IsTrue(reset.ClassListContains(DeucarianEditorIconTextButton.LeadingClass));
            Assert.IsTrue(reset.ClassListContains(DeucarianEditorSettingsActions.ResetActionClass));
            Assert.AreEqual(
                DeucarianEditorSettingsActions.ResetToDefaultsLabel,
                reset.Q<Label>(className: DeucarianEditorIconTextButton.LabelClass).text);
            Assert.AreEqual(DeucarianEditorSettingsActions.ResetButtonWidth, reset.style.width.value.value);
            Assert.AreEqual(DeucarianEditorSettingsActions.ResetButtonHeight, reset.style.height.value.value);
            Assert.NotNull(reset.Q<VisualElement>(className: DeucarianEditorIconTextButton.GapClass));
        }

        [Test]
        public void WorkbenchImGuiStyles_UseStandaloneAndEmbeddedPageContracts()
        {
            DeucarianEditorWorkbenchGUI.ClearCache();

            Assert.AreEqual(28f, DeucarianEditorWorkbenchGUI.PrimaryButtonStyle.fixedHeight);
            Assert.AreEqual(FontStyle.Bold, DeucarianEditorWorkbenchGUI.PrimaryButtonStyle.fontStyle);
            Assert.AreEqual(28f, DeucarianEditorWorkbenchGUI.SecondaryButtonStyle.fixedHeight);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PageHorizontalPadding,
                DeucarianEditorWorkbenchGUI.WindowStyle.padding.left);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PageTopPadding,
                DeucarianEditorWorkbenchGUI.WindowStyle.padding.top);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.PageBottomPadding,
                DeucarianEditorWorkbenchGUI.WindowStyle.padding.bottom);
            Assert.AreEqual(0, DeucarianEditorWorkbenchGUI.EmbeddedPageStyle.padding.left);
            Assert.AreEqual(0, DeucarianEditorWorkbenchGUI.EmbeddedPageStyle.padding.right);
            Assert.AreEqual(0, DeucarianEditorWorkbenchGUI.EmbeddedPageStyle.padding.top);
            Assert.AreEqual(0, DeucarianEditorWorkbenchGUI.EmbeddedPageStyle.padding.bottom);
            Assert.AreEqual(0, DeucarianEditorWorkbenchGUI.EmbeddedPageStyle.margin.left);
            Assert.NotNull(typeof(DeucarianEditorWorkbenchGUI).GetMethod(
                nameof(DeucarianEditorWorkbenchGUI.BeginEmbeddedPage)));
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding,
                DeucarianEditorStyles.SectionBox.padding.left);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.SurfaceVerticalPadding,
                DeucarianEditorStyles.SectionBox.padding.top);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.SurfaceSpacing,
                DeucarianEditorStyles.SectionBox.margin.bottom);
            Assert.AreEqual(10, DeucarianEditorWorkbenchGUI.SidebarStyle.padding.left);
            Assert.AreEqual(8, DeucarianEditorWorkbenchGUI.SidebarStyle.padding.top);
            Assert.AreEqual(8, DeucarianEditorWorkbenchGUI.SidebarStyle.padding.bottom);
            Assert.AreEqual(10, DeucarianEditorWorkbenchGUI.DetailsStyle.padding.right);
            Assert.AreEqual(8, DeucarianEditorWorkbenchGUI.DetailsStyle.padding.top);
            Assert.AreEqual(8, DeucarianEditorWorkbenchGUI.DetailsStyle.padding.bottom);
            Assert.AreEqual(8, DeucarianEditorWorkbenchGUI.SampleRowStyle.padding.top);
            Assert.AreEqual(0, DeucarianEditorWorkbenchGUI.SampleRowStyle.margin.top);
            Assert.AreEqual(8, DeucarianEditorWorkbenchGUI.SampleRowStyle.margin.bottom);
            Assert.AreEqual(118f, DeucarianEditorWorkbenchGUI.DetailLabelWidth);
            Assert.AreEqual(28f, DeucarianEditorWorkbenchGUI.CompactIconActionHeight);
            Assert.AreEqual(14f, DeucarianEditorWorkbenchGUI.CompactIconSize);
            Assert.AreEqual(8f, DeucarianEditorWorkbenchGUI.CompactIconTextGap);
            Assert.AreEqual(8, DeucarianEditorStyles.ToolbarButton.padding.left);
            Assert.AreEqual(0, DeucarianEditorStyles.ToolbarButton.padding.top);
            Assert.AreEqual(
                DeucarianEditorIconTextButton.IconSize,
                DeucarianEditorWorkbenchGUI.CompactIconSize);
            Assert.AreEqual(
                DeucarianEditorIconTextButton.IconTextGap,
                DeucarianEditorWorkbenchGUI.CompactIconTextGap);
            Assert.NotNull(typeof(DeucarianEditorWorkbenchGUI).GetMethod(
                nameof(DeucarianEditorWorkbenchGUI.DrawCompactIconAction),
                new[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(bool),
                    typeof(bool),
                    typeof(GUILayoutOption[])
                }));
            DeucarianEditorIconTextButton.CalculateImGuiContentRects(
                new Rect(0f, 0f, 164f, 28f),
                out Rect compactIconRect,
                out Rect compactTextRect);
            Assert.AreEqual(new Rect(8f, 7f, 14f, 14f), compactIconRect);
            Assert.AreEqual(new Rect(30f, 5f, 126f, 18f), compactTextRect);
            Assert.AreEqual(14f, compactIconRect.center.y);
            Assert.AreEqual(14f, compactTextRect.center.y);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.TextColor, DeucarianEditorWorkbenchGUI.LabelStyle.normal.textColor);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.TextColor, DeucarianEditorWorkbenchGUI.BoldLabelStyle.normal.textColor);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.TextColor, DeucarianEditorWorkbenchGUI.SectionTitleStyle.normal.textColor);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.MutedTextColor, DeucarianEditorWorkbenchGUI.WordWrappedMiniLabelStyle.normal.textColor);
            Assert.AreEqual(0.46f, DeucarianEditorWorkbenchGUI.RowBackgroundColor.a, 0.001f);
            Assert.AreEqual(0.62f, DeucarianEditorWorkbenchGUI.RowHoverColor.a, 0.001f);
            Assert.AreEqual(0.58f, DeucarianEditorWorkbenchGUI.RowSelectedColor.a, 0.001f);
        }

        [Test]
        public void SharedImGuiContainers_UseCanonicalSurfaceInsetsAndSingleSpacing()
        {
            AssertCanonicalSurfaceInsets(GetPrivateStaticStyle(
                typeof(DeucarianEditorCards),
                "CardStyle"));
            AssertCanonicalSurfaceInsets(GetPrivateStaticStyle(
                typeof(DeucarianEditorCards),
                "HeaderCardStyle"));
            GUIStyle inlineCard = GetPrivateStaticStyle(
                typeof(DeucarianEditorCards),
                "InlineCardStyle");
            AssertCanonicalSurfaceInsets(inlineCard);
            Assert.AreEqual(0, inlineCard.margin.top);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.SurfaceSpacing,
                inlineCard.margin.bottom);

            AssertCanonicalSurfaceInsets(GetPrivateStaticStyle(
                typeof(DeucarianEditorAccordion),
                "HeaderStyle"));
            AssertCanonicalSurfaceInsets(GetPrivateStaticStyle(
                typeof(DeucarianEditorAccordion),
                "BodyStyle"));
            AssertCanonicalSurfaceInsets(DeucarianEditorSidebar.ContainerStyle);
            AssertCanonicalSurfaceInsets(GetPrivateStaticStyle(
                typeof(DeucarianEditorCompactObjectCard),
                "CardStyle"));
            AssertCanonicalSurfaceInsets(GetPrivateStaticStyle(
                typeof(DeucarianEditorCompactObjectCard),
                "SelectedCardStyle"));
            AssertCanonicalSurfaceInsets(GetPrivateStaticStyle(
                typeof(DeucarianEditorStatusPanel),
                "MessageStyle"));
        }

        [Test]
        public void BrandedImGuiSurfaces_KeepReadableContrastAcrossUnitySkins()
        {
            Assert.That(
                ContrastRatio(DeucarianEditorColors.TitleText, DeucarianEditorColors.HeaderBackground),
                Is.GreaterThanOrEqualTo(4.5f));
            Assert.That(
                ContrastRatio(DeucarianEditorColors.MutedText, DeucarianEditorColors.HeaderBackground),
                Is.GreaterThanOrEqualTo(3f));
            Assert.That(
                ContrastRatio(DeucarianEditorWorkbenchGUI.TextColor, DeucarianEditorColors.SectionBackground),
                Is.GreaterThanOrEqualTo(4.5f));

            Color lightSkinControlText = new Color(31f / 255f, 43f / 255f, 50f / 255f, 1f);
            Assert.That(
                ContrastRatio(lightSkinControlText, new Color(0.75f, 0.75f, 0.75f, 1f)),
                Is.GreaterThanOrEqualTo(4.5f));
        }

        [Test]
        public void WorkbenchStatusRows_PreserveInstallerContentColorComposition()
        {
            const string assetPath =
                "Packages/com.deucarian.editor/Editor/DeucarianEditorWorkbenchGUI.cs";
            PackageInfo package = PackageInfo.FindForAssetPath(assetPath);
            const string packagePrefix = "Packages/com.deucarian.editor/";
            string relativePath = assetPath.Substring(packagePrefix.Length);
            string absolutePath = package == null
                ? Path.GetFullPath(assetPath)
                : Path.Combine(package.resolvedPath, relativePath);
            string source = File.ReadAllText(absolutePath);
            int methodStart = source.IndexOf(
                "private static void DrawColoredLabel",
                StringComparison.Ordinal);
            int methodEnd = source.IndexOf(
                "private static void EnsureStyles",
                methodStart,
                StringComparison.Ordinal);

            Assert.GreaterOrEqual(methodStart, 0);
            Assert.Greater(methodEnd, methodStart);
            string methodSource = source.Substring(methodStart, methodEnd - methodStart);

            StringAssert.Contains("Color previousColor = GUI.contentColor;", methodSource);
            StringAssert.Contains("GUI.contentColor = color;", methodSource);
            StringAssert.Contains("GUI.Label(rect, content, style);", methodSource);
            StringAssert.Contains("GUI.contentColor = previousColor;", methodSource);
            StringAssert.DoesNotContain("new GUIStyle(style)", methodSource);
        }

        [Test]
        public void SharedStyleSheet_ContainsWorkbenchAndCompatibilityContracts()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(DeucarianEditorUIResources.SharedStyleSheetPath);
            const string packagePrefix = "Packages/com.deucarian.editor/";
            string relativePath = DeucarianEditorUIResources.SharedStyleSheetPath.Substring(packagePrefix.Length);
            string absolutePath = package == null
                ? Path.GetFullPath(DeucarianEditorUIResources.SharedStyleSheetPath)
                : Path.Combine(package.resolvedPath, relativePath);
            Assert.IsTrue(File.Exists(absolutePath), absolutePath);
            string uss = File.ReadAllText(absolutePath);

            StringAssert.Contains(".deucarian-workbench-toolbar", uss);
            StringAssert.Contains(".dpi-view-toolbar", uss);
            StringAssert.Contains("height: 24px;", uss);
            StringAssert.Contains("min-width: 86px;", uss);
            StringAssert.Contains(".deucarian-workbench-operation-drawer", uss);
            StringAssert.Contains(".dpi-operation-drawer", uss);
            StringAssert.Contains(".deucarian-workbench-operation-footer__action:active", uss);
            StringAssert.Contains(".dpi-operation-footer__details-button:active", uss);
            StringAssert.Contains("--deucarian-workbench-operation-footer-height: 34px;", uss);
        }

        [Test]
        public void SharedStyleSheet_UsesOneIconTextPaddingContractAcrossSurfaces()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(DeucarianEditorUIResources.SharedStyleSheetPath);
            const string packagePrefix = "Packages/com.deucarian.editor/";
            string relativePath = DeucarianEditorUIResources.SharedStyleSheetPath.Substring(packagePrefix.Length);
            string absolutePath = package == null
                ? Path.GetFullPath(DeucarianEditorUIResources.SharedStyleSheetPath)
                : Path.Combine(package.resolvedPath, relativePath);
            string uss = File.ReadAllText(absolutePath);

            Assert.AreEqual(8f, DeucarianEditorIconTextButton.HorizontalPadding);
            Assert.AreEqual(0f, DeucarianEditorIconTextButton.VerticalPadding);
            Assert.AreEqual(8f, DeucarianEditorIconTextButton.IconTextGap);
            Assert.AreEqual(14f, DeucarianEditorIconTextButton.IconSize);

            string rootRule = GetStyleRule(uss, ".deucarian-icon-text-button");
            StringAssert.Contains("height: 28px;", rootRule);
            StringAssert.Contains("min-height: 28px;", rootRule);
            StringAssert.Contains("max-height: 28px;", rootRule);
            StringAssert.Contains("padding-left: 8px;", rootRule);
            StringAssert.Contains("padding-right: 8px;", rootRule);
            StringAssert.Contains("padding-top: 0;", rootRule);
            StringAssert.Contains("padding-bottom: 0;", rootRule);
            StringAssert.Contains("flex-shrink: 0;", rootRule);
            AssertButtonSurfaceRuleHasNoIndependentPadding(
                uss,
                ".deucarian-workbench-toolbar__action--icon");
            AssertButtonSurfaceRuleHasNoIndependentPadding(
                uss,
                ".deucarian-workbench-operation-drawer__action");
            AssertButtonSurfaceRuleHasNoIndependentPadding(
                uss,
                ".deucarian-workbench-operation-footer__action,");
            string contentRule = GetStyleRule(uss, ".deucarian-icon-text-button__content");
            StringAssert.DoesNotContain("position: absolute;", contentRule);
            StringAssert.DoesNotContain(
                ".deucarian-icon-text-button > .deucarian-icon-text-button__content",
                uss);
            string gapRule = GetStyleRule(uss, ".deucarian-icon-text-button__gap");
            StringAssert.Contains("width: 8px;", gapRule);
            StringAssert.Contains("min-width: 8px;", gapRule);
            StringAssert.Contains("max-width: 8px;", gapRule);
            StringAssert.Contains("align-content: center;", uss);
            StringAssert.Contains("height: 28px;", uss);
            StringAssert.DoesNotContain("padding-left: 6px;", uss);
            StringAssert.DoesNotContain("padding-right: 6px;", uss);
        }

        [Test]
        public void SharedStyleSheet_UsesOnePackageHeaderContractForCanonicalAndLegacyClasses()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(DeucarianEditorUIResources.SharedStyleSheetPath);
            const string packagePrefix = "Packages/com.deucarian.editor/";
            string relativePath = DeucarianEditorUIResources.SharedStyleSheetPath.Substring(packagePrefix.Length);
            string absolutePath = package == null
                ? Path.GetFullPath(DeucarianEditorUIResources.SharedStyleSheetPath)
                : Path.Combine(package.resolvedPath, relativePath);
            string uss = File.ReadAllText(absolutePath);

            string headerRule = GetStyleRule(uss, ".deucarian-package-header,");
            StringAssert.Contains(".deucarian-header", headerRule);
            StringAssert.Contains("min-height: 58px;", headerRule);
            StringAssert.Contains("margin-bottom: 8px;", headerRule);
            StringAssert.Contains("padding-left: 10px;", headerRule);
            StringAssert.Contains("padding-right: 10px;", headerRule);
            StringAssert.Contains("padding-top: 8px;", headerRule);
            StringAssert.Contains("padding-bottom: 8px;", headerRule);

            string iconRule = GetStyleRule(uss, ".deucarian-package-header__icon");
            StringAssert.Contains("width: 24px;", iconRule);
            StringAssert.Contains("height: 24px;", iconRule);
            StringAssert.Contains("margin-right: 10px;", iconRule);
            StringAssert.DoesNotContain("min-height: 90px;", uss);
        }

        [Test]
        public void SharedStyleSheet_UsesCanonicalSurfaceInsetsAndSingleSpacing()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(
                DeucarianEditorUIResources.SharedStyleSheetPath);
            const string packagePrefix = "Packages/com.deucarian.editor/";
            string relativePath = DeucarianEditorUIResources.SharedStyleSheetPath.Substring(
                packagePrefix.Length);
            string absolutePath = package == null
                ? Path.GetFullPath(DeucarianEditorUIResources.SharedStyleSheetPath)
                : Path.Combine(package.resolvedPath, relativePath);
            string uss = File.ReadAllText(absolutePath);

            AssertCanonicalSurfaceRule(uss, ".deucarian-window-content");
            AssertCanonicalSurfaceRule(uss, ".deucarian-sidebar");
            AssertCanonicalSurfaceRule(uss, ".deucarian-panel");
            AssertCanonicalSurfaceRule(uss, ".deucarian-toolbar-row");

            string sectionRule = GetStyleRule(uss, ".deucarian-section");
            StringAssert.Contains("margin-bottom: 8px;", sectionRule);
            StringAssert.DoesNotContain("margin-bottom: 14px;", uss);
        }

        private static void AssertButtonSurfaceRuleHasNoIndependentPadding(
            string uss,
            string selector)
        {
            string rule = GetStyleRule(uss, selector);

            StringAssert.Contains("padding-left: 0;", rule, selector);
            StringAssert.Contains("padding-right: 0;", rule, selector);
            StringAssert.Contains("padding-top: 0;", rule, selector);
            StringAssert.Contains("padding-bottom: 0;", rule, selector);
        }

        private static void AssertCanonicalSurfaceRule(string uss, string selector)
        {
            string rule = GetStyleRule(uss, selector);
            StringAssert.Contains("padding-left: 10px;", rule, selector);
            StringAssert.Contains("padding-right: 10px;", rule, selector);
            StringAssert.Contains("padding-top: 8px;", rule, selector);
            StringAssert.Contains("padding-bottom: 8px;", rule, selector);
        }

        private static void AssertCanonicalSurfaceInsets(GUIStyle style)
        {
            Assert.NotNull(style);
            Assert.AreEqual(DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding, style.padding.left);
            Assert.AreEqual(DeucarianEditorLayoutMetrics.SurfaceHorizontalPadding, style.padding.right);
            Assert.AreEqual(DeucarianEditorLayoutMetrics.SurfaceVerticalPadding, style.padding.top);
            Assert.AreEqual(DeucarianEditorLayoutMetrics.SurfaceVerticalPadding, style.padding.bottom);
        }

        private static GUIStyle GetPrivateStaticStyle(Type owner, string propertyName)
        {
            PropertyInfo property = owner.GetProperty(
                propertyName,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(property, owner.FullName + "." + propertyName);
            return property.GetValue(null) as GUIStyle;
        }

        private static string GetStyleRule(string uss, string selector)
        {
            int ruleStart = uss.IndexOf(selector, StringComparison.Ordinal);
            Assert.GreaterOrEqual(ruleStart, 0, selector);
            int ruleEnd = uss.IndexOf('}', ruleStart);
            Assert.Greater(ruleEnd, ruleStart, selector);
            return uss.Substring(ruleStart, ruleEnd - ruleStart);
        }

        [Test]
        public void LayoutScopes_AreDisposableContracts()
        {
            Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(typeof(DeucarianEditorCardScope)));
            Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(typeof(DeucarianEditorFoldoutScope)));
            Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(typeof(DeucarianEditorWorkbenchPanelScope)));
            Assert.NotNull(typeof(DeucarianEditorCards).GetMethod("BeginCardScope"));
            Assert.NotNull(typeof(DeucarianEditorCards).GetMethod("BeginInlineCardScope"));
            Assert.NotNull(typeof(DeucarianEditorAccordion).GetMethod("BeginFoldoutCardScope"));
            Assert.NotNull(typeof(DeucarianEditorFoldoutCard).GetMethod("BeginScope"));
        }

        [Test]
        public void CardScope_DisposeEndsLayoutExactlyOnce()
        {
            int endCount = 0;
            ConstructorInfo constructor = typeof(DeucarianEditorCardScope).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(Action) },
                null);

            Assert.NotNull(constructor);
            var scope = (IDisposable)constructor.Invoke(new object[] { (Action)(() => endCount++) });
            scope.Dispose();
            scope.Dispose();

            Assert.AreEqual(1, endCount);
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

        private static void AssertScaleAndCropBackground(VisualElement element)
        {
#if UNITY_2022_2_OR_NEWER
            Assert.AreEqual(BackgroundSizeType.Cover, element.style.backgroundSize.value.sizeType);
            Assert.AreEqual(BackgroundPositionKeyword.Center, element.style.backgroundPositionX.value.keyword);
            Assert.AreEqual(BackgroundPositionKeyword.Center, element.style.backgroundPositionY.value.keyword);
            Assert.AreEqual(Repeat.NoRepeat, element.style.backgroundRepeat.value.x);
            Assert.AreEqual(Repeat.NoRepeat, element.style.backgroundRepeat.value.y);
#else
            Assert.AreEqual(ScaleMode.ScaleAndCrop, element.style.unityBackgroundScaleMode.value);
#endif
        }

        private static void AssertOneRowCommandBarGeometry(
            VisualElement commandBar,
            DeucarianEditorCommandBarLanes lanes,
            Button leadingButton,
            Button trailingButton)
        {
            Assert.That(
                commandBar.resolvedStyle.height,
                Is.EqualTo(DeucarianEditorLayoutMetrics.CommandBarSingleRowHeight).Within(0.5f));
            Assert.That(
                leadingButton.resolvedStyle.height,
                Is.EqualTo(DeucarianEditorLayoutMetrics.CommandControlHeight).Within(0.5f));
            Assert.That(
                trailingButton.resolvedStyle.height,
                Is.EqualTo(DeucarianEditorLayoutMetrics.CommandControlHeight).Within(0.5f));
            Assert.That(
                lanes.Summary.resolvedStyle.height,
                Is.EqualTo(DeucarianEditorLayoutMetrics.TextLineHeight).Within(0.5f));
            AssertCenteredY(commandBar.worldBound, lanes.Leading.worldBound, "leading lane");
            AssertCenteredY(commandBar.worldBound, lanes.Summary.worldBound, "summary lane");
            AssertCenteredY(commandBar.worldBound, lanes.Trailing.worldBound, "trailing lane");
            AssertCenteredY(lanes.Leading.worldBound, leadingButton.worldBound, "leading button");
            AssertCenteredY(lanes.Trailing.worldBound, trailingButton.worldBound, "trailing button");
            AssertIconTextCentered(leadingButton);
            AssertIconTextCentered(trailingButton);
        }

        private static void AssertHeadlessCommandBarGeometryContract()
        {
            VisualElement commandBar = DeucarianEditorCommandBar.Create(
                DeucarianEditorWorkbenchToolbarLayout.StableActionLanes);
            DeucarianEditorCommandBarLanes lanes =
                DeucarianEditorCommandBar.CreateLanes(commandBar);
            Button leading = DeucarianEditorCommandBar.CreateToggle(
                "Theme",
                null,
                true,
                DeucarianEditorIconIds.Palette);
            Button trailing = DeucarianEditorCommandBar.CreateAction(
                DeucarianEditorIconIds.Check,
                "Active",
                null);
            lanes.Leading.Add(leading);
            lanes.Trailing.Add(trailing);

            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.TextLineHeight,
                lanes.Summary.style.height.value.value);
            AssertInlineIconTextGeometry(leading);
            AssertInlineIconTextGeometry(trailing);

            string uss = ReadSharedStyleSheet();
            string toolbarRule = GetStyleRule(uss, ".deucarian-toolbar-row");
            StringAssert.Contains("height: 46px;", toolbarRule);
            StringAssert.Contains("min-height: 46px;", toolbarRule);
            StringAssert.Contains("max-height: 46px;", toolbarRule);
            StringAssert.Contains("align-items: center;", toolbarRule);
            StringAssert.Contains("align-content: center;", toolbarRule);

            string laneRule = GetStyleRule(uss, ".deucarian-command-bar__navigation,");
            StringAssert.Contains("height: 28px;", laneRule);
            StringAssert.Contains("min-height: 28px;", laneRule);
            StringAssert.Contains("max-height: 28px;", laneRule);
            StringAssert.Contains("align-items: center;", laneRule);

            string summaryRule = GetStyleRule(uss, ".deucarian-command-bar__summary");
            StringAssert.Contains("height: 18px;", summaryRule);
            StringAssert.Contains("min-height: 18px;", summaryRule);
            StringAssert.Contains("max-height: 18px;", summaryRule);
            StringAssert.Contains("-unity-text-align: middle-left;", summaryRule);

            string stackedRule = GetStyleRule(
                uss,
                ".deucarian-responsive--narrow .deucarian-workbench-toolbar--stable-action-lanes,");
            StringAssert.Contains("height: 78px;", stackedRule);
            StringAssert.Contains("min-height: 78px;", stackedRule);
            StringAssert.Contains("max-height: 78px;", stackedRule);
            StringAssert.Contains("flex-direction: column;", stackedRule);
            StringAssert.Contains("justify-content: center;", stackedRule);

            string compactRule = GetStyleRule(
                uss,
                ".deucarian-responsive--narrow .deucarian-workbench-toolbar--compact-single-line,");
            StringAssert.Contains("height: 46px;", compactRule);
            StringAssert.Contains("min-height: 46px;", compactRule);
            StringAssert.Contains("max-height: 46px;", compactRule);
            StringAssert.Contains("align-items: center;", compactRule);
            StringAssert.Contains("align-content: center;", compactRule);
        }

        private static void AssertInlineIconTextGeometry(Button button)
        {
            VisualElement content = button.Q<VisualElement>(
                className: DeucarianEditorIconTextButton.ContentClass);
            Image icon = button.Q<Image>(className: DeucarianEditorIconTextButton.IconClass);
            Label label = button.Q<Label>(className: DeucarianEditorIconTextButton.LabelClass);

            Assert.NotNull(content);
            Assert.NotNull(icon);
            Assert.NotNull(label);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.CommandControlHeight,
                button.style.height.value.value);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.TextLineHeight,
                content.style.height.value.value);
            Assert.AreEqual(Align.Center, content.style.alignItems.value);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.IconSize,
                icon.style.height.value.value);
            Assert.AreEqual(
                DeucarianEditorLayoutMetrics.TextLineHeight,
                label.style.height.value.value);
        }

        private static string ReadSharedStyleSheet()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(
                DeucarianEditorUIResources.SharedStyleSheetPath);
            const string packagePrefix = "Packages/com.deucarian.editor/";
            string relativePath = DeucarianEditorUIResources.SharedStyleSheetPath.Substring(
                packagePrefix.Length);
            string absolutePath = package == null
                ? Path.GetFullPath(DeucarianEditorUIResources.SharedStyleSheetPath)
                : Path.Combine(package.resolvedPath, relativePath);
            Assert.IsTrue(File.Exists(absolutePath), absolutePath);
            return File.ReadAllText(absolutePath);
        }

        private static void AssertIconTextCentered(Button button)
        {
            Image icon = button.Q<Image>(className: DeucarianEditorIconTextButton.IconClass);
            Label label = button.Q<Label>(className: DeucarianEditorIconTextButton.LabelClass);
            Assert.NotNull(icon);
            Assert.NotNull(label);
            Assert.That(
                icon.resolvedStyle.height,
                Is.EqualTo(DeucarianEditorLayoutMetrics.IconSize).Within(0.5f));
            Assert.That(
                label.resolvedStyle.height,
                Is.EqualTo(DeucarianEditorLayoutMetrics.TextLineHeight).Within(0.5f));
            AssertCenteredY(button.worldBound, icon.worldBound, "button icon");
            AssertCenteredY(button.worldBound, label.worldBound, "button label");
        }

        private static void AssertCenteredY(Rect outer, Rect inner, string description)
        {
            Assert.That(
                inner.center.y,
                Is.EqualTo(outer.center.y).Within(0.5f),
                description + " should be vertically centered");
        }

        private sealed class LayoutHostWindow : EditorWindow
        {
        }

        private static float ContrastRatio(Color first, Color second)
        {
            float brighter = Mathf.Max(RelativeLuminance(first), RelativeLuminance(second));
            float darker = Mathf.Min(RelativeLuminance(first), RelativeLuminance(second));
            return (brighter + 0.05f) / (darker + 0.05f);
        }

        private static float RelativeLuminance(Color color)
        {
            return 0.2126f * Linearize(color.r) +
                   0.7152f * Linearize(color.g) +
                   0.0722f * Linearize(color.b);
        }

        private static float Linearize(float channel)
        {
            return channel <= 0.03928f
                ? channel / 12.92f
                : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
        }
    }
}
