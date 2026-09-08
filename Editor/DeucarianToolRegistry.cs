using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public static class DeucarianToolIds
    {
        public const string ControlCenter = "deucarian.control-center";
        public const string Bootstrap = "deucarian.bootstrap";
        public const string PackageInstaller = "deucarian.package-installer";
        public const string BuildManager = "deucarian.build-manager";
        public const string Diagnostics = "deucarian.diagnostics";
        public const string LoggingSettings = "deucarian.logging-settings";
        public const string GameContentAuthoring = "deucarian.game-content-authoring";
        public const string ApiConnections = "deucarian.api-connections";
        public const string Authentication = "deucarian.authentication";
        public const string SimultriaViewerDevelopment =
            "deucarian.simultria-viewer-development";
        public const string SimultriaContractUpdater =
            "deucarian.simultria-contract-updater";
        public const string ThemeManager = "deucarian.theme-manager";
        public const string CommandRouting = "deucarian.command-routing";
        public const string CommandRoutingWebGl =
            "deucarian.command-routing.webgl";
        public const string CommandRoutingUdp = "deucarian.command-routing.udp";
        public const string CameraNavigation = "deucarian.camera-navigation";
        public const string ViewerNavigation = "deucarian.viewer-navigation";
        public const string PointerCapture = "deucarian.pointer-capture";
    }

    public sealed class DeucarianToolDescriptor
    {
        private readonly Action open;

        public DeucarianToolDescriptor(
            string id,
            string displayName,
            string description,
            DeucarianControlCenterArea area,
            Action open,
            string owningPackage,
            string iconKey = null,
            IEnumerable<string> searchTerms = null,
            int order = 0)
        {
            Id = DeucarianControlCenterAction.Require(id, nameof(id));
            DisplayName = DeucarianControlCenterAction.Require(
                displayName,
                nameof(displayName));
            Description = DeucarianControlCenterAction.Clean(description);
            if (!Enum.IsDefined(typeof(DeucarianControlCenterArea), area))
            {
                throw new ArgumentOutOfRangeException(nameof(area));
            }

            Area = area;
            this.open = open ?? throw new ArgumentNullException(nameof(open));
            OwningPackage = DeucarianControlCenterAction.Require(
                owningPackage,
                nameof(owningPackage));
            IconKey = DeucarianControlCenterAction.Clean(iconKey);
            SearchTerms = DeucarianControlCenterAction.Copy(searchTerms);
            Order = order;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public DeucarianControlCenterArea Area { get; }
        public string OwningPackage { get; }
        public string IconKey { get; }
        public IReadOnlyList<string> SearchTerms { get; }
        public int Order { get; }
        public void Open()
        {
            open();
            DeucarianToolHistory.RecordOpened(Id);
        }
    }

    public static class DeucarianToolRegistry
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<string, DeucarianToolDescriptor> Tools =
            new Dictionary<string, DeucarianToolDescriptor>(
                StringComparer.Ordinal);

        public static event Action Changed;

        public static IDisposable Register(DeucarianToolDescriptor tool)
        {
            if (tool == null)
            {
                throw new ArgumentNullException(nameof(tool));
            }

            lock (Gate)
            {
                if (Tools.ContainsKey(tool.Id))
                {
                    throw new InvalidOperationException(
                        "Deucarian tool '" + tool.Id + "' is already registered.");
                }

                Tools.Add(tool.Id, tool);
            }

            Changed?.Invoke();
            return new Registration(tool);
        }

        public static IReadOnlyList<DeucarianToolDescriptor> GetTools()
        {
            var result = new List<DeucarianToolDescriptor>();
            lock (Gate)
            {
                result.AddRange(Tools.Values);
            }

            result.Sort(Compare);
            return result;
        }

        public static bool TryGet(
            string id,
            out DeucarianToolDescriptor tool)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                tool = null;
                return false;
            }

            lock (Gate)
            {
                return Tools.TryGetValue(id.Trim(), out tool);
            }
        }

        public static bool TryOpen(string id)
        {
            if (!TryGet(id, out DeucarianToolDescriptor tool))
            {
                return false;
            }

            tool.Open();
            return true;
        }

        private static int Compare(
            DeucarianToolDescriptor left,
            DeucarianToolDescriptor right)
        {
            int area = left.Area.CompareTo(right.Area);
            if (area != 0)
            {
                return area;
            }

            int order = left.Order.CompareTo(right.Order);
            if (order != 0)
            {
                return order;
            }

            int name = string.Compare(
                left.DisplayName,
                right.DisplayName,
                StringComparison.OrdinalIgnoreCase);
            return name != 0
                ? name
                : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        }

        private static void Remove(DeucarianToolDescriptor tool)
        {
            bool removed = false;
            lock (Gate)
            {
                if (Tools.TryGetValue(
                        tool.Id,
                        out DeucarianToolDescriptor current) &&
                    ReferenceEquals(current, tool))
                {
                    removed = Tools.Remove(tool.Id);
                }
            }

            if (removed)
            {
                Changed?.Invoke();
            }
        }

        private sealed class Registration : IDisposable
        {
            private DeucarianToolDescriptor tool;

            internal Registration(DeucarianToolDescriptor value)
            {
                tool = value;
            }

            public void Dispose()
            {
                DeucarianToolDescriptor current = tool;
                tool = null;
                if (current != null)
                {
                    Remove(current);
                }
            }
        }
    }
}
