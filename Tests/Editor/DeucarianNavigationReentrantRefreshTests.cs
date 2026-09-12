using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianNavigationReentrantRefreshTests
    {
        [UnityTest]
        public IEnumerator AttachedNavigationCoalescesPresentationRefreshDuringRendering()
        {
            const string id = "test.navigation.reentrant";
            bool refresh = false;
            var host = ScriptableObject.CreateInstance<NavigationRefreshWindow>();
            host.titleContent = new GUIContent("Navigation refresh test");
            host.Show();
            try
            {
                using (DeucarianToolRegistry.Register(new DeucarianToolDescriptor(id, "Fixture", "Fixture",
                    DeucarianControlCenterArea.Developer, () => { }, "com.deucarian.editor",
                    createPage: () => new DeucarianEditorPage(new VisualElement()), isFeatureEnabled: () => {
                        if (refresh) { refresh = false; DeucarianToolRegistry.RefreshPresentation(); }
                        return true;
                    })))
                using (var workspace = new DeucarianEditorWorkspace(host.rootVisualElement, "Fixture"))
                {
                    refresh = true;
                    Assert.DoesNotThrow(() => DeucarianEditorWorkspaceNavigation.Populate(workspace, id));
                    Assert.IsFalse(refresh);
                    for (int i = 0; i < 5; i++) yield return null;
                    Assert.That(workspace.Root.Query<Button>("workspace-nav-" + id).ToList().Count, Is.EqualTo(1));
                    Assert.That(workspace.Root.Query<Button>("workspace-nav-advanced").ToList().Count, Is.EqualTo(1));
                }
            }
            finally { host.Close(); }
        }

        private sealed class NavigationRefreshWindow : EditorWindow { }
    }
}
