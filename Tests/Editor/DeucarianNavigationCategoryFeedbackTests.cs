using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianNavigationCategoryFeedbackTests
    {
        private sealed class CategoryWindow : EditorWindow { }

        [UnityTest]
        public IEnumerator CategoryFocusAddsFeedbackWithoutMovingContentOrReplacingSelection()
        {
            var window = ScriptableObject.CreateInstance<CategoryWindow>();
            window.position = new Rect(20, 20, 1500, 850);
            window.Show();
            try
            {
                using var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Feedback test");
                var category = new Foldout { text = "Notifications", value = true };
                category.AddToClassList("dw-navigation-group");
                category.AddToClassList("dw-navigation-icon-group");
                var toggle = category.Q<Toggle>();
                var icon = DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.Notifications);
                icon.AddToClassList("dw-navigation-group-icon");
                toggle.Insert(0, icon);
                workspace.Navigation.Add(category);
                var selected = workspace.AddNavigation("selected", "Notification Lab", null, () => { });
                category.Add(selected);
                workspace.SelectNavigation("selected");
                for (int i = 0; i < 10; i++) yield return null;
                Color background = toggle.resolvedStyle.backgroundColor;
                Color selectedBackground = selected.resolvedStyle.backgroundColor;
                Rect bounds = toggle.worldBound;
                toggle.Focus();
                for (int i = 0; i < 3; i++) yield return null;
                Assert.That(toggle.resolvedStyle.backgroundColor, Is.Not.EqualTo(background));
                Assert.That(toggle.worldBound, Is.EqualTo(bounds));
                Assert.That(selected.ClassListContains("dw-selected"), Is.True);
                Assert.That(selected.resolvedStyle.backgroundColor, Is.EqualTo(selectedBackground));
                Assert.That(category.value, Is.True);
            }
            finally { window.Close(); }
        }
    }
}
