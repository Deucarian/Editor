using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public static class DeucarianControlCenterRegistry
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<string, IDeucarianControlCenterCardProvider>
            CardProviders =
                new Dictionary<string, IDeucarianControlCenterCardProvider>(
                    StringComparer.Ordinal);
        private static readonly Dictionary<string, IDeucarianControlCenterSectionProvider>
            SectionProviders =
                new Dictionary<string, IDeucarianControlCenterSectionProvider>(
                    StringComparer.Ordinal);

        public static event Action Changed;

        public static IDisposable RegisterCardProvider(
            IDeucarianControlCenterCardProvider provider)
        {
            Validate(provider, provider?.Id, nameof(provider));
            lock (Gate)
            {
                RejectDuplicate(provider.Id);
                CardProviders.Add(provider.Id, provider);
            }

            Changed?.Invoke();
            return new Registration(provider.Id, provider, true);
        }

        public static IDisposable RegisterSectionProvider(
            IDeucarianControlCenterSectionProvider provider)
        {
            Validate(provider, provider?.Id, nameof(provider));
            lock (Gate)
            {
                RejectDuplicate(provider.Id);
                SectionProviders.Add(provider.Id, provider);
            }

            Changed?.Invoke();
            return new Registration(provider.Id, provider, false);
        }

        internal static IDeucarianControlCenterCardProvider[]
            GetCardProviders()
        {
            lock (Gate)
            {
                var result = new IDeucarianControlCenterCardProvider[
                    CardProviders.Count];
                CardProviders.Values.CopyTo(result, 0);
                Array.Sort(result, CompareCardProviders);
                return result;
            }
        }

        internal static IDeucarianControlCenterSectionProvider[]
            GetSectionProviders()
        {
            lock (Gate)
            {
                var result = new IDeucarianControlCenterSectionProvider[
                    SectionProviders.Count];
                SectionProviders.Values.CopyTo(result, 0);
                Array.Sort(result, CompareSectionProviders);
                return result;
            }
        }

        private static void Validate(object provider, string id, string parameterName)
        {
            if (provider == null || string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "A Control Center provider with a stable ID is required.",
                    parameterName);
            }
        }

        private static void RejectDuplicate(string id)
        {
            if (CardProviders.ContainsKey(id) || SectionProviders.ContainsKey(id))
            {
                throw new InvalidOperationException(
                    "Control Center provider '" + id + "' is already registered.");
            }
        }

        private static int CompareCardProviders(
            IDeucarianControlCenterCardProvider left,
            IDeucarianControlCenterCardProvider right)
        {
            return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        }

        private static int CompareSectionProviders(
            IDeucarianControlCenterSectionProvider left,
            IDeucarianControlCenterSectionProvider right)
        {
            return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        }

        private static void Remove(string id, object provider, bool cardProvider)
        {
            bool removed = false;
            lock (Gate)
            {
                if (cardProvider &&
                    CardProviders.TryGetValue(
                        id,
                        out IDeucarianControlCenterCardProvider card) &&
                    ReferenceEquals(card, provider))
                {
                    removed = CardProviders.Remove(id);
                }
                else if (!cardProvider &&
                    SectionProviders.TryGetValue(
                        id,
                        out IDeucarianControlCenterSectionProvider section) &&
                    ReferenceEquals(section, provider))
                {
                    removed = SectionProviders.Remove(id);
                }
            }

            if (removed)
            {
                Changed?.Invoke();
            }
        }

        private sealed class Registration : IDisposable
        {
            private readonly string id;
            private readonly bool cardProvider;
            private object provider;

            internal Registration(string value, object instance, bool isCardProvider)
            {
                id = value;
                provider = instance;
                cardProvider = isCardProvider;
            }

            public void Dispose()
            {
                object current = provider;
                provider = null;
                if (current != null)
                {
                    Remove(id, current, cardProvider);
                }
            }
        }
    }
}
