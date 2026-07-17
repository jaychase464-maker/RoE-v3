namespace RulesOfEntry.Officers
{
    public enum OfficerOrderType
    {
        MoveTo = 0,
        HoldPosition = 1,
        Follow = 2,
        StackAtDoor = 3,
        OpenDoor = 4,
        RestrainSubject = 5
    }

    public enum OfficerOrderOrigin
    {
        PlayerCommand = 0,
        OfficerInitiative = 1
    }

    public enum OfficerOrderStatus
    {
        Pending = 0,
        Accepted = 1,
        Executing = 2,
        Completed = 3,
        Cancelled = 4,
        Failed = 5,
        Refused = 6
    }

    public enum OfficerOrderOutcomeReason
    {
        None = 0,
        OfficerUnavailable = 1,
        OfficerIncapacitated = 2,
        InvalidTarget = 3,
        NoNavigationSurface = 4,
        NoPath = 5,
        TargetUnreachable = 6,
        DoorAlreadyOpen = 7,
        DoorInteractionFailed = 8,
        SubjectNotCompliant = 9,
        SubjectNoLongerCompliant = 10,
        SubjectAlreadyRestrained = 11,
        RestraintTransitionFailed = 12,
        Superseded = 13,
        CancelledByPlayer = 14,
        TimedOut = 15,
        RoomNoLongerClear = 16,
        CoverOfficerUnavailable = 17
    }

    public enum OfficerSelection
    {
        OfficerOne = 0,
        OfficerTwo = 1,
        Team = 2
    }
}
