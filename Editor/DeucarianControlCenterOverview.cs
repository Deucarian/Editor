using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterOverview
    {
        private readonly struct SummaryDefinition
        {
            internal SummaryDefinition(
                DeucarianControlCenterArea area,
                string id,
                string title,
                string description,
                int order,
                string[] searchTerms)
            {
                Area = area;
                Id = id;
                Title = title;
                Description = description;
                Order = order;
                SearchTerms = searchTerms;
            }

            internal DeucarianControlCenterArea Area { get; }
            internal string Id { get; }
            internal string Title { get; }
            internal string Description { get; }
            internal int Order { get; }
            internal string[] SearchTerms { get; }
        }

        private static readonly SummaryDefinition[] Definitions =
        {
            new SummaryDefinition(
                DeucarianControlCenterArea.Connections,
                "deucarian.overview.connections",
                "Connections",
                "Connection and sign-in status.",
                -900,
                new[] { "connections", "environment", "authentication", "api" }),
            new SummaryDefinition(
                DeucarianControlCenterArea.BuildAndPackages,
                "deucarian.overview.build-packages",
                "Build & packages",
                "Package and build checks.",
                -800,
                new[] { "packages", "updates", "build", "profile", "target" }),
            new SummaryDefinition(
                DeucarianControlCenterArea.Diagnostics,
                "deucarian.overview.diagnostics",
                "Diagnostics",
                "Local health and diagnostic checks.",
                -700,
                new[] { "diagnostics", "logging", "health", "severity" })
        };

        internal static IEnumerable<DeucarianControlCenterCard> Build(
            IReadOnlyList<DeucarianControlCenterCard> cards,
            IReadOnlyList<DeucarianControlCenterSection> sections)
        {
            foreach (SummaryDefinition definition in Definitions)
            {
                List<DeucarianControlCenterCard> sources =
                    GetCards(definition.Area, cards, sections);
                if (sources.Count > 0)
                {
                    yield return CreateSummary(definition, sources);
                }
            }
        }

        private static List<DeucarianControlCenterCard> GetCards(
            DeucarianControlCenterArea area,
            IReadOnlyList<DeucarianControlCenterCard> cards,
            IReadOnlyList<DeucarianControlCenterSection> sections)
        {
            var result = new List<DeucarianControlCenterCard>();
            foreach (DeucarianControlCenterCard card in cards)
            {
                if (card.Area == area)
                {
                    result.Add(card);
                }
            }

            foreach (DeucarianControlCenterSection section in sections)
            {
                if (section.Area == area)
                {
                    result.AddRange(section.Cards);
                }
            }

            result.Sort((left, right) =>
                string.Compare(left.Id, right.Id, StringComparison.Ordinal));
            return result;
        }

        private static DeucarianControlCenterCard CreateSummary(
            SummaryDefinition definition,
            IReadOnlyList<DeucarianControlCenterCard> sources)
        {
            int errors = 0;
            int warnings = 0;
            int successes = 0;
            var details = new List<string>();
            foreach (DeucarianControlCenterCard source in sources)
            {
                if (source.Status == DeucarianControlCenterStatus.Error) errors++;
                if (source.Status == DeucarianControlCenterStatus.Warning) warnings++;
                if (source.Status == DeucarianControlCenterStatus.Success) successes++;
                if (details.Count < 6)
                {
                    details.Add(source.Title + ": " + DisplayStatus(source));
                }
            }

            if (sources.Count > details.Count)
            {
                details.Add("Additional installed contributions: " +
                    (sources.Count - details.Count));
            }

            DeucarianControlCenterArea area = definition.Area;
            DeucarianControlCenterStatus status = errors > 0
                ? DeucarianControlCenterStatus.Error
                : warnings > 0
                    ? DeucarianControlCenterStatus.Warning
                    : successes == sources.Count
                        ? DeucarianControlCenterStatus.Success
                        : DeucarianControlCenterStatus.Info;
            return new DeucarianControlCenterCard(
                definition.Id,
                DeucarianControlCenterArea.Overview,
                definition.Title,
                definition.Description,
                "com.deucarian.editor",
                status,
                sources.Count == 1 ? DisplayStatus(sources[0]) : BuildStatusText(errors, warnings, successes, sources.Count),
                definition.Order,
                details,
                new[]
                {
                    new DeucarianControlCenterAction(
                        definition.Id + ".open",
                        "Review " +
                            DeucarianControlCenterAreaIds.GetDisplayName(area),
                        () => DeucarianControlCenterWindow.Open(area))
                },
                definition.SearchTerms);
        }


        private static string DisplayStatus(DeucarianControlCenterCard card)
        {
            return string.IsNullOrWhiteSpace(card.StatusText)
                ? card.Status.ToString()
                : card.StatusText;
        }

        private static string BuildStatusText(
            int errors,
            int warnings,
            int successes,
            int total)
        {
            if (errors > 0) return errors + (errors == 1 ? " issue" : " issues");
            if (warnings > 0) return warnings + (warnings == 1 ? " warning" : " warnings");
            if (successes == total) return "Ready";
            return "Review status";
        }
    }
}
