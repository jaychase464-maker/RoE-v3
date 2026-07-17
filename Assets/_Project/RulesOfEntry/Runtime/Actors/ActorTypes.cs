using System;

namespace RulesOfEntry.Actors
{
    public enum ActorRole
    {
        Officer = 0,
        Suspect = 1,
        Civilian = 2
    }

    public enum ActorConditionLevel
    {
        Stable = 0,
        Wounded = 1,
        Incapacitated = 2,
        Deceased = 3
    }

    public enum ActorHitRegionType
    {
        Torso = 0,
        Head = 1,
        Neck = 2,
        Pelvis = 3,
        Limb = 4
    }

    public enum CustodyState
    {
        Free = 0,
        Surrendering = 1,
        Kneeling = 2,
        Restrained = 3,
        Searched = 4,
        InCustody = 5
    }

    public enum CustodyAction
    {
        BeginSurrender = 0,
        OrderToKneel = 1,
        SecureIncapacitated = 2,
        ApplyRestraints = 3,
        Search = 4,
        TransferCustody = 5,
        BreakSurrender = 6
    }

    public enum HumanBehaviorState
    {
        Idle = 0,
        Observing = 1,
        Frozen = 2,
        Hiding = 3,
        Fleeing = 4,
        Resisting = 5,
        Threatening = 6,
        Complying = 7,
        Surrendering = 8,
        Restrained = 9,
        Incapacitated = 10
    }

    public enum HumanDecisionReason
    {
        None = 0,
        CommandNotPerceived = 1,
        AlreadyRestrained = 2,
        PhysicallyUnable = 3,
        CommandUnderstood = 4,
        LowMorale = 5,
        OfficerAdvantage = 6,
        HighPanic = 7,
        FreezeResponse = 8,
        EscapeOpportunity = 9,
        HostileIntent = 10,
        RefusedCommand = 11,
        DeceptiveCompliance = 12,
        SurrenderAbandoned = 13,
        CustodyStateChanged = 14,
        InjuryResponse = 15
    }

    public enum VerbalCommandType
    {
        PoliceShowHands = 0,
        GetDown = 1,
        DropWeapon = 2,
        Stop = 3,
        DoNotMove = 4
    }

    [Serializable]
    public readonly struct ActorConditionSnapshot
    {
        public ActorConditionSnapshot(
            ActorConditionLevel level,
            float bloodVolumeLiters,
            float bleedingLitersPerMinute,
            float consciousness,
            float mobility)
        {
            Level = level;
            BloodVolumeLiters = bloodVolumeLiters;
            BleedingLitersPerMinute = bleedingLitersPerMinute;
            Consciousness = consciousness;
            Mobility = mobility;
        }

        public ActorConditionLevel Level { get; }
        public float BloodVolumeLiters { get; }
        public float BleedingLitersPerMinute { get; }
        public float Consciousness { get; }
        public float Mobility { get; }
        public bool CanAct => Level == ActorConditionLevel.Stable
            || Level == ActorConditionLevel.Wounded;
    }
}
