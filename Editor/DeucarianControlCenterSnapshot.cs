using System;
using System.Collections.Generic;

namespace Deucarian.Editor
{
    public sealed class DeucarianControlCenterSnapshot
    {
        internal DeucarianControlCenterSnapshot(
            DateTime capturedAtUtc,
            IReadOnlyList<DeucarianControlCenterCard> cards,
            IReadOnlyList<DeucarianControlCenterSection> sections,
            IReadOnlyList<DeucarianToolDescriptor> tools)
        {
            CapturedAtUtc = capturedAtUtc;
            Cards = cards;
            Sections = sections;
            Tools = tools;
            Areas = BuildAreas(cards, sections, tools);
        }

        public DateTime CapturedAtUtc { get; }
        public IReadOnlyList<DeucarianControlCenterCard> Cards { get; }
        public IReadOnlyList<DeucarianControlCenterSection> Sections { get; }
        public IReadOnlyList<DeucarianToolDescriptor> Tools { get; }
        public IReadOnlyList<DeucarianControlCenterArea> Areas { get; }

        public IEnumerable<DeucarianControlCenterCard> GetCards(
            DeucarianControlCenterArea area)
        {
            foreach (DeucarianControlCenterCard card in Cards)
            {
                if (card.Area == area)
                {
                    yield return card;
                }
            }

            foreach (DeucarianControlCenterSection section in Sections)
            {
                if (section.Area != area)
                {
                    continue;
                }

                foreach (DeucarianControlCenterCard card in section.Cards)
                {
                    yield return card;
                }
            }
        }

        private static IReadOnlyList<DeucarianControlCenterArea> BuildAreas(
            IReadOnlyList<DeucarianControlCenterCard> cards,
            IReadOnlyList<DeucarianControlCenterSection> sections,
            IReadOnlyList<DeucarianToolDescriptor> tools)
        {
            var unique = new HashSet<DeucarianControlCenterArea>
            {
                DeucarianControlCenterArea.Overview,
                DeucarianControlCenterArea.Project
            };
            foreach (DeucarianControlCenterCard card in cards)
            {
                unique.Add(card.Area);
            }

            foreach (DeucarianControlCenterSection section in sections)
            {
                unique.Add(section.Area);
            }

            foreach (DeucarianToolDescriptor tool in tools)
            {
                unique.Add(tool.Area);
            }

            var result = new List<DeucarianControlCenterArea>(unique);
            result.Sort();
            return result;
        }
    }

    public static partial class DeucarianControlCenterSnapshotBuilder
    {
    }
}
