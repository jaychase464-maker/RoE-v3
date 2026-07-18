using RulesOfEntry.Campaign;
using UnityEngine;

namespace RulesOfEntry.UI.TacticalHud
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BodyCameraIdentity))]
    public sealed class CampaignBodyCameraIdentityBinder : MonoBehaviour
    {
        [SerializeField] private BodyCameraIdentity bodyCameraIdentity;

        public BodyCameraIdentity BodyCameraIdentity => bodyCameraIdentity;
        public bool HasCompleteConfiguration => bodyCameraIdentity != null;

        public void Configure(BodyCameraIdentity configuredIdentity)
        {
            bodyCameraIdentity = configuredIdentity;
            if (Application.isPlaying)
            {
                ApplyCampaignIdentity();
            }
        }

        private void Awake()
        {
            bodyCameraIdentity ??= GetComponent<BodyCameraIdentity>();
        }

        private void OnEnable()
        {
            CampaignSession.ActiveCampaignChanged -= ApplyCampaignIdentity;
            CampaignSession.ActiveCampaignChanged += ApplyCampaignIdentity;
            ApplyCampaignIdentity();
        }

        private void OnDisable()
        {
            CampaignSession.ActiveCampaignChanged -= ApplyCampaignIdentity;
        }

        private void ApplyCampaignIdentity()
        {
            if (bodyCameraIdentity == null || !CampaignSession.HasActiveCampaign)
            {
                return;
            }

            CampaignSaveData campaign = CampaignSession.ActiveCampaign;
            bodyCameraIdentity.Configure(
                campaign.officerDisplayName,
                campaign.badgeIdentifier,
                campaign.departmentName);
        }
    }
}
