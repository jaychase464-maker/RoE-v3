using System;
using UnityEngine;

namespace RulesOfEntry.Campaign
{
    public static class CampaignSession
    {
        public static event Action ActiveCampaignChanged;

        public static CampaignSaveData ActiveCampaign { get; private set; }
        public static bool HasActiveCampaign => ActiveCampaign != null;

        internal static void SetActive(CampaignSaveData campaign)
        {
            ActiveCampaign = campaign;
            ActiveCampaignChanged?.Invoke();
        }

        public static void Clear()
        {
            ActiveCampaign = null;
            ActiveCampaignChanged?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForApplicationSession()
        {
            ActiveCampaign = null;
            ActiveCampaignChanged = null;
        }
    }
}
