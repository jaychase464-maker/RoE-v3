using System;
using System.Globalization;
using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.UI.TacticalHud
{
    [DisallowMultipleComponent]
    public sealed class MissionClock : MonoBehaviour
    {
        private const string TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss";

        [SerializeField] private string initialTimestamp = "2026-07-17T22:41:00";
        [SerializeField, Min(0f)] private float gameSecondsPerRealSecond = 1f;

        private DateTime currentTimestamp;
        private bool initialized;

        public DateTime CurrentTimestamp
        {
            get
            {
                Initialize();
                return currentTimestamp;
            }
        }

        public bool Configure(DateTime timestamp, float configuredTimeScale = 1f)
        {
            currentTimestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Unspecified);
            initialTimestamp = currentTimestamp.ToString(
                TimestampFormat,
                CultureInfo.InvariantCulture);
            gameSecondsPerRealSecond = Mathf.Max(0f, configuredTimeScale);
            initialized = true;
            return true;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!initialized || gameSecondsPerRealSecond <= 0f)
            {
                return;
            }

            currentTimestamp = currentTimestamp.AddSeconds(
                Time.deltaTime * gameSecondsPerRealSecond);
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = DateTime.TryParseExact(
                initialTimestamp,
                TimestampFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out currentTimestamp);
            if (initialized)
            {
                currentTimestamp = DateTime.SpecifyKind(
                    currentTimestamp,
                    DateTimeKind.Unspecified);
                return;
            }

            currentTimestamp = new DateTime(2026, 7, 17, 22, 41, 0);
            initialized = true;
            ProjectLog.Warning(
                "Mission Clock",
                $"Invalid timestamp '{initialTimestamp}'. Using the safe prototype timestamp.",
                this);
        }
    }
}
