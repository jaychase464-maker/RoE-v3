using RulesOfEntry.Campaign;
using RulesOfEntry.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RulesOfEntry.UI.FrontEnd
{
    [DisallowMultipleComponent]
    public sealed class CampaignFrontEndController : MonoBehaviour
    {
        [SerializeField] private FrontEndFlowController flowController;
        [SerializeField] private CanvasGroup mainMenuPanel;
        [SerializeField] private Button continueCampaignButton;
        [SerializeField] private Button newCampaignButton;
        [SerializeField] private CanvasGroup newCampaignPanel;
        [SerializeField] private InputField officerNameInput;
        [SerializeField] private InputField badgeIdentifierInput;
        [SerializeField] private Button createCampaignButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text menuCampaignStatusText;

        private bool panelOpen;
        private FrontEndState lastState = (FrontEndState)(-1);

        public bool IsNewCampaignPanelOpen => panelOpen;
        public bool HasCompleteConfiguration => flowController != null
            && mainMenuPanel != null
            && continueCampaignButton != null
            && newCampaignButton != null
            && newCampaignPanel != null
            && officerNameInput != null
            && badgeIdentifierInput != null
            && createCampaignButton != null
            && cancelButton != null
            && statusText != null
            && menuCampaignStatusText != null;

        public void Configure(
            FrontEndFlowController configuredFlow,
            CanvasGroup configuredMainMenu,
            Button configuredContinueButton,
            Button configuredNewButton,
            CanvasGroup configuredNewCampaignPanel,
            InputField configuredOfficerName,
            InputField configuredBadge,
            Button configuredCreateButton,
            Button configuredCancelButton,
            Text configuredStatus,
            Text configuredMenuStatus)
        {
            UnwireControls();
            flowController = configuredFlow;
            mainMenuPanel = configuredMainMenu;
            continueCampaignButton = configuredContinueButton;
            newCampaignButton = configuredNewButton;
            newCampaignPanel = configuredNewCampaignPanel;
            officerNameInput = configuredOfficerName;
            badgeIdentifierInput = configuredBadge;
            createCampaignButton = configuredCreateButton;
            cancelButton = configuredCancelButton;
            statusText = configuredStatus;
            menuCampaignStatusText = configuredMenuStatus;
            SetPanelVisible(false);
            WireControls();
            RefreshCampaignAvailability();
            SetMenuStatusVisible(flowController != null
                && flowController.State == FrontEndState.MainMenu);
        }

        private void Awake()
        {
            SetPanelVisible(false);
            WireControls();
            RefreshCampaignAvailability();
            SetMenuStatusVisible(false);
        }

        private void OnEnable()
        {
            WireControls();
        }

        private void OnDisable()
        {
            UnwireControls();
        }

        private void Update()
        {
            if (flowController == null)
            {
                return;
            }

            if (lastState != flowController.State)
            {
                lastState = flowController.State;
                if (lastState == FrontEndState.MainMenu)
                {
                    RefreshCampaignAvailability();
                }

                SetMenuStatusVisible(lastState == FrontEndState.MainMenu && !panelOpen);
            }

            if (!panelOpen)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            if (keyboard?.escapeKey.wasPressedThisFrame == true
                || gamepad?.buttonEast.wasPressedThisFrame == true)
            {
                CloseNewCampaign();
            }
            else if (keyboard?.enterKey.wasPressedThisFrame == true
                || keyboard?.numpadEnterKey.wasPressedThisFrame == true)
            {
                CreateCampaign();
            }
        }

        public void OpenNewCampaign()
        {
            if (!HasCompleteConfiguration
                || flowController.State != FrontEndState.MainMenu)
            {
                return;
            }

            panelOpen = true;
            mainMenuPanel.interactable = false;
            mainMenuPanel.blocksRaycasts = false;
            SetMenuStatusVisible(false);
            officerNameInput.SetTextWithoutNotify(string.Empty);
            badgeIdentifierInput.SetTextWithoutNotify(string.Empty);
            statusText.text = CampaignSaveService.HasActiveCampaignSave
                ? "CREATING A NEW RECORD WILL SWITCH THE ACTIVE CAMPAIGN. THE EXISTING FILE IS RETAINED."
                : "ENTER THE PERSONNEL IDENTITY USED BY THE BODY-CAMERA AND OPERATION RECORD.";
            SetPanelVisible(true);
            officerNameInput.Select();
            officerNameInput.ActivateInputField();
        }

        public void CloseNewCampaign()
        {
            if (!panelOpen)
            {
                return;
            }

            panelOpen = false;
            SetPanelVisible(false);
            if (flowController != null && flowController.State == FrontEndState.MainMenu)
            {
                mainMenuPanel.interactable = true;
                mainMenuPanel.blocksRaycasts = true;
                SetMenuStatusVisible(true);
                EventSystem.current?.SetSelectedGameObject(newCampaignButton.gameObject);
            }
        }

        public void ContinueCampaign()
        {
            if (!HasCompleteConfiguration || panelOpen)
            {
                return;
            }

            if (!CampaignSaveService.TryContinueActiveCampaign(
                    out CampaignSaveData campaign,
                    out string error))
            {
                RefreshCampaignAvailability();
                menuCampaignStatusText.text =
                    $"CAMPAIGN LOAD FAILED  //  {error.ToUpperInvariant()}";
                return;
            }

            menuCampaignStatusText.text =
                $"{campaign.officerDisplayName.ToUpperInvariant()}  //  BADGE {campaign.badgeIdentifier}";
            flowController.BeginOperation();
        }

        public void CreateCampaign()
        {
            if (!panelOpen)
            {
                return;
            }

            if (!CampaignSaveService.TryCreateCampaign(
                    officerNameInput.text,
                    badgeIdentifierInput.text,
                    out CampaignSaveData campaign,
                    out string error))
            {
                statusText.text = error.ToUpperInvariant();
                return;
            }

            statusText.text =
                $"PERSONNEL RECORD CREATED  //  {campaign.officerDisplayName.ToUpperInvariant()}";
            panelOpen = false;
            SetPanelVisible(false);
            mainMenuPanel.interactable = true;
            mainMenuPanel.blocksRaycasts = true;
            flowController.BeginOperation();
        }

        private void RefreshCampaignAvailability()
        {
            if (continueCampaignButton == null || newCampaignButton == null)
            {
                return;
            }

            bool available = CampaignSaveService.HasActiveCampaignSave;
            continueCampaignButton.interactable = available;
            newCampaignButton.interactable = true;
            if (menuCampaignStatusText != null)
            {
                if (CampaignSession.HasActiveCampaign)
                {
                    CampaignSaveData campaign = CampaignSession.ActiveCampaign;
                    menuCampaignStatusText.text =
                        $"ACTIVE  //  {campaign.officerDisplayName.ToUpperInvariant()}  //  "
                        + $"{campaign.CompletedOperationCount} COMPLETED OPERATION(S)";
                }
                else
                {
                    menuCampaignStatusText.text = available
                        ? "ACTIVE CAMPAIGN RECORD AVAILABLE"
                        : "NO CAMPAIGN RECORD  //  NEW CAMPAIGN REQUIRED";
                }
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (newCampaignPanel == null)
            {
                return;
            }

            newCampaignPanel.gameObject.SetActive(visible);
            newCampaignPanel.alpha = visible ? 1f : 0f;
            newCampaignPanel.interactable = visible;
            newCampaignPanel.blocksRaycasts = visible;
        }

        private void SetMenuStatusVisible(bool visible)
        {
            if (menuCampaignStatusText != null)
            {
                menuCampaignStatusText.gameObject.SetActive(visible);
            }
        }

        private void WireControls()
        {
            if (continueCampaignButton != null)
            {
                continueCampaignButton.onClick.RemoveListener(ContinueCampaign);
                continueCampaignButton.onClick.AddListener(ContinueCampaign);
            }

            if (newCampaignButton != null)
            {
                newCampaignButton.onClick.RemoveListener(OpenNewCampaign);
                newCampaignButton.onClick.AddListener(OpenNewCampaign);
            }

            if (createCampaignButton != null)
            {
                createCampaignButton.onClick.RemoveListener(CreateCampaign);
                createCampaignButton.onClick.AddListener(CreateCampaign);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(CloseNewCampaign);
                cancelButton.onClick.AddListener(CloseNewCampaign);
            }
        }

        private void UnwireControls()
        {
            continueCampaignButton?.onClick.RemoveListener(ContinueCampaign);
            newCampaignButton?.onClick.RemoveListener(OpenNewCampaign);
            createCampaignButton?.onClick.RemoveListener(CreateCampaign);
            cancelButton?.onClick.RemoveListener(CloseNewCampaign);
        }
    }
}
