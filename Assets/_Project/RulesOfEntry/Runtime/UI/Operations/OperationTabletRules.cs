using UnityEngine;

namespace RulesOfEntry.UI.Operations
{
    public static class OperationTabletRules
    {
        public static int WrapFeedIndex(int currentIndex, int delta, int feedCount)
        {
            if (feedCount <= 0)
            {
                return -1;
            }

            int normalized = (currentIndex + delta) % feedCount;
            return normalized < 0 ? normalized + feedCount : normalized;
        }

        public static string GetSignalLabel(bool signalAvailable, bool streaming)
        {
            if (!signalAvailable)
            {
                return "SIGNAL UNAVAILABLE";
            }

            return streaming ? "LIVE / ENCRYPTED" : "CONNECTING";
        }

        public static Color GetSignalColor(bool signalAvailable, bool streaming)
        {
            if (!signalAvailable)
            {
                return new Color(0.95f, 0.22f, 0.18f, 1f);
            }

            return streaming
                ? new Color(0.28f, 0.95f, 0.42f, 1f)
                : new Color(1f, 0.66f, 0.02f, 1f);
        }
    }
}
