namespace RulesOfEntry.Officers
{
    public enum OfficerCommandTargetType
    {
        None = 0,
        Position = 1,
        Door = 2,
        Subject = 3
    }

    public readonly struct OfficerCommandContext
    {
        public OfficerCommandContext(
            OfficerCommandTargetType targetType,
            string displayName,
            float distanceMeters)
        {
            TargetType = targetType;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? targetType.ToString()
                : displayName.Trim();
            DistanceMeters = distanceMeters < 0f ? 0f : distanceMeters;
        }

        public OfficerCommandTargetType TargetType { get; }
        public string DisplayName { get; }
        public float DistanceMeters { get; }
    }
}
