using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianWorkspaceNavigationFooterTests
    {
        private sealed class FooterWindow : EditorWindow { }

        [UnityTest]
        public IEnumerator ExpandingAllGroupsKeepsAdvancedOutsideTheScrollableTree()
        {
            int originalScale = DeucarianEditorAppearance.WorkspaceScalePercent;
            var window = ScriptableObject.CreateInstance<FooterWindow>();
            var registrations = new List<IDisposable>();
            try
            {
                for (int group = 0; group < 16; group++)
                    for (int item = 0; item < 4; item++)
                        registrations.Add(DeucarianToolRegistry.Register(new DeucarianToolDescriptor(
                            "test.footer." + group + "." + item, "Tool " + item, "Footer layout fixture",
                            DeucarianControlCenterArea.Developer, () => { }, "com.deucarian.editor",
                            createPage: () => new DeucarianEditorPage(new VisualElement()),
                            navigationPath: "Fixture " + group + "/Nested")));
                window.Show();
                foreach (int width in new[] { 1800, 1000 })
                    foreach (int scale in new[] { 75, 100, 150 })
                    {
                        DeucarianEditorAppearance.WorkspaceScalePercent = scale;
                        window.position = new Rect(20, 20, width, 650);
                        using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Footer test"))
                        {
                            DeucarianEditorWorkspaceNavigation.Populate(workspace, "home");
                            for (int frame = 0; frame < 12; frame++) yield return null;
                            workspace.Navigation.Query<Foldout>().ForEach(group => group.value = true);
                            for (int frame = 0; frame < 12; frame++) yield return null;
                            var advanced = workspace.NavigationFooter.Q<Button>("workspace-nav-advanced");
                            var scroll = workspace.Root.ClassListContains("dw-compact")
                                ? workspace.Root.Q<ScrollView>("workspace-navigation-rail")
                                : workspace.Root.Q<ScrollView>("workspace-navigation-scroll");
                            Assert.That(advanced.worldBound.height, Is.GreaterThan(10));
                            Assert.That(advanced.worldBound.yMax, Is.LessThanOrEqualTo(workspace.Sidebar.worldBound.yMax + 1));
                            Assert.That(advanced.worldBound.yMin, Is.GreaterThanOrEqualTo(workspace.Sidebar.worldBound.yMin));
                            Assert.That(workspace.Navigation.worldBound.yMax, Is.LessThanOrEqualTo(workspace.NavigationFooter.worldBound.yMin + 1));
                            Assert.That(scroll.contentViewport.worldBound.height, Is.GreaterThan(20));
                            Assert.That(scroll.verticalScroller.highValue, Is.GreaterThan(0), width + " / " + scale);
                            scroll.scrollOffset = new Vector2(0, scroll.verticalScroller.highValue);
                            yield return null;
                            Assert.That(advanced.worldBound.yMax, Is.LessThanOrEqualTo(workspace.Sidebar.worldBound.yMax + 1));
                        }
                    }
            }
            finally
            {
                foreach (var registration in registrations) registration.Dispose();
                window.Close();
                DeucarianEditorAppearance.WorkspaceScalePercent = originalScale;
            }
        }

        [UnityTest]
        public IEnumerator ButtonRoleDoesNotChangeSizeOrBaseline()
        {
            int originalScale = DeucarianEditorAppearance.WorkspaceScalePercent;
            var window = ScriptableObject.CreateInstance<FooterWindow>();
            window.Show();
            window.position = new Rect(20, 20, 1586, 940);
            try
            {
                using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Button test"))
                {
                    var primary = DeucarianEditorWorkspaceControls.Button("Primary", () => { }, true);
                    var secondary = DeucarianEditorWorkspaceControls.Button("Secondary", () => { });
                    var quiet = DeucarianEditorWorkspaceControls.Button("Quiet", () => { }, DeucarianEditorButtonRole.Quiet);
                    workspace.Content.Add(DeucarianEditorWorkspaceControls.Actions(primary, secondary, quiet));
                    foreach (int scale in new[] { 75, 100, 150 })
                    {
                        DeucarianEditorAppearance.WorkspaceScalePercent = scale;
                        for (int frame = 0; frame < 12; frame++) yield return null;
                        Assert.That(primary.worldBound.height, Is.EqualTo(secondary.worldBound.height).Within(.1f));
                        Assert.That(primary.worldBound.yMin, Is.EqualTo(secondary.worldBound.yMin).Within(.1f));
                        Assert.That(primary.resolvedStyle.fontSize, Is.EqualTo(secondary.resolvedStyle.fontSize));
                        Assert.That(quiet.worldBound.height, Is.EqualTo(secondary.worldBound.height).Within(.1f));
                    }
                }
            }
            finally { window.Close(); DeucarianEditorAppearance.WorkspaceScalePercent = originalScale; }
        }
    }
}
