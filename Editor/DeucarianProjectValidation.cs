using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Deucarian.Editor
{
    public enum DeucarianProjectIssueSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>One actionable, CI-safe project configuration issue.</summary>
    public sealed class DeucarianProjectIssue
    {
        public DeucarianProjectIssue(
            string code,
            DeucarianProjectIssueSeverity severity,
            string owningPackage,
            string explanation,
            string affectedPath = null,
            Action fix = null,
            Action select = null,
            Action openSetup = null,
            string setupToolId = null,
            string setupRoute = null)
        {
            Code = Require(code, nameof(code));
            Severity = severity;
            OwningPackage = Require(owningPackage, nameof(owningPackage));
            Explanation = Require(explanation, nameof(explanation));
            AffectedPath = affectedPath?.Trim() ?? string.Empty;
            Fix = fix;
            Select = select;
            OpenSetup = openSetup;
            SetupToolId = setupToolId;
            SetupRoute = setupRoute;
        }

        public string Code { get; }
        public DeucarianProjectIssueSeverity Severity { get; }
        public string OwningPackage { get; }
        public string Explanation { get; }
        public string AffectedPath { get; }
        public Action Fix { get; }
        public Action Select { get; }
        public Action OpenSetup { get; }
        public string SetupToolId { get; }
        public string SetupRoute { get; }
        public bool IsBlocking => Severity == DeucarianProjectIssueSeverity.Error;

        public override string ToString()
        {
            return Code + " " + Explanation +
                (AffectedPath.Length == 0 ? string.Empty :
                    " (" + AffectedPath + ")");
        }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "A non-empty issue value is required.",
                    parameterName);
            }

            return value.Trim();
        }
    }

    public interface IDeucarianProjectCheckProvider
    {
        string Id { get; }
        void Evaluate(ICollection<DeucarianProjectIssue> issues);
    }

    /// <summary>Process-local registry for package-contributed project checks.</summary>
    public static class DeucarianProjectValidationRegistry
    {
        private static readonly object Gate = new object();
        private static readonly List<IDeucarianProjectCheckProvider> Providers =
            new List<IDeucarianProjectCheckProvider>();

        public static event Action Changed;

        public static IDisposable Register(
            IDeucarianProjectCheckProvider provider)
        {
            if (provider == null || string.IsNullOrWhiteSpace(provider.Id))
            {
                throw new ArgumentException(
                    "A project check provider with a stable ID is required.",
                    nameof(provider));
            }

            lock (Gate)
            {
                foreach (IDeucarianProjectCheckProvider existing in Providers)
                {
                    if (string.Equals(
                            existing.Id,
                            provider.Id,
                            StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "Project check provider '" + provider.Id +
                            "' is already registered.");
                    }
                }

                Providers.Add(provider);
            }

            Changed?.Invoke();
            return new Registration(provider);
        }

        public static IReadOnlyList<DeucarianProjectIssue> Evaluate()
        {
            IDeucarianProjectCheckProvider[] providers;
            lock (Gate)
            {
                providers = Providers.ToArray();
            }

            var issues = new List<DeucarianProjectIssue>();
            foreach (IDeucarianProjectCheckProvider provider in providers)
            {
                try
                {
                    provider.Evaluate(issues);
                }
                catch (Exception exception)
                {
                    issues.Add(new DeucarianProjectIssue(
                        "DEU-SETUP-001",
                        DeucarianProjectIssueSeverity.Error,
                        provider.Id,
                        "Project validation failed internally (" +
                        exception.GetType().Name + ")."));
                }
            }

            return issues;
        }

        public static bool TryGetFirstBlocking(
            out DeucarianProjectIssue issue)
        {
            foreach (DeucarianProjectIssue candidate in Evaluate())
            {
                if (candidate.IsBlocking)
                {
                    issue = candidate;
                    return true;
                }
            }

            issue = null;
            return false;
        }

        private static void Remove(IDeucarianProjectCheckProvider provider)
        {
            lock (Gate)
            {
                Providers.Remove(provider);
            }

            Changed?.Invoke();
        }

        private sealed class Registration : IDisposable
        {
            private IDeucarianProjectCheckProvider provider;

            internal Registration(IDeucarianProjectCheckProvider value)
            {
                provider = value;
            }

            public void Dispose()
            {
                IDeucarianProjectCheckProvider current = provider;
                provider = null;
                if (current != null)
                {
                    Remove(current);
                }
            }
        }
    }

    [InitializeOnLoad]
    internal static class DeucarianPlayModeValidationGate
    {
        private static string lastMessage;

        static DeucarianPlayModeValidationGate()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            if (!DeucarianProjectValidationRegistry.TryGetFirstBlocking(
                    out DeucarianProjectIssue issue))
            {
                lastMessage = null;
                return;
            }

            EditorApplication.isPlaying = false;
            DeucarianControlCenterWindow.OpenProjectIssue(issue.Code);
            string message = issue.ToString() +
                " Open Tools/Deucarian/Control Center...";
            if (!string.Equals(lastMessage, message, StringComparison.Ordinal))
            {
                Console.Error.WriteLine(message);
                lastMessage = message;
            }
        }
    }

    internal sealed class DeucarianBuildValidationGate :
        IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (DeucarianProjectValidationRegistry.TryGetFirstBlocking(
                    out DeucarianProjectIssue issue))
            {
                throw new BuildFailedException(issue.ToString() +
                    " Open Tools/Deucarian/Control Center...");
            }
        }
    }

    public static class DeucarianProjectValidationCommandLine
    {
        /// <summary>Unity `-executeMethod` entry point. Throws on blockers.</summary>
        public static void Validate()
        {
            IReadOnlyList<DeucarianProjectIssue> issues =
                DeucarianProjectValidationRegistry.Evaluate();
            bool blocked = false;
            foreach (DeucarianProjectIssue issue in issues)
            {
                if (issue.IsBlocking)
                {
                    blocked = true;
                    Console.Error.WriteLine(issue.ToString());
                }
                else
                {
                    Console.WriteLine(issue.ToString());
                }
            }

            if (blocked)
            {
                throw new InvalidOperationException(
                    "Deucarian Control Center project validation failed.");
            }

            Console.WriteLine("Deucarian Control Center project validation passed.");
        }
    }
}
