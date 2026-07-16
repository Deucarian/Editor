using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

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
                IncludeToolbar = true,
                IncludeDrawer = true,
                IncludeFooter = true,
                ToolbarLayout = DeucarianEditorWorkbenchToolbarLayout.StableActionLanes,
                DrawerMode = DeucarianEditorWorkbenchDrawerMode.Overlay,
                TopSafeFadeName = "workbench-safe-fade"
            };

            using (DeucarianEditorWorkbench workbench = DeucarianEditorWorkbench.Create(root, options))
            {
                Assert.NotNull(workbench.ShellContent);
                Assert.NotNull(workbench.Toolbar);
                Assert.NotNull(workbench.Main);
                Assert.AreSame(workbench.Main, workbench.Content.parent);
                Assert.AreSame(workbench.Main, workbench.Drawer.parent);
                Assert.IsTrue(workbench.Drawer.ClassListContains(
                    DeucarianEditorWorkbenchSurfaces.OverlayDrawerHostClass));
                Assert.IsTrue(workbench.Toolbar.ClassListContains(
                    DeucarianEditorWorkbenchToolbar.StableActionLanesClass));
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
        public void WorkbenchToolbarFactories_ApplySharedContractClasses()
        {
            VisualElement toolbar = DeucarianEditorWorkbenchToolbar.CreateToolbar();
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
            Label summary = DeucarianEditorWorkbenchToolbar.CreateSummary("3 packages");
            VisualElement spacer = DeucarianEditorWorkbenchToolbar.CreateSpacer();

            Assert.IsTrue(toolbar.ClassListContains(DeucarianEditorWorkbenchToolbar.ToolbarClass));
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
            Assert.AreEqual(
                "Refresh",
                iconAction.Q<Label>(className: DeucarianEditorWorkbenchToolbar.IconLabelClass).text);
            Assert.AreEqual(
                DeucarianEditorTheme.Text,
                iconAction.Q<Label>(className: DeucarianEditorWorkbenchToolbar.IconLabelClass).style.color.value);
            Assert.AreEqual("Reload", iconAction.tooltip);
            Assert.IsTrue(summary.ClassListContains(DeucarianEditorWorkbenchToolbar.SummaryClass));
            Assert.IsTrue(spacer.ClassListContains(DeucarianEditorWorkbenchToolbar.SpacerClass));

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
        }

        [Test]
        public void WorkbenchImGuiStyles_PreserveInstallerMetrics()
        {
            DeucarianEditorWorkbenchGUI.ClearCache();

            Assert.AreEqual(24f, DeucarianEditorWorkbenchGUI.PrimaryButtonStyle.fixedHeight);
            Assert.AreEqual(FontStyle.Bold, DeucarianEditorWorkbenchGUI.PrimaryButtonStyle.fontStyle);
            Assert.AreEqual(24f, DeucarianEditorWorkbenchGUI.SecondaryButtonStyle.fixedHeight);
            Assert.AreEqual(12, DeucarianEditorWorkbenchGUI.WindowStyle.padding.left);
            Assert.AreEqual(10, DeucarianEditorWorkbenchGUI.WindowStyle.padding.top);
            Assert.AreEqual(10, DeucarianEditorWorkbenchGUI.SidebarStyle.padding.left);
            Assert.AreEqual(10, DeucarianEditorWorkbenchGUI.DetailsStyle.padding.right);
            Assert.AreEqual(8, DeucarianEditorWorkbenchGUI.SampleRowStyle.padding.top);
            Assert.AreEqual(2, DeucarianEditorWorkbenchGUI.SampleRowStyle.margin.top);
            Assert.AreEqual(6, DeucarianEditorWorkbenchGUI.SampleRowStyle.margin.bottom);
            Assert.AreEqual(118f, DeucarianEditorWorkbenchGUI.DetailLabelWidth);
            Assert.AreEqual(28f, DeucarianEditorWorkbenchGUI.CompactIconActionHeight);
            Assert.AreEqual(14f, DeucarianEditorWorkbenchGUI.CompactIconSize);
            Assert.AreEqual(8f, DeucarianEditorWorkbenchGUI.CompactIconTextGap);
            Assert.AreEqual(
                DeucarianEditorIconTextButton.IconSize,
                DeucarianEditorWorkbenchGUI.CompactIconSize);
            Assert.AreEqual(
                DeucarianEditorIconTextButton.IconTextGap,
                DeucarianEditorWorkbenchGUI.CompactIconTextGap);
            DeucarianEditorIconTextButton.CalculateImGuiContentRects(
                new Rect(0f, 0f, 164f, 28f),
                out Rect compactIconRect,
                out Rect compactTextRect);
            Assert.AreEqual(new Rect(8f, 7f, 14f, 14f), compactIconRect);
            Assert.AreEqual(new Rect(30f, 0f, 126f, 28f), compactTextRect);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.TextColor, DeucarianEditorWorkbenchGUI.LabelStyle.normal.textColor);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.TextColor, DeucarianEditorWorkbenchGUI.BoldLabelStyle.normal.textColor);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.TextColor, DeucarianEditorWorkbenchGUI.SectionTitleStyle.normal.textColor);
            Assert.AreEqual(DeucarianEditorWorkbenchGUI.MutedTextColor, DeucarianEditorWorkbenchGUI.WordWrappedMiniLabelStyle.normal.textColor);
            Assert.AreEqual(0.46f, DeucarianEditorWorkbenchGUI.RowBackgroundColor.a, 0.001f);
            Assert.AreEqual(0.62f, DeucarianEditorWorkbenchGUI.RowHoverColor.a, 0.001f);
            Assert.AreEqual(0.58f, DeucarianEditorWorkbenchGUI.RowSelectedColor.a, 0.001f);
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
