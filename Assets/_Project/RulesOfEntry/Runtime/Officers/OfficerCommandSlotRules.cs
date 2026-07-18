namespace RulesOfEntry.Officers
{
    public static class OfficerCommandSlotRules
    {
        public static bool TryGetOrderType(int slot, out OfficerOrderType orderType)
        {
            orderType = slot switch
            {
                1 => OfficerOrderType.MoveTo,
                2 => OfficerOrderType.HoldPosition,
                3 => OfficerOrderType.StackAtDoor,
                4 => OfficerOrderType.OpenDoor,
                5 => OfficerOrderType.Follow,
                6 => OfficerOrderType.RestrainSubject,
                _ => default
            };
            return slot >= 1 && slot <= 6;
        }
    }
}
