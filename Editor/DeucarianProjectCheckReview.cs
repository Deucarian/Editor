using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    internal enum DeucarianProjectCheckFilter { All, Errors, Warnings, Information }

    internal static class DeucarianProjectCheckReview
    {
        internal static List<DeucarianControlCenterCard> Collect(DeucarianControlCenterSnapshot snapshot)
        {
            var result = new List<DeucarianControlCenterCard>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var card in AllCards(snapshot))
            {
                if (IsSummary(card) || !ids.Add(card.Id)) continue;
                if (card.Area == DeucarianControlCenterArea.Project || card.Status >= DeucarianControlCenterStatus.Warning)
                    result.Add(card);
            }
            bool hasIssue = result.Exists(card => card.Status >= DeucarianControlCenterStatus.Warning);
            if (hasIssue) result.RemoveAll(card => card.Id == "deucarian.readiness.project");
            result.Sort((left, right) =>
            {
                int severity = right.Status.CompareTo(left.Status);
                return severity != 0 ? severity : StringComparer.Ordinal.Compare(left.Id, right.Id);
            });
            return result;
        }

        internal static bool Matches(DeucarianControlCenterCard card, DeucarianProjectCheckFilter filter)
        {
            switch (filter)
            {
                case DeucarianProjectCheckFilter.Errors: return card.Status == DeucarianControlCenterStatus.Error;
                case DeucarianProjectCheckFilter.Warnings: return card.Status == DeucarianControlCenterStatus.Warning;
                case DeucarianProjectCheckFilter.Information: return card.Status == DeucarianControlCenterStatus.Info;
                default: return true;
            }
        }

        internal static IEnumerable<DeucarianControlCenterCard> AllCards(DeucarianControlCenterSnapshot snapshot)
        {
            foreach (var card in snapshot.Cards) yield return card;
            foreach (var section in snapshot.Sections)
                foreach (var card in section.Cards) yield return card;
        }

        internal static bool IsSummary(DeucarianControlCenterCard card) =>
            card.Id == "deucarian.readiness.overview" ||
            DeucarianControlCenterOverviewPresentation.TryGetSummaryArea(card.Id, out _);
    }
}
