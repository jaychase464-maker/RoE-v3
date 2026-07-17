using System.Collections;
using RulesOfEntry.Core;
using RulesOfEntry.Missions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.UI.FrontEnd
{
    public enum FrontEndState
    {
        Splash = 0,
        Warning = 1,
        Title = 2,
        MainMenu = 3,
        Settings = 4,
        Credits = 5,
        Loading = 6
    }

    [DisallowMultipleComponent]
    public sealed class FrontEndFlowController : MonoBehaviour
    {
        private const string MasterVolumePreference = "roe.audio.master_volume";
        private const string FullscreenPreference = "roe.video.fullscreen";
        private const string QualityPreference = "roe.video.quality";

        [Header("Panels")]
        [SerializeField] private CanvasGroup splashPanel;
        [SerializeField] private CanvasGroup warningPanel;
        [SerializeField] private CanvasGroup titlePanel;
        [SerializeField] private CanvasGroup mainMenuPanel;
        [SerializeField] private CanvasGroup settingsPanel;
        [SerializeField] private CanvasGroup creditsPanel;
        [SerializeField] private CanvasGroup loadingPanel;

        [Header("Navigation")]
        [SerializeField] private Button titleContinueButton;
        [SerializeField] private Button deployButton;
        [SerializeField] private Button trainingButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button creditsBackButton;
        [SerializeField] private Button qualityButton;

        [Header("Settings and status")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Text qualityValueText;
        [SerializeField] private Text loadingContextText;
        [SerializeField] private Text loadingDestinationText;
        [SerializeField] private Text loadingDetailText;
        [SerializeField] private Text loadingStatusText;
        [SerializeField] private Text loadingPercentageText;
        [SerializeField] private Image loadingProgressFill;
        [SerializeField] private Text buildLabel;

        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float splashFadeInSeconds = 0.65f;
        [SerializeField, Min(0.1f)] private float splashHoldSeconds = 1.35f;
        [SerializeField, Min(0.1f)] private float splashFadeOutSeconds = 0.65f;
        [SerializeField, Min(0.05f)] private float panelFadeSeconds = 0.22f;
        [SerializeField, Min(0f)] private float minimumLoadingDisplaySeconds = 0.45f;
        [SerializeField] private string operationScenePath = ProjectInfo.HeadquartersScenePath;
        [SerializeField] private MissionDefinition operationDefinition;

        private CanvasGroup activePanel;
        private Coroutine transitionRoutine;
        private bool loadingOperation;
        private string activeLoadingContext = "MISSION";
        private string activeDestinationPath = ProjectInfo.HeadquartersScenePath;
        private string activeDestinationName = "Calder City Police Department";
        private string activeDestinationDetail = "CALDER CITY  •  OPERATIONS DIVISION";

        public FrontEndState State { get; private set; } = FrontEndState.Splash;
        public string OperationScenePath => operationScenePath;
        public string OperationDisplayName => "Calder City Police Department";
        public bool HasCompleteConfiguration => splashPanel != null
            && warningPanel != null
            && titlePanel != null
            && mainMenuPanel != null
            && settingsPanel != null
            && creditsPanel != null
            && loadingPanel != null
            && titleContinueButton != null
            && deployButton != null
            && trainingButton != null
            && settingsButton != null
            && creditsButton != null
            && quitButton != null
            && settingsBackButton != null
            && creditsBackButton != null
            && qualityButton != null
            && masterVolumeSlider != null
            && fullscreenToggle != null
            && qualityValueText != null
            && loadingContextText != null
            && loadingDestinationText != null
            && loadingDetailText != null
            && loadingStatusText != null
            && loadingPercentageText != null
            && loadingProgressFill != null
            && operationDefinition != null
            && !string.IsNullOrWhiteSpace(operationScenePath);

        public void Configure(
            CanvasGroup configuredSplashPanel,
            CanvasGroup configuredWarningPanel,
            CanvasGroup configuredTitlePanel,
            CanvasGroup configuredMainMenuPanel,
            CanvasGroup configuredSettingsPanel,
            CanvasGroup configuredCreditsPanel,
            CanvasGroup configuredLoadingPanel,
            Button configuredTitleContinueButton,
            Button configuredDeployButton,
            Button configuredTrainingButton,
            Button configuredSettingsButton,
            Button configuredCreditsButton,
            Button configuredQuitButton,
            Button configuredSettingsBackButton,
            Button configuredCreditsBackButton,
            Button configuredQualityButton,
            Slider configuredMasterVolumeSlider,
            Toggle configuredFullscreenToggle,
            Text configuredQualityValueText,
            Text configuredLoadingContextText,
            Text configuredLoadingDestinationText,
            Text configuredLoadingDetailText,
            Text configuredLoadingStatusText,
            Text configuredLoadingPercentageText,
            Image configuredLoadingProgressFill,
            Text configuredBuildLabel,
            string configuredOperationScenePath,
            MissionDefinition configuredOperationDefinition)
        {
            splashPanel = configuredSplashPanel;
            warningPanel = configuredWarningPanel;
            titlePanel = configuredTitlePanel;
            mainMenuPanel = configuredMainMenuPanel;
            settingsPanel = configuredSettingsPanel;
            creditsPanel = configuredCreditsPanel;
            loadingPanel = configuredLoadingPanel;
            titleContinueButton = configuredTitleContinueButton;
            deployButton = configuredDeployButton;
            trainingButton = configuredTrainingButton;
            settingsButton = configuredSettingsButton;
            creditsButton = configuredCreditsButton;
            quitButton = configuredQuitButton;
            settingsBackButton = configuredSettingsBackButton;
            creditsBackButton = configuredCreditsBackButton;
            qualityButton = configuredQualityButton;
            masterVolumeSlider = configuredMasterVolumeSlider;
            fullscreenToggle = configuredFullscreenToggle;
            qualityValueText = configuredQualityValueText;
            loadingContextText = configuredLoadingContextText;
            loadingDestinationText = configuredLoadingDestinationText;
            loadingDetailText = configuredLoadingDetailText;
            loadingStatusText = configuredLoadingStatusText;
            loadingPercentageText = configuredLoadingPercentageText;
            loadingProgressFill = configuredLoadingProgressFill;
            buildLabel = configuredBuildLabel;
            operationScenePath = string.IsNullOrWhiteSpace(configuredOperationScenePath)
                ? ProjectInfo.HeadquartersScenePath
                : configuredOperationScenePath;
            operationDefinition = configuredOperationDefinition;
        }

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            WireControls();
            InitializeSettings();
            SetAllPanelsHidden();
            if (buildLabel != null)
            {
                buildLabel.text = $"PROTOTYPE {Application.version}  •  {ProjectInfo.CurrentMilestone}";
            }
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Front End",
                    "Front-end references are incomplete. Run the UI Presentation setup tool.",
                    this);
                return;
            }

            transitionRoutine = StartCoroutine(RunSplashSequence());
        }

        private void Update()
        {
            if (loadingOperation)
            {
                return;
            }

            if (State == FrontEndState.Warning && WasWarningContinuePressed())
            {
                ContinueFromWarning();
                return;
            }

            if (State == FrontEndState.Title && WasContinuePressed())
            {
                ContinueFromTitle();
                return;
            }

            bool cancelPressed = Keyboard.current?.escapeKey.wasPressedThisFrame == true
                || Gamepad.current?.buttonEast.wasPressedThisFrame == true;
            if (cancelPressed
                && (State == FrontEndState.Settings || State == FrontEndState.Credits))
            {
                ReturnToMainMenu();
            }
        }

        private void OnDestroy()
        {
            UnwireControls();
        }

        public void ContinueFromTitle()
        {
            if (State != FrontEndState.Title || loadingOperation)
            {
                return;
            }

            TransitionTo(FrontEndState.MainMenu, mainMenuPanel, deployButton);
        }

        public void ContinueFromWarning()
        {
            if (State != FrontEndState.Warning || loadingOperation)
            {
                return;
            }

            TransitionTo(FrontEndState.Title, titlePanel, titleContinueButton);
        }

        public void BeginOperation()
        {
            BeginDestinationLoad(
                "HEADQUARTERS",
                operationScenePath,
                "Calder City Police Department",
                "CALDER CITY  •  OPERATIONS DIVISION");
        }

        public void BeginTraining()
        {
            BeginDestinationLoad(
                "TRAINING",
                ProjectInfo.PrototypeScenePath,
                operationDefinition != null
                    ? operationDefinition.DisplayName
                    : "Training Facility",
                "CALDER CITY  •  TACTICAL TRAINING COMPLEX");
        }

        private void BeginDestinationLoad(
            string loadingContext,
            string destinationPath,
            string destinationName,
            string destinationDetail)
        {
            if (loadingOperation)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(destinationPath))
            {
                ProjectLog.Error(
                    "Front End",
                    $"Destination scene is not available in Build Settings: {destinationPath}",
                    this);
                if (loadingStatusText != null)
                {
                    loadingStatusText.text = "DESTINATION UNAVAILABLE";
                }

                if (loadingPercentageText != null)
                {
                    loadingPercentageText.text = "ERROR";
                }

                return;
            }

            activeLoadingContext = string.IsNullOrWhiteSpace(loadingContext)
                ? "DESTINATION"
                : loadingContext.Trim().ToUpperInvariant();
            activeDestinationPath = destinationPath;
            activeDestinationName = string.IsNullOrWhiteSpace(destinationName)
                ? GetSceneDisplayName(destinationPath)
                : destinationName.Trim();
            activeDestinationDetail = destinationDetail?.Trim() ?? string.Empty;
            loadingOperation = true;
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(LoadOperation());
        }

        public void OpenSettings()
        {
            if (!loadingOperation)
            {
                TransitionTo(FrontEndState.Settings, settingsPanel, settingsBackButton);
            }
        }

        public void OpenCredits()
        {
            if (!loadingOperation)
            {
                TransitionTo(FrontEndState.Credits, creditsPanel, creditsBackButton);
            }
        }

        public void ReturnToMainMenu()
        {
            if (!loadingOperation)
            {
                TransitionTo(FrontEndState.MainMenu, mainMenuPanel, deployButton);
            }
        }

        public void CycleQuality()
        {
            string[] names = QualitySettings.names;
            int next = FrontEndRules.GetNextQualityIndex(
                QualitySettings.GetQualityLevel(),
                names.Length);
            QualitySettings.SetQualityLevel(next, true);
            PlayerPrefs.SetInt(QualityPreference, next);
            PlayerPrefs.Save();
            RefreshQualityText();
        }

        public void QuitGame()
        {
            ProjectLog.Info("Front End", "Exit requested from the main menu.", this);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator RunSplashSequence()
        {
            State = FrontEndState.Splash;
            activePanel = splashPanel;
            SetPanelVisible(splashPanel, true);
            SetPanelInteraction(splashPanel, false);
            yield return Fade(splashPanel, 0f, 1f, splashFadeInSeconds);
            yield return new WaitForSecondsRealtime(splashHoldSeconds);
            yield return Fade(splashPanel, 1f, 0f, splashFadeOutSeconds);
            SetPanelVisible(splashPanel, false);
            State = FrontEndState.Warning;
            activePanel = warningPanel;
            SetPanelVisible(warningPanel, true);
            SetPanelInteraction(warningPanel, false);
            yield return Fade(warningPanel, 0f, 1f, panelFadeSeconds);
            transitionRoutine = null;
        }

        private IEnumerator LoadOperation()
        {
            State = FrontEndState.Loading;
            if (activePanel != null)
            {
                SetPanelInteraction(activePanel, false);
                yield return Fade(activePanel, activePanel.alpha, 0f, panelFadeSeconds);
                SetPanelVisible(activePanel, false);
            }

            activePanel = loadingPanel;
            SetPanelVisible(loadingPanel, true);
            SetPanelInteraction(loadingPanel, false);
            loadingContextText.text = activeLoadingContext;
            loadingDestinationText.text = activeDestinationName.ToUpperInvariant();
            loadingDetailText.text = activeDestinationDetail;
            loadingProgressFill.fillAmount = 0f;
            loadingStatusText.text = "ESTABLISHING COMMAND LINK";
            loadingPercentageText.text = "LOADING  0%";
            yield return Fade(loadingPanel, 0f, 1f, panelFadeSeconds);

            float shownAt = Time.unscaledTime;
            AsyncOperation operation = SceneManager.LoadSceneAsync(
                activeDestinationPath,
                LoadSceneMode.Single);
            if (operation == null)
            {
                loadingOperation = false;
                loadingStatusText.text = "LOAD FAILED — CHECK CONSOLE";
                loadingPercentageText.text = "ERROR";
                ProjectLog.Error(
                    "Front End",
                    $"Unity did not create a load operation for {activeDestinationPath}.",
                    this);
                yield break;
            }

            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f)
            {
                float progress = FrontEndRules.NormalizeLoadingProgress(operation.progress);
                loadingProgressFill.fillAmount = progress;
                loadingStatusText.text = GetLoadingPhase(progress);
                loadingPercentageText.text = $"LOADING  {progress * 100f:0}%";
                yield return null;
            }

            loadingProgressFill.fillAmount = 1f;
            loadingStatusText.text = "DESTINATION READY";
            loadingPercentageText.text = "READY";
            float remaining = minimumLoadingDisplaySeconds
                - (Time.unscaledTime - shownAt);
            if (remaining > 0f)
            {
                yield return new WaitForSecondsRealtime(remaining);
            }

            operation.allowSceneActivation = true;
        }

        private void TransitionTo(
            FrontEndState nextState,
            CanvasGroup nextPanel,
            Button firstSelection)
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(
                TransitionPanels(nextState, nextPanel, firstSelection));
        }

        private IEnumerator TransitionPanels(
            FrontEndState nextState,
            CanvasGroup nextPanel,
            Button firstSelection)
        {
            CanvasGroup previous = activePanel;
            if (previous != null)
            {
                SetPanelInteraction(previous, false);
                yield return Fade(previous, previous.alpha, 0f, panelFadeSeconds);
                SetPanelVisible(previous, false);
            }

            State = nextState;
            activePanel = nextPanel;
            SetPanelVisible(nextPanel, true);
            SetPanelInteraction(nextPanel, true);
            yield return Fade(nextPanel, 0f, 1f, panelFadeSeconds);
            Select(firstSelection);
            transitionRoutine = null;
        }

        private static IEnumerator Fade(
            CanvasGroup group,
            float from,
            float to,
            float duration)
        {
            if (group == null)
            {
                yield break;
            }

            float elapsed = 0f;
            group.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float amount = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                amount = amount * amount * (3f - 2f * amount);
                group.alpha = Mathf.Lerp(from, to, amount);
                yield return null;
            }

            group.alpha = to;
        }

        private void WireControls()
        {
            titleContinueButton?.onClick.AddListener(ContinueFromTitle);
            deployButton?.onClick.AddListener(BeginOperation);
            trainingButton?.onClick.AddListener(BeginTraining);
            settingsButton?.onClick.AddListener(OpenSettings);
            creditsButton?.onClick.AddListener(OpenCredits);
            quitButton?.onClick.AddListener(QuitGame);
            settingsBackButton?.onClick.AddListener(ReturnToMainMenu);
            creditsBackButton?.onClick.AddListener(ReturnToMainMenu);
            qualityButton?.onClick.AddListener(CycleQuality);
            masterVolumeSlider?.onValueChanged.AddListener(ApplyMasterVolume);
            fullscreenToggle?.onValueChanged.AddListener(ApplyFullscreen);
        }

        private void UnwireControls()
        {
            titleContinueButton?.onClick.RemoveListener(ContinueFromTitle);
            deployButton?.onClick.RemoveListener(BeginOperation);
            trainingButton?.onClick.RemoveListener(BeginTraining);
            settingsButton?.onClick.RemoveListener(OpenSettings);
            creditsButton?.onClick.RemoveListener(OpenCredits);
            quitButton?.onClick.RemoveListener(QuitGame);
            settingsBackButton?.onClick.RemoveListener(ReturnToMainMenu);
            creditsBackButton?.onClick.RemoveListener(ReturnToMainMenu);
            qualityButton?.onClick.RemoveListener(CycleQuality);
            masterVolumeSlider?.onValueChanged.RemoveListener(ApplyMasterVolume);
            fullscreenToggle?.onValueChanged.RemoveListener(ApplyFullscreen);
        }

        private void InitializeSettings()
        {
            float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(
                MasterVolumePreference,
                AudioListener.volume));
            AudioListener.volume = volume;
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(volume);
            }

            bool fullscreen = PlayerPrefs.GetInt(
                FullscreenPreference,
                Screen.fullScreen ? 1 : 0) == 1;
            Screen.fullScreen = fullscreen;
            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
            }

            int quality = Mathf.Clamp(
                PlayerPrefs.GetInt(QualityPreference, QualitySettings.GetQualityLevel()),
                0,
                Mathf.Max(0, QualitySettings.names.Length - 1));
            QualitySettings.SetQualityLevel(quality, true);
            RefreshQualityText();
        }

        private void ApplyMasterVolume(float value)
        {
            float volume = Mathf.Clamp01(value);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(MasterVolumePreference, volume);
            PlayerPrefs.Save();
        }

        private void ApplyFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
            PlayerPrefs.SetInt(FullscreenPreference, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void RefreshQualityText()
        {
            if (qualityValueText == null)
            {
                return;
            }

            string[] names = QualitySettings.names;
            int index = QualitySettings.GetQualityLevel();
            qualityValueText.text = index >= 0 && index < names.Length
                ? names[index].ToUpperInvariant()
                : "UNAVAILABLE";
        }

        private static string GetLoadingPhase(float progress)
        {
            if (progress < 0.34f)
            {
                return "LOADING MISSION DATA";
            }

            if (progress < 0.78f)
            {
                return "PREPARING RESPONSE ENVIRONMENT";
            }

            return "FINALIZING DEPLOYMENT";
        }

        private static string GetSceneDisplayName(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return "UNKNOWN DESTINATION";
            }

            int nameStart = scenePath.LastIndexOf('/') + 1;
            int extensionStart = scenePath.LastIndexOf('.');
            if (extensionStart <= nameStart)
            {
                extensionStart = scenePath.Length;
            }

            return scenePath
                .Substring(nameStart, extensionStart - nameStart)
                .Replace('_', ' ')
                .ToUpperInvariant();
        }

        private void SetAllPanelsHidden()
        {
            SetPanelVisible(splashPanel, false);
            SetPanelVisible(warningPanel, false);
            SetPanelVisible(titlePanel, false);
            SetPanelVisible(mainMenuPanel, false);
            SetPanelVisible(settingsPanel, false);
            SetPanelVisible(creditsPanel, false);
            SetPanelVisible(loadingPanel, false);
        }

        private static void SetPanelVisible(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.gameObject.SetActive(visible);
            group.alpha = visible ? 1f : 0f;
            SetPanelInteraction(group, visible);
        }

        private static void SetPanelInteraction(CanvasGroup group, bool interactive)
        {
            if (group == null)
            {
                return;
            }

            group.interactable = interactive;
            group.blocksRaycasts = interactive;
        }

        private static void Select(Button button)
        {
            if (button == null || EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private static bool WasContinuePressed()
        {
            return Keyboard.current?.anyKey.wasPressedThisFrame == true
                || Mouse.current?.leftButton.wasPressedThisFrame == true
                || Gamepad.current?.startButton.wasPressedThisFrame == true
                || Gamepad.current?.buttonSouth.wasPressedThisFrame == true;
        }

        private static bool WasWarningContinuePressed()
        {
            return FrontEndRules.IsWarningContinueRequested(
                Keyboard.current?.enterKey.wasPressedThisFrame == true,
                Keyboard.current?.numpadEnterKey.wasPressedThisFrame == true,
                Gamepad.current?.buttonSouth.wasPressedThisFrame == true);
        }
    }
}
