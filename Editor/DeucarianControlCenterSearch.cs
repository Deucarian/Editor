using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public enum DeucarianControlCenterSearchResultKind
    {
        Area,
        Card,
        Tool,
        Action
    }

    public sealed class DeucarianControlCenterSearchResult
    {
        internal DeucarianControlCenterSearchResult(
            DeucarianControlCenterSearchResultKind kind,
            DeucarianControlCenterArea area,
            string targetId,
            string title,
            string description,
            int score,
            Action invoke,
            bool requiresConfirmation)
        {
            Kind = kind;
            Area = area;
            TargetId = targetId;
            Title = title;
            Description = description;
            Score = score;
            this.invoke = invoke;
            RequiresConfirmation = requiresConfirmation;
        }

        private readonly Action invoke;
        public DeucarianControlCenterSearchResultKind Kind { get; }
        public DeucarianControlCenterArea Area { get; }
        public string TargetId { get; }
        public string Title { get; }
        public string Description { get; }
        public int Score { get; }
        public bool CanInvoke => invoke != null;
        public bool RequiresConfirmation { get; }
        public void Invoke() => invoke?.Invoke();
    }

    public static class DeucarianControlCenterSearch
    {
        public static IReadOnlyList<DeucarianControlCenterSearchResult> Search(
            DeucarianControlCenterSnapshot snapshot,
            string query)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            string[] terms = SplitTerms(query);
            var results = new List<DeucarianControlCenterSearchResult>();
            if (terms.Length == 0)
            {
                return results;
            }

            foreach (DeucarianControlCenterArea area in snapshot.Areas)
            {
                string name = DeucarianControlCenterAreaIds.GetDisplayName(area);
                AddIfMatch(
                    results,
                    terms,
                    DeucarianControlCenterSearchResultKind.Area,
                    area,
                    DeucarianControlCenterAreaIds.GetId(area),
                    name,
                    "Open the " + name + " area.",
                    null,
                    false,
                    new[] { name, DeucarianControlCenterAreaIds.GetId(area) });
            }

            foreach (DeucarianControlCenterCard card in snapshot.Cards)
            {
                AddCard(results, terms, card);
            }

            foreach (DeucarianControlCenterSection section in snapshot.Sections)
            {
                foreach (DeucarianControlCenterCard card in section.Cards)
                {
                    AddCard(results, terms, card);
                }
            }

            foreach (DeucarianToolDescriptor tool in snapshot.Tools)
            {
                if (tool.Id == DeucarianToolIds.ControlCenter) continue;
                AddIfMatch(
                    results,
                    terms,
                    DeucarianControlCenterSearchResultKind.Tool,
                    tool.Area,
                    tool.Id,
                    tool.DisplayName,
                    tool.Description,
                    tool.Open,
                    false,
                    Combine(
                        tool.DisplayName,
                        tool.Description,
                        tool.OwningPackage,
                        tool.SearchTerms));
            }

            results.Sort(Compare);
            return results;
        }

        private static void AddCard(
            ICollection<DeucarianControlCenterSearchResult> results,
            string[] terms,
            DeucarianControlCenterCard card)
        {
            AddIfMatch(
                results,
                terms,
                DeucarianControlCenterSearchResultKind.Card,
                card.Area,
                card.Id,
                card.Title,
                card.Description,
                null,
                false,
                Combine(
                    card.Title,
                    card.Description,
                    card.OwningPackage,
                    new[] { card.StatusText },
                    card.Details,
                    card.SearchTerms));
            foreach (DeucarianControlCenterAction action in card.Actions)
            {
                AddIfMatch(
                    results,
                    terms,
                    DeucarianControlCenterSearchResultKind.Action,
                    card.Area,
                    action.Id,
                    action.Label,
                    action.Description.Length == 0
                        ? card.Title
                        : action.Description,
                    action.Invoke,
                    action.RequiresConfirmation,
                    Combine(
                        action.Label,
                        action.Description,
                        card.Title,
                        new[] { card.OwningPackage },
                        action.SearchTerms));
            }
        }

        private static void AddIfMatch(
            ICollection<DeucarianControlCenterSearchResult> results,
            string[] terms,
            DeucarianControlCenterSearchResultKind kind,
            DeucarianControlCenterArea area,
            string targetId,
            string title,
            string description,
            Action invoke,
            bool requiresConfirmation,
            IEnumerable<string> searchable)
        {
            string text = string.Join(" ", searchable).ToLowerInvariant();
            foreach (string term in terms)
            {
                if (text.IndexOf(term, StringComparison.Ordinal) < 0)
                {
                    return;
                }
            }

            string safeTitle = title ?? string.Empty;
            int score = safeTitle.IndexOf(
                terms[0],
                StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 10;
            results.Add(new DeucarianControlCenterSearchResult(
                kind,
                area,
                targetId,
                safeTitle,
                description ?? string.Empty,
                score,
                invoke,
                requiresConfirmation));
        }

        private static IEnumerable<string> Combine(
            string first,
            string second,
            string third,
            params IEnumerable<string>[] collections)
        {
            yield return first ?? string.Empty;
            yield return second ?? string.Empty;
            yield return third ?? string.Empty;
            foreach (IEnumerable<string> collection in collections)
            {
                if (collection == null) continue;
                foreach (string value in collection)
                {
                    yield return value ?? string.Empty;
                }
            }
        }

        private static string[] SplitTerms(string query)
        {
            return string.IsNullOrWhiteSpace(query)
                ? Array.Empty<string>()
                : query.Trim().ToLowerInvariant().Split(
                    new[] { ' ', '\t', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
        }

        private static int Compare(
            DeucarianControlCenterSearchResult left,
            DeucarianControlCenterSearchResult right)
        {
            int score = left.Score.CompareTo(right.Score);
            if (score != 0) return score;
            int area = left.Area.CompareTo(right.Area);
            if (area != 0) return area;
            int kind = left.Kind.CompareTo(right.Kind);
            if (kind != 0) return kind;
            int title = string.Compare(
                left.Title,
                right.Title,
                StringComparison.OrdinalIgnoreCase);
            return title != 0
                ? title
                : string.Compare(left.TargetId, right.TargetId, StringComparison.Ordinal);
        }
    }
}
