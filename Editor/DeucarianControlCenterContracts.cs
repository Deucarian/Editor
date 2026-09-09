using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public enum DeucarianControlCenterArea
    {
        Overview = 0,
        Project = 100,
        Connections = 200,
        Experience = 300,
        Communication = 400,
        BuildAndPackages = 500,
        Authoring = 600,
        Diagnostics = 700,
        Developer = 800
    }

    public enum DeucarianControlCenterStatus
    {
        None = 0,
        Info = 1,
        Success = 2,
        Warning = 3,
        Error = 4
    }

    public static class DeucarianControlCenterAreaIds
    {
        public const string Overview = "overview";
        public const string Project = "project";
        public const string Connections = "connections";
        public const string Experience = "experience";
        public const string Communication = "communication";
        public const string BuildAndPackages = "build-and-packages";
        public const string Authoring = "authoring";
        public const string Diagnostics = "diagnostics";
        public const string Developer = "developer";

        public static string GetId(DeucarianControlCenterArea area)
        {
            switch (area)
            {
                case DeucarianControlCenterArea.Overview: return Overview;
                case DeucarianControlCenterArea.Project: return Project;
                case DeucarianControlCenterArea.Connections: return Connections;
                case DeucarianControlCenterArea.Experience: return Experience;
                case DeucarianControlCenterArea.Communication: return Communication;
                case DeucarianControlCenterArea.BuildAndPackages: return BuildAndPackages;
                case DeucarianControlCenterArea.Authoring: return Authoring;
                case DeucarianControlCenterArea.Diagnostics: return Diagnostics;
                case DeucarianControlCenterArea.Developer: return Developer;
                default: throw new ArgumentOutOfRangeException(nameof(area));
            }
        }

        public static string GetDisplayName(DeucarianControlCenterArea area)
        {
            return area == DeucarianControlCenterArea.BuildAndPackages
                ? "Build & Packages"
                : area.ToString();
        }
    }

    public sealed class DeucarianControlCenterContext
    {
        public DeucarianControlCenterContext(DateTime capturedAtUtc, bool explicitRefresh)
        {
            CapturedAtUtc = capturedAtUtc.Kind == DateTimeKind.Utc
                ? capturedAtUtc
                : capturedAtUtc.ToUniversalTime();
            IsExplicitRefresh = explicitRefresh;
        }

        public DateTime CapturedAtUtc { get; }
        public bool IsExplicitRefresh { get; }
    }

    public sealed class DeucarianControlCenterAction
    {
        private readonly Action invoke;

        public DeucarianControlCenterAction(
            string id,
            string label,
            Action invoke,
            string description = null,
            IEnumerable<string> searchTerms = null,
            bool requiresConfirmation = false,
            string navigationToolId = null,
            string navigationRoute = null)
        {
            Id = Require(id, nameof(id));
            Label = Require(label, nameof(label));
            this.invoke = invoke ?? throw new ArgumentNullException(nameof(invoke));
            Description = Clean(description);
            SearchTerms = Copy(searchTerms);
            RequiresConfirmation = requiresConfirmation;
            NavigationToolId = Clean(navigationToolId);
            NavigationRoute = Clean(navigationRoute);
        }

        public string Id { get; }
        public string Label { get; }
        public string Description { get; }
        public IReadOnlyList<string> SearchTerms { get; }
        public bool RequiresConfirmation { get; }
        public string NavigationToolId { get; }
        public string NavigationRoute { get; }
        public void Invoke() => invoke();

        internal static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A non-empty value is required.", parameterName);
            }

            return value.Trim();
        }

        internal static string Clean(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        internal static IReadOnlyList<string> Copy(IEnumerable<string> values)
        {
            var result = new List<string>();
            if (values == null)
            {
                return result;
            }

            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Add(value.Trim());
                }
            }

            return result;
        }
    }

    public sealed class DeucarianControlCenterCard
    {
        public DeucarianControlCenterCard(
            string id,
            DeucarianControlCenterArea area,
            string title,
            string description,
            string owningPackage,
            DeucarianControlCenterStatus status = DeucarianControlCenterStatus.None,
            string statusText = null,
            int order = 0,
            IEnumerable<string> details = null,
            IEnumerable<DeucarianControlCenterAction> actions = null,
            IEnumerable<string> searchTerms = null)
        {
            Id = DeucarianControlCenterAction.Require(id, nameof(id));
            Area = area;
            Title = DeucarianControlCenterAction.Require(title, nameof(title));
            Description = DeucarianControlCenterAction.Clean(description);
            OwningPackage = DeucarianControlCenterAction.Require(
                owningPackage,
                nameof(owningPackage));
            Status = status;
            StatusText = DeucarianControlCenterAction.Clean(statusText);
            Order = order;
            Details = DeucarianControlCenterAction.Copy(details);
            Actions = CopyActions(actions);
            SearchTerms = DeucarianControlCenterAction.Copy(searchTerms);
        }

        public string Id { get; }
        public DeucarianControlCenterArea Area { get; }
        public string Title { get; }
        public string Description { get; }
        public string OwningPackage { get; }
        public DeucarianControlCenterStatus Status { get; }
        public string StatusText { get; }
        public int Order { get; }
        public IReadOnlyList<string> Details { get; }
        public IReadOnlyList<DeucarianControlCenterAction> Actions { get; }
        public IReadOnlyList<string> SearchTerms { get; }

        private static IReadOnlyList<DeucarianControlCenterAction> CopyActions(
            IEnumerable<DeucarianControlCenterAction> actions)
        {
            var result = new List<DeucarianControlCenterAction>();
            if (actions == null)
            {
                return result;
            }

            foreach (DeucarianControlCenterAction action in actions)
            {
                if (action != null)
                {
                    result.Add(action);
                }
            }

            return result;
        }
    }

    public sealed class DeucarianControlCenterSection
    {
        public DeucarianControlCenterSection(
            string id,
            DeucarianControlCenterArea area,
            string title,
            IEnumerable<DeucarianControlCenterCard> cards,
            string description = null,
            int order = 0)
        {
            Id = DeucarianControlCenterAction.Require(id, nameof(id));
            Area = area;
            Title = DeucarianControlCenterAction.Require(title, nameof(title));
            Description = DeucarianControlCenterAction.Clean(description);
            Order = order;
            Cards = CopyCards(cards);
        }

        public string Id { get; }
        public DeucarianControlCenterArea Area { get; }
        public string Title { get; }
        public string Description { get; }
        public int Order { get; }
        public IReadOnlyList<DeucarianControlCenterCard> Cards { get; }

        private static IReadOnlyList<DeucarianControlCenterCard> CopyCards(
            IEnumerable<DeucarianControlCenterCard> cards)
        {
            var result = new List<DeucarianControlCenterCard>();
            if (cards == null)
            {
                return result;
            }

            foreach (DeucarianControlCenterCard card in cards)
            {
                if (card != null)
                {
                    result.Add(card);
                }
            }

            return result;
        }
    }

    public interface IDeucarianControlCenterCardProvider
    {
        string Id { get; }
        IEnumerable<DeucarianControlCenterCard> Capture(
            DeucarianControlCenterContext context);
    }

    public interface IDeucarianControlCenterSectionProvider
    {
        string Id { get; }
        IEnumerable<DeucarianControlCenterSection> Capture(
            DeucarianControlCenterContext context);
    }
}
