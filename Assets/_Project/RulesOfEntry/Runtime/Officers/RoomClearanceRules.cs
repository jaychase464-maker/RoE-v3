using RulesOfEntry.Actors;
using RulesOfEntry.AI;

namespace RulesOfEntry.Officers
{
    /// <summary>
    /// Pure policy used by room-clearance and officer-initiative systems.
    /// It deliberately makes no navigation or scene changes.
    /// </summary>
    public static class RoomClearanceRules
    {
        public static bool IsActiveThreat(HumanActorController subject)
        {
            if (subject == null || subject.Identity == null)
            {
                return false;
            }

            ActorConditionLevel conditionLevel = subject.Condition != null
                ? subject.Condition.Snapshot.Level
                : ActorConditionLevel.Stable;
            CustodyState custodyState = subject.Custody != null
                ? subject.Custody.State
                : CustodyState.Free;
            return IsActiveThreat(
                subject.Identity.Role,
                custodyState,
                conditionLevel);
        }

        public static bool IsActiveThreat(
            ActorRole role,
            CustodyState custodyState,
            ActorConditionLevel conditionLevel)
        {
            if (role != ActorRole.Suspect
                || IsRestrained(custodyState)
                || conditionLevel == ActorConditionLevel.Incapacitated
                || conditionLevel == ActorConditionLevel.Deceased)
            {
                return false;
            }

            return custodyState != CustodyState.Surrendering
                && custodyState != CustodyState.Kneeling;
        }

        public static bool IsEligibleForAutomaticCustody(HumanActorController subject)
        {
            if (subject == null || subject.Identity == null)
            {
                return false;
            }

            ActorConditionLevel conditionLevel = subject.Condition != null
                ? subject.Condition.Snapshot.Level
                : ActorConditionLevel.Stable;
            CustodyState custodyState = subject.Custody != null
                ? subject.Custody.State
                : CustodyState.Free;
            return IsEligibleForAutomaticCustody(
                subject.Identity.Role,
                custodyState,
                conditionLevel);
        }

        public static bool IsEligibleForAutomaticCustody(
            ActorRole role,
            CustodyState custodyState,
            ActorConditionLevel conditionLevel)
        {
            if (role != ActorRole.Suspect
                || IsRestrained(custodyState)
                || conditionLevel == ActorConditionLevel.Deceased)
            {
                return false;
            }

            return custodyState == CustodyState.Surrendering
                || custodyState == CustodyState.Kneeling
                || conditionLevel == ActorConditionLevel.Incapacitated;
        }

        public static bool IsRestrained(CustodyState custodyState)
        {
            return custodyState == CustodyState.Restrained
                || custodyState == CustodyState.Searched
                || custodyState == CustodyState.InCustody;
        }
    }
}
