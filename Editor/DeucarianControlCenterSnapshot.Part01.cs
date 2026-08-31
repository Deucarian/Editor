using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public static partial class DeucarianControlCenterSnapshotBuilder
    {

        public static DeucarianControlCenterSnapshot Capture(
            bool explicitRefresh = false)
        {
            return Capture(new DeucarianControlCenterContext(
                DateTime.UtcNow,
                explicitRefresh));
        }

        public static DeucarianControlCenterSnapshot Capture(
            DeucarianControlCenterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cards = new List<DeucarianControlCenterCard>();
            var sections = new List<DeucarianControlCenterSection>();
            var cardIds = new HashSet<string>(StringComparer.Ordinal);
            var sectionIds = new HashSet<string>(StringComparer.Ordinal);
            var actionIds = new HashSet<string>(StringComparer.Ordinal);
            AddReadinessCards(cards, cardIds, actionIds);

            foreach (IDeucarianControlCenterCardProvider provider in
                     DeucarianControlCenterRegistry.GetCardProviders())
            {
                try
                {
                    IEnumerable<DeucarianControlCenterCard> captured =
                        provider.Capture(context);
                    AddCards(captured, cards, cardIds, actionIds, provider.Id);
                }
                catch (Exception exception)
                {
                    AddProviderFailure(cards, cardIds, provider.Id, exception);
                }
            }

            foreach (IDeucarianControlCenterSectionProvider provider in
                     DeucarianControlCenterRegistry.GetSectionProviders())
            {
                try
                {
                    IEnumerable<DeucarianControlCenterSection> captured =
                        provider.Capture(context);
                    AddSections(
                        captured,
                        sections,
                        cardIds,
                        sectionIds,
                        actionIds,
                        provider.Id);
                }
                catch (Exception exception)
                {
                    AddProviderFailure(cards, cardIds, provider.Id, exception);
                }
            }

            AddCards(
                DeucarianControlCenterOverview.Build(cards, sections),
                cards,
                cardIds,
                actionIds,
                "com.deucarian.editor.overview");

            cards.Sort(CompareCards);
            sections.Sort(CompareSections);
            return new DeucarianControlCenterSnapshot(
                context.CapturedAtUtc,
                cards,
                sections,
                DeucarianToolRegistry.GetTools());
        }

        private static void AddReadinessCards(
            ICollection<DeucarianControlCenterCard> cards,
            ISet<string> ids,
            ISet<string> actionIds)
        {
            IReadOnlyList<DeucarianProjectIssue> issues =
                DeucarianProjectValidationRegistry.Evaluate();
            int blockers = 0;
            int warnings = 0;
            foreach (DeucarianProjectIssue issue in issues)
            {
                if (issue.IsBlocking)
                {
                    blockers++;
                }
                else if (issue.Severity == DeucarianProjectIssueSeverity.Warning)
                {
                    warnings++;
                }
            }

            DeucarianControlCenterStatus status = blockers > 0
                ? DeucarianControlCenterStatus.Error
                : warnings > 0
                    ? DeucarianControlCenterStatus.Warning
                    : DeucarianControlCenterStatus.Success;
            string statusText = blockers > 0
                ? blockers + " blocker(s)"
                : warnings > 0
                    ? warnings + " warning(s)"
                    : "Ready";
            AddCard(
                new DeucarianControlCenterCard(
                    "deucarian.readiness.overview",
                    DeucarianControlCenterArea.Overview,
                    "Project readiness",
                    issues.Count == 0
                        ? "All installed package checks pass."
                        : issues.Count + " project readiness issue(s) reported by their owning packages.",
                    "com.deucarian.editor",
                    status,
                    statusText,
                    -1000,
                    new[]
                    {
                        "Errors: " + blockers,
                        "Warnings: " + warnings,
                        "Information: " + Math.Max(0, issues.Count - blockers - warnings)
                    },
                    new[]
                    {
                        new DeucarianControlCenterAction(
                            "deucarian.readiness.open-project",
                            "Review project checks",
                            () => DeucarianControlCenterWindow.Open(
                                DeucarianControlCenterArea.Project))
                    },
                    new[] { "project", "readiness", "validation", "blockers" }),
                cards,
                ids,
                actionIds,
                "com.deucarian.editor");

            if (issues.Count == 0)
            {
                AddCard(
                    new DeucarianControlCenterCard(
                        "deucarian.readiness.project",
                        DeucarianControlCenterArea.Project,
                        "Project checks",
                        "No blockers or warnings are currently reported.",
                        "com.deucarian.editor",
                        DeucarianControlCenterStatus.Success,
                        "Ready",
                        -1000,
                        searchTerms: new[] { "project", "validation", "checks" }),
                    cards,
                    ids,
                    actionIds,
                    "com.deucarian.editor");
                return;
            }

            foreach (DeucarianProjectIssue issue in issues)
            {
                string cardId = BuildIssueCardId(issue, ids);
                AddCard(
                    CreateIssueCard(issue, cardId),
                    cards,
                    ids,
                    actionIds,
                    issue.OwningPackage);
            }
        }

        private static DeucarianControlCenterCard CreateIssueCard(
            DeucarianProjectIssue issue,
            string cardId)
        {
            var actions = new List<DeucarianControlCenterAction>();
            AddIssueAction(actions, cardId, "fix", "Fix", issue.Fix);
            AddIssueAction(actions, cardId, "select", "Select", issue.Select);
            AddIssueAction(
                actions,
                cardId,
                "open-setup",
                "Open setup",
                issue.OpenSetup);
            var details = new List<string>();
            if (!string.IsNullOrWhiteSpace(issue.AffectedPath))
            {
                details.Add(issue.AffectedPath);
            }

            return new DeucarianControlCenterCard(
                cardId,
                DeucarianControlCenterArea.Project,
                issue.Code,
                issue.Explanation,
                issue.OwningPackage,
                ConvertStatus(issue.Severity),
                issue.Severity.ToString(),
                0,
                details,
                actions,
                new[] { issue.Code, issue.OwningPackage, "readiness", "issue" });
        }

        private static void AddIssueAction(
            ICollection<DeucarianControlCenterAction> actions,
            string cardId,
            string suffix,
            string label,
            Action action)
        {
            if (action != null)
            {
                actions.Add(new DeucarianControlCenterAction(
                    cardId + "." + suffix,
                    label,
                    action));
            }
        }

        private static string BuildIssueCardId(
            DeucarianProjectIssue issue,
            ISet<string> ids)
        {
            string baseId = "deucarian.readiness.issue." + issue.Code;
            if (!ids.Contains(baseId))
            {
                return baseId;
            }

            string owner = NormalizeIdPart(issue.OwningPackage);
            string candidate = baseId + "." + owner;
            int ordinal = 2;
            while (ids.Contains(candidate))
            {
                candidate = baseId + "." + owner + "." + ordinal;
                ordinal++;
            }

            return candidate;
        }

        private static string NormalizeIdPart(string value)
        {
            var result = new System.Text.StringBuilder();
            foreach (char character in value ?? string.Empty)
            {
                bool allowed = char.IsLetterOrDigit(character) ||
                    character == '.' || character == '-';
                result.Append(allowed
                    ? char.ToLowerInvariant(character)
                    : '-');
            }

            return result.Length == 0 ? "unknown" : result.ToString();
        }

        private static DeucarianControlCenterStatus ConvertStatus(
            DeucarianProjectIssueSeverity severity)
        {
            switch (severity)
            {
                case DeucarianProjectIssueSeverity.Error:
                    return DeucarianControlCenterStatus.Error;
                case DeucarianProjectIssueSeverity.Warning:
                    return DeucarianControlCenterStatus.Warning;
                default:
                    return DeucarianControlCenterStatus.Info;
            }
        }

        private static void AddCards(
            IEnumerable<DeucarianControlCenterCard> captured,
            ICollection<DeucarianControlCenterCard> cards,
            ISet<string> ids,
            ISet<string> actionIds,
            string providerId)
        {
            if (captured == null)
            {
                return;
            }

            foreach (DeucarianControlCenterCard card in captured)
            {
                if (card != null)
                {
                    AddCard(card, cards, ids, actionIds, providerId);
                }
            }
        }

        private static void AddCard(
            DeucarianControlCenterCard card,
            ICollection<DeucarianControlCenterCard> cards,
            ISet<string> ids,
            ISet<string> actionIds,
            string providerId)
        {
            ValidateArea(card.Area, providerId, "card '" + card.Id + "'");
            if (!ids.Add(card.Id))
            {
                throw new InvalidOperationException(
                    "Control Center card '" + card.Id + "' from provider '" +
                    providerId + "' duplicates an existing card.");
            }

            AddActionIds(card.Id, card.Actions, actionIds, providerId);
            cards.Add(card);
        }

        private static void AddSections(
            IEnumerable<DeucarianControlCenterSection> captured,
            ICollection<DeucarianControlCenterSection> sections,
            ISet<string> cardIds,
            ISet<string> sectionIds,
            ISet<string> actionIds,
            string providerId)
        {
            if (captured == null)
            {
                return;
            }

            foreach (DeucarianControlCenterSection section in captured)
            {
                if (section == null)
                {
                    continue;
                }

                if (section.Cards.Count == 0)
                {
                    continue;
                }

                ValidateArea(
                    section.Area,
                    providerId,
                    "section '" + section.Id + "'");
                if (!sectionIds.Add(section.Id))
                {
                    throw new InvalidOperationException(
                        "Control Center section '" + section.Id +
                        "' from provider '" + providerId +
                        "' duplicates an existing section.");
                }

                foreach (DeucarianControlCenterCard card in section.Cards)
                {
                    ValidateArea(card.Area, providerId, "card '" + card.Id + "'");
                    if (card.Area != section.Area)
                    {
                        throw new InvalidOperationException(
                            "Control Center card '" + card.Id +
                            "' must use its section area " + section.Area + ".");
                    }

                    if (!cardIds.Add(card.Id))
                    {
                        throw new InvalidOperationException(
                            "Control Center card '" + card.Id +
                            "' from provider '" + providerId +
                            "' duplicates an existing card.");
                    }

                    AddActionIds(card.Id, card.Actions, actionIds, providerId);
                }

                sections.Add(section);
            }
        }

        private static void AddActionIds(
            string cardId,
            IEnumerable<DeucarianControlCenterAction> actions,
            ISet<string> actionIds,
            string providerId)
        {
            foreach (DeucarianControlCenterAction action in actions)
            {
                if (!actionIds.Add(cardId + "\n" + action.Id))
                {
                    throw new InvalidOperationException(
                        "Control Center action '" + action.Id +
                        "' on card '" + cardId + "' from provider '" +
                        providerId + "' duplicates an existing action.");
                }
            }
        }

        private static void ValidateArea(
            DeucarianControlCenterArea area,
            string providerId,
            string contribution)
        {
            if (!Enum.IsDefined(typeof(DeucarianControlCenterArea), area))
            {
                throw new InvalidOperationException(
                    "Control Center " + contribution + " from provider '" +
                    providerId + "' uses an unknown area.");
            }
        }
    }
}
