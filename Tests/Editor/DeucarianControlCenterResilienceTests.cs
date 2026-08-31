using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianControlCenterResilienceTests
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
        public void OverviewProjectsAuthoritativeDomainStatus()
        {
            string prefix = Unique("overview");
            IDisposable registration =
                DeucarianControlCenterRegistry.RegisterCardProvider(
                    new CardProvider(
                        prefix,
                        Card(
                            prefix + ".connection",
                            DeucarianControlCenterArea.Connections,
                            "Effective environment",
                            DeucarianControlCenterStatus.Warning),
                        Card(
                            prefix + ".build",
                            DeucarianControlCenterArea.BuildAndPackages,
                            "Selected build profile",
                            DeucarianControlCenterStatus.Success),
                        Card(
                            prefix + ".diagnostics",
                            DeucarianControlCenterArea.Diagnostics,
                            "Aggregate severity",
                            DeucarianControlCenterStatus.Info)));
            try
            {
                DeucarianControlCenterSnapshot snapshot =
                    DeucarianControlCenterSnapshotBuilder.Capture();
                AssertSummaryContains(
                    snapshot,
                    "deucarian.overview.connections",
                    "Effective environment");
                AssertSummaryContains(
                    snapshot,
                    "deucarian.overview.build-packages",
                    "Selected build profile");
                AssertSummaryContains(
                    snapshot,
                    "deucarian.overview.diagnostics",
                    "Aggregate severity");
            }
            finally
            {
                registration.Dispose();
            }
        }

        [Test]
        public void TwoThrowingReadinessProvidersRemainIsolated()
        {
            string firstId = Unique("readiness-first");
            string secondId = Unique("readiness-second");
            IDisposable first = DeucarianProjectValidationRegistry.Register(
                new ThrowingCheckProvider(firstId));
            IDisposable second = DeucarianProjectValidationRegistry.Register(
                new ThrowingCheckProvider(secondId));
            try
            {
                DeucarianControlCenterSnapshot snapshot =
                    DeucarianControlCenterSnapshotBuilder.Capture();
                DeucarianControlCenterCard[] failures = snapshot.Cards
                    .Where(card => card.Title == "DEU-SETUP-001")
                    .Where(card => card.OwningPackage == firstId ||
                        card.OwningPackage == secondId)
                    .ToArray();
                Assert.AreEqual(2, failures.Length);
                Assert.AreEqual(2, failures.Select(card => card.Id).Distinct().Count());
                Assert.That(
                    failures.Select(card => card.Id),
                    Does.Contain("deucarian.readiness.issue.DEU-SETUP-001"));
            }
            finally
            {
                second.Dispose();
                first.Dispose();
            }
        }

        [Test]
        public void MalformedCardContributionBecomesProviderFailure()
        {
            string prefix = Unique("bad-card");
            var duplicateAction = new DeucarianControlCenterAction(
                prefix + ".action",
                "Action",
                () => { });
            IDisposable registration =
                DeucarianControlCenterRegistry.RegisterCardProvider(
                    new CardProvider(
                        prefix,
                        Card(
                            prefix + ".first",
                            DeucarianControlCenterArea.Connections,
                            "First",
                            DeucarianControlCenterStatus.Info,
                            duplicateAction,
                            duplicateAction)));
            try
            {
                AssertProviderFailure(prefix);
            }
            finally
            {
                registration.Dispose();
            }
        }

        [Test]
        public void MalformedSectionContributionBecomesProviderFailure()
        {
            string prefix = Unique("bad-section");
            var mismatch = new DeucarianControlCenterSection(
                prefix + ".section",
                DeucarianControlCenterArea.Authoring,
                "Authoring",
                new[]
                {
                    Card(
                        prefix + ".card",
                        DeucarianControlCenterArea.Developer,
                        "Wrong area",
                        DeucarianControlCenterStatus.Info)
                });
            IDisposable registration =
                DeucarianControlCenterRegistry.RegisterSectionProvider(
                    new SectionProvider(prefix, mismatch));
            try
            {
                AssertProviderFailure(prefix);
            }
            finally
            {
                registration.Dispose();
            }
        }

        [Test]
        public void DuplicateSectionIdBecomesProviderFailure()
        {
            string prefix = Unique("duplicate-section");
            var first = new DeucarianControlCenterSection(
                prefix + ".section",
                DeucarianControlCenterArea.Authoring,
                "First",
                new[]
                {
                    Card(
                        prefix + ".first-card",
                        DeucarianControlCenterArea.Authoring,
                        "First card",
                        DeucarianControlCenterStatus.Info)
                });
            var second = new DeucarianControlCenterSection(
                prefix + ".section",
                DeucarianControlCenterArea.Authoring,
                "Second",
                new[]
                {
                    Card(
                        prefix + ".second-card",
                        DeucarianControlCenterArea.Authoring,
                        "Second card",
                        DeucarianControlCenterStatus.Info)
                });
            IDisposable registration =
                DeucarianControlCenterRegistry.RegisterSectionProvider(
                    new SectionProvider(prefix, first, second));
            try
            {
                AssertProviderFailure(prefix);
            }
            finally
            {
                registration.Dispose();
            }
        }

        [Test]
        public void UnknownAreaBecomesProviderFailure()
        {
            string prefix = Unique("unknown-area");
            IDisposable registration =
                DeucarianControlCenterRegistry.RegisterCardProvider(
                    new CardProvider(
                        prefix,
                        Card(
                            prefix + ".card",
                            (DeucarianControlCenterArea)9999,
                            "Unknown",
                            DeucarianControlCenterStatus.Info)));
            try
            {
                AssertProviderFailure(prefix);
            }
            finally
            {
                registration.Dispose();
            }
        }

        [Test]
        public void ToolDescriptorRejectsUnknownArea()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DeucarianToolDescriptor(
                    Unique("tool"),
                    "Unknown",
                    "Invalid area",
                    (DeucarianControlCenterArea)9999,
                    () => { },
                    "com.deucarian.editor.tests"));
        }

        [Test]
        public void OpenRoutesResetOrPersistSearchAsIntended()
        {
            DeucarianControlCenterWindow window =
                DeucarianControlCenterWindow.Open();
            window.CreateGUI();
            window.rootVisualElement
                .Q<TextField>("control-center-search")
                .value = "diagnostics";
            Assert.AreEqual("diagnostics", window.SearchQuery);

            DeucarianControlCenterWindow.Open(
                DeucarianControlCenterArea.Developer);
            Assert.AreEqual(string.Empty, window.SearchQuery);

            MethodInfo legacy = typeof(DeucarianControlCenterWindow).GetMethod(
                "OpenLegacyMenu",
                BindingFlags.Static | BindingFlags.NonPublic);
            legacy.Invoke(null, null);
            Assert.AreEqual("legacy shortcuts", window.SearchQuery);

            DeucarianControlCenterWindow.OpenProjectIssue("FOCUSED");
            Assert.AreEqual(string.Empty, window.SearchQuery);
            Assert.AreEqual(
                "deucarian.readiness.issue.FOCUSED",
                window.FocusedTargetId);
        }

        [Test]
        public void SuccessfulActionRequestsImmediateRefresh()
        {
            int invoked = 0;
            int refreshed = 0;
            using (var view = new DeucarianControlCenterView(
                       (_, __) => { },
                       () => refreshed++))
            {
                MethodInfo invoke = typeof(DeucarianControlCenterView).GetMethod(
                    "InvokeSafely",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                invoke.Invoke(
                    view,
                    new object[] { "Test action", (Action)(() => invoked++) });
            }

            Assert.AreEqual(1, invoked);
            Assert.AreEqual(1, refreshed);
        }

        private static void AssertSummaryContains(
            DeucarianControlCenterSnapshot snapshot,
            string cardId,
            string expectedDetail)
        {
            DeucarianControlCenterCard card = snapshot.Cards.Single(
                candidate => candidate.Id == cardId);
            Assert.That(card.Area, Is.EqualTo(DeucarianControlCenterArea.Overview));
            Assert.That(string.Join(" ", card.Details), Does.Contain(expectedDetail));
            Assert.That(card.Actions, Is.Not.Empty);
        }

        private static void AssertProviderFailure(string providerId)
        {
            Assert.That(
                DeucarianControlCenterSnapshotBuilder.Capture()
                    .Cards.Select(card => card.Id),
                Does.Contain("deucarian.provider-failure." + providerId));
        }

        private static DeucarianControlCenterCard Card(
            string id,
            DeucarianControlCenterArea area,
            string title,
            DeucarianControlCenterStatus status,
            params DeucarianControlCenterAction[] actions)
        {
            return new DeucarianControlCenterCard(
                id,
                area,
                title,
                "Test contribution",
                "com.deucarian.editor.tests",
                status,
                status.ToString(),
                actions: actions);
        }

        private static string Unique(string prefix)
        {
            return "test." + prefix + "." + Guid.NewGuid().ToString("N");
        }

        private sealed class CardProvider : IDeucarianControlCenterCardProvider
        {
            private readonly DeucarianControlCenterCard[] cards;

            internal CardProvider(
                string id,
                params DeucarianControlCenterCard[] values)
            {
                Id = id;
                cards = values;
            }

            public string Id { get; }

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                return cards;
            }
        }

        private sealed class SectionProvider : IDeucarianControlCenterSectionProvider
        {
            private readonly DeucarianControlCenterSection[] sections;

            internal SectionProvider(
                string id,
                params DeucarianControlCenterSection[] values)
            {
                Id = id;
                sections = values;
            }

            public string Id { get; }

            public IEnumerable<DeucarianControlCenterSection> Capture(
                DeucarianControlCenterContext context)
            {
                return sections;
            }
        }

        private sealed class ThrowingCheckProvider : IDeucarianProjectCheckProvider
        {
            internal ThrowingCheckProvider(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public void Evaluate(ICollection<DeucarianProjectIssue> issues)
            {
                throw new InvalidOperationException("private-test-marker");
            }
        }
    }
}
