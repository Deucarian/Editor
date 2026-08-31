using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public static partial class DeucarianControlCenterSnapshotBuilder
    {


        private static void AddProviderFailure(
            ICollection<DeucarianControlCenterCard> cards,
            ISet<string> ids,
            string providerId,
            Exception exception)
        {
            string id = "deucarian.provider-failure." + providerId;
            if (!ids.Add(id))
            {
                return;
            }

            cards.Add(new DeucarianControlCenterCard(
                id,
                DeucarianControlCenterArea.Diagnostics,
                "Contribution unavailable",
                "An installed Control Center contribution failed safely (" +
                exception.GetType().Name + ").",
                providerId,
                DeucarianControlCenterStatus.Error,
                "Unavailable",
                int.MaxValue,
                searchTerms: new[] { providerId, "provider", "failure" }));
        }

        private static int CompareCards(
            DeucarianControlCenterCard left,
            DeucarianControlCenterCard right)
        {
            int area = left.Area.CompareTo(right.Area);
            if (area != 0) return area;
            int order = left.Order.CompareTo(right.Order);
            if (order != 0) return order;
            return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        }

        private static int CompareSections(
            DeucarianControlCenterSection left,
            DeucarianControlCenterSection right)
        {
            int area = left.Area.CompareTo(right.Area);
            if (area != 0) return area;
            int order = left.Order.CompareTo(right.Order);
            if (order != 0) return order;
            return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        }
    }
}
