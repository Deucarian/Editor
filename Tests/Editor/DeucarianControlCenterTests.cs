using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianControlCenterTests
    {
        [TearDown]
        public void CloseWindow()
        {
            foreach (DeucarianControlCenterWindow window in
                     UnityEngine.Resources.FindObjectsOfTypeAll<
                         DeucarianControlCenterWindow>())
            {
                window.Close();
            }
        }

        [Test]
        public void Window_OpensBuildsWorkbenchAndFocusesIssue()
        {
            DeucarianControlCenterWindow window =
                DeucarianControlCenterWindow.OpenProjectIssue("TEST-FOCUS");
            window.CreateGUI();

            Assert.AreEqual(
                DeucarianControlCenterArea.Project,
                window.SelectedArea);
            Assert.AreEqual(
                "deucarian.readiness.issue.TEST-FOCUS",
                window.FocusedTargetId);
            Assert.NotNull(
                window.rootVisualElement.Q("deucarian-workbench-content"));
            Assert.NotNull(
                window.rootVisualElement.Q<TextField>(
                    "control-center-search"));
            Assert.NotNull(
                window.rootVisualElement.Q<Button>(
                    "control-center-refresh"));
            Assert.NotNull(
                window.rootVisualElement.Q(
                    "control-center-sidebar"));
        }

        [Test]
        public void Registry_RejectsCrossKindDuplicatesAndDisposesIdempotently()
        {
            string id = Unique("registry");
            var card = new CardProvider(id);
            IDisposable registration =
                DeucarianControlCenterRegistry.RegisterCardProvider(card);
            try
            {
                Assert.Throws<InvalidOperationException>(() =>
                    DeucarianControlCenterRegistry.RegisterSectionProvider(
                        new SectionProvider(id)));
            }
            finally
            {
                registration.Dispose();
                registration.Dispose();
            }

            IDisposable replacement =
                DeucarianControlCenterRegistry.RegisterSectionProvider(
                    new SectionProvider(id));
            replacement.Dispose();
        }

        [Test]
        public void ToolRegistry_DisposeSupportsReloadStyleReregistration()
        {
            string id = Unique("tool");
            IDisposable first = DeucarianToolRegistry.Register(
                Tool(id, "Same name", 5));
            Assert.Throws<InvalidOperationException>(() =>
                DeucarianToolRegistry.Register(Tool(id, "Duplicate", 0)));
            first.Dispose();
            first.Dispose();

            IDisposable second = DeucarianToolRegistry.Register(
                Tool(id, "Same name", 5));
            try
            {
                Assert.IsTrue(DeucarianToolRegistry.TryGet(id, out _));
            }
            finally
            {
                second.Dispose();
            }
        }

        [Test]
        public void ToolRegistry_OrdersEqualNamesByStableId()
        {
            string prefix = Unique("order");
            IDisposable second = DeucarianToolRegistry.Register(
                Tool(prefix + ".b", "Equal", 7));
            IDisposable first = DeucarianToolRegistry.Register(
                Tool(prefix + ".a", "Equal", 7));
            try
            {
                string[] ids = DeucarianToolRegistry.GetTools()
                    .Where(tool => tool.Id.StartsWith(
                        prefix,
                        StringComparison.Ordinal))
                    .Select(tool => tool.Id)
                    .ToArray();
                CollectionAssert.AreEqual(
                    new[] { prefix + ".a", prefix + ".b" },
                    ids);
            }
            finally
            {
                first.Dispose();
                second.Dispose();
            }
        }

        [Test]
        public void Snapshot_OrdersCardsAndOnlyShowsPopulatedAreas()
        {
            string prefix = Unique("snapshot");
            var provider = new CardProvider(
                prefix,
                Card(prefix + ".b", DeucarianControlCenterArea.Connections, 5),
                Card(prefix + ".a", DeucarianControlCenterArea.Connections, 5));
            IDisposable registration =
                DeucarianControlCenterRegistry.RegisterCardProvider(provider);
            try
            {
                DeucarianControlCenterSnapshot snapshot =
                    DeucarianControlCenterSnapshotBuilder.Capture();
                string[] ordered = snapshot.Cards
                    .Where(card => card.Id.StartsWith(
                        prefix,
                        StringComparison.Ordinal))
                    .Select(card => card.Id)
                    .ToArray();
                CollectionAssert.AreEqual(
                    new[] { prefix + ".a", prefix + ".b" },
                    ordered);

                var expected = new HashSet<DeucarianControlCenterArea>
                {
                    DeucarianControlCenterArea.Overview,
                    DeucarianControlCenterArea.Project
                };
                foreach (DeucarianControlCenterCard card in snapshot.Cards)
                {
                    expected.Add(card.Area);
                }

                foreach (DeucarianControlCenterSection section in snapshot.Sections)
                {
                    expected.Add(section.Area);
                }

                foreach (DeucarianToolDescriptor tool in snapshot.Tools)
                {
                    expected.Add(tool.Area);
                }

                CollectionAssert.AreEquivalent(expected, snapshot.Areas);
            }
            finally
            {
                registration.Dispose();
            }
        }

        [Test]
        public void Search_CoversCardsActionsToolsAndReadiness()
        {
            string prefix = Unique("search");
            bool invoked = false;
            var action = new DeucarianControlCenterAction(
                prefix + ".action",
                "Rotate access token",
                () => invoked = true,
                "Explicit test action",
                requiresConfirmation: true);
            var provider = new CardProvider(
                prefix,
                new DeucarianControlCenterCard(
                    prefix + ".card",
                    DeucarianControlCenterArea.Communication,
                    "Signal status",
                    "Healthy relay",
                    prefix + ".package",
                    details: new[] { "bounded detail" },
                    actions: new[] { action }));
            IDisposable cards =
                DeucarianControlCenterRegistry.RegisterCardProvider(provider);
            IDisposable tools = DeucarianToolRegistry.Register(
                new DeucarianToolDescriptor(
                    prefix + ".tool",
                    "Signal console",
                    "Open signal diagnostics",
                    DeucarianControlCenterArea.Communication,
                    () => invoked = true,
                    prefix + ".package"));
            IDisposable readiness =
                DeucarianProjectValidationRegistry.Register(
                    new CheckProvider(prefix));
            try
            {
                DeucarianControlCenterSnapshot snapshot =
                    DeucarianControlCenterSnapshotBuilder.Capture();
                Assert.That(
                    SearchIds(snapshot, "signal status"),
                    Does.Contain(prefix + ".card"));
                Assert.That(
                    SearchIds(snapshot, "signal console"),
                    Does.Contain(prefix + ".tool"));
                DeucarianControlCenterSearchResult actionResult =
                    DeucarianControlCenterSearch.Search(
                            snapshot,
                            "rotate token")
                        .Single(result => result.TargetId == prefix + ".action");
                Assert.IsTrue(actionResult.RequiresConfirmation);
                actionResult.Invoke();
                Assert.IsTrue(invoked);
                Assert.That(
                    SearchIds(snapshot, prefix + "-ISSUE"),
                    Does.Contain(
                        "deucarian.readiness.issue." + prefix + "-ISSUE"));
                Assert.That(
                    SearchIds(snapshot, prefix + ".package"),
                    Does.Contain(prefix + ".card"));
            }
            finally
            {
                readiness.Dispose();
                tools.Dispose();
                cards.Dispose();
            }
        }

        [Test]
        public void Snapshot_IsolatesProviderExceptionsWithoutLeakingMessages()
        {
            string prefix = Unique("failure");
            IDisposable bad =
                DeucarianControlCenterRegistry.RegisterCardProvider(
                    new ThrowingProvider(prefix, "provider-message-marker"));
            IDisposable good =
                DeucarianControlCenterRegistry.RegisterCardProvider(
                    new CardProvider(
                        prefix + ".good",
                        Card(
                            prefix + ".good.card",
                            DeucarianControlCenterArea.Diagnostics,
                            0)));
            try
            {
                DeucarianControlCenterSnapshot snapshot =
                    DeucarianControlCenterSnapshotBuilder.Capture();
                Assert.That(
                    snapshot.Cards.Select(card => card.Id),
                    Does.Contain(prefix + ".good.card"));
                DeucarianControlCenterCard failure = snapshot.Cards.Single(
                    card => card.Id ==
                        "deucarian.provider-failure." + prefix);
                StringAssert.Contains(
                    nameof(InvalidOperationException),
                    failure.Description);
                StringAssert.DoesNotContain(
                    "provider-message-marker",
                    failure.Description);
            }
            finally
            {
                good.Dispose();
                bad.Dispose();
            }
        }

        [Test]
        public void LegacyFacade_IsObsoleteAndRedirectsToControlCenter()
        {
            Assert.NotNull(
                typeof(DeucarianProjectSetupWindow)
                    .GetCustomAttribute<ObsoleteAttribute>());
            MethodInfo open = typeof(DeucarianProjectSetupWindow).GetMethod(
                "Open",
                new[] { typeof(string) });
            open.Invoke(null, new object[] { "OLD-API" });

            DeucarianControlCenterWindow window =
                EditorWindow.GetWindow<DeucarianControlCenterWindow>();
            Assert.AreEqual(
                DeucarianControlCenterArea.Project,
                window.SelectedArea);
            Assert.AreEqual(
                "deucarian.readiness.issue.OLD-API",
                window.FocusedTargetId);
        }

        [Test]
        public void MenuAndSettingsPaths_FollowGovernedPolicy()
        {
            string packageRoot = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                typeof(DeucarianControlCenterWindow).Assembly).resolvedPath;
            string windowSource = File.ReadAllText(Path.Combine(
                packageRoot,
                "Editor",
                "DeucarianControlCenterWindow.cs"));
            string facadeSource = File.ReadAllText(Path.Combine(
                packageRoot,
                "Editor",
                "DeucarianProjectSetupWindow.cs"));
            string settingsSource = File.ReadAllText(Path.Combine(
                packageRoot,
                "Editor",
                "DeucarianControlCenterSettings.cs"));

            StringAssert.Contains(
                "Tools/Deucarian/Control Center...",
                windowSource);
            StringAssert.Contains(
                "Tools/Deucarian/Advanced/Developer Tools...",
                windowSource);
            StringAssert.Contains(
                "Tools/Deucarian/Advanced/Legacy Shortcuts...",
                windowSource);
            StringAssert.DoesNotContain("[MenuItem", facadeSource);
            StringAssert.DoesNotContain(
                "Tools/Deucarian/Project Setup",
                facadeSource);
            StringAssert.Contains(
                "Project/Deucarian/Control Center",
                settingsSource);
        }

        [Test]
        public void OverviewAndProjectAlwaysExposeReadinessContent()
        {
            DeucarianControlCenterSnapshot snapshot =
                DeucarianControlCenterSnapshotBuilder.Capture();
            Assert.That(
                snapshot.Areas,
                Does.Contain(DeucarianControlCenterArea.Overview));
            Assert.That(
                snapshot.Areas,
                Does.Contain(DeucarianControlCenterArea.Project));
            Assert.That(
                snapshot.Cards.Select(card => card.Id),
                Does.Contain("deucarian.readiness.overview"));
            Assert.IsNotEmpty(
                snapshot.GetCards(DeucarianControlCenterArea.Project));
        }

        private static IEnumerable<string> SearchIds(
            DeucarianControlCenterSnapshot snapshot,
            string query)
        {
            return DeucarianControlCenterSearch.Search(snapshot, query)
                .Select(result => result.TargetId);
        }

        private static DeucarianControlCenterCard Card(
            string id,
            DeucarianControlCenterArea area,
            int order)
        {
            return new DeucarianControlCenterCard(
                id,
                area,
                id,
                "Test card",
                "com.deucarian.editor",
                order: order);
        }

        private static DeucarianToolDescriptor Tool(
            string id,
            string name,
            int order)
        {
            return new DeucarianToolDescriptor(
                id,
                name,
                "Test tool",
                DeucarianControlCenterArea.Developer,
                () => { },
                "com.deucarian.editor",
                order: order);
        }

        private static string Unique(string prefix)
        {
            return "test." + prefix + "." + Guid.NewGuid().ToString("N");
        }

        private sealed class CardProvider :
            IDeucarianControlCenterCardProvider
        {
            private readonly DeucarianControlCenterCard[] cards;

            internal CardProvider(
                string id,
                params DeucarianControlCenterCard[] values)
            {
                Id = id;
                cards = values ?? Array.Empty<DeucarianControlCenterCard>();
            }

            public string Id { get; }

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                return cards;
            }
        }

        private sealed class SectionProvider :
            IDeucarianControlCenterSectionProvider
        {
            internal SectionProvider(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public IEnumerable<DeucarianControlCenterSection> Capture(
                DeucarianControlCenterContext context)
            {
                return Array.Empty<DeucarianControlCenterSection>();
            }
        }

        private sealed class ThrowingProvider :
            IDeucarianControlCenterCardProvider
        {
            private readonly string message;

            internal ThrowingProvider(string id, string failureMessage)
            {
                Id = id;
                message = failureMessage;
            }

            public string Id { get; }

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class CheckProvider :
            IDeucarianProjectCheckProvider
        {
            internal CheckProvider(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public void Evaluate(ICollection<DeucarianProjectIssue> issues)
            {
                issues.Add(new DeucarianProjectIssue(
                    Id + "-ISSUE",
                    DeucarianProjectIssueSeverity.Warning,
                    Id + ".package",
                    "Synthetic readiness issue."));
            }
        }
    }
}
