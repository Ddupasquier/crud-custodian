using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrudCustodian.Core;
using CrudCustodian.Authentication;

namespace CrudCustodian.UI
{
    /// <summary>
    /// Controls the Main Menu screen: Google Sign-In button, Guest button,
    /// Play button, and the settings entry point.
    ///
    /// The Google Sign-In button label adapts to the platform:
    ///   • Desktop → "Sign in with Google (Browser)"
    ///   • Mobile  → "Sign in with Google"
    ///
    /// The full-screen toggle button is only visible on desktop platforms.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Buttons")]
        [Tooltip("Button that starts the Google Sign-In flow.")]
        [SerializeField] private Button googleSignInButton;

        [Tooltip("Label on the Google Sign-In button (updated at runtime to show platform context).")]
        [SerializeField] private TextMeshProUGUI googleSignInButtonLabel;

        [Tooltip("Button that skips sign-in and enters the game as a guest.")]
        [SerializeField] private Button continueAsGuestButton;

        [Tooltip("Button that loads the Safari Zone gameplay scene. Only visible when signed in or as a guest.")]
        [SerializeField] private Button playButton;

        [Tooltip("Button that opens the Character Customization screen.")]
        [SerializeField] private Button characterCustomizationButton;

        [Tooltip("Button that toggles full-screen mode. Shown ONLY on desktop platforms.")]
        [SerializeField] private Button fullScreenToggleButton;

        [Header("Status Display")]
        [Tooltip("Displays the currently signed-in player name, or 'Playing as Guest'.")]
        [SerializeField] private TextMeshProUGUI signedInStatusLabel;

        [Tooltip("Panel shown while a sign-in attempt is in progress.")]
        [SerializeField] private GameObject signInLoadingSpinnerPanel;

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Start()
        {
            ConfigureButtonsForCurrentPlatform();
            RefreshSignInStatusLabel();
            SubscribeToButtonClickEvents();
            SubscribeToAuthenticationEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromButtonClickEvents();
            UnsubscribeFromAuthenticationEvents();
        }

        // ── Setup helpers ──────────────────────────────────────────────────

        private void ConfigureButtonsForCurrentPlatform()
        {
            bool isDesktop = PlatformManager.Instance.IsAnyDesktopPlatform;

            // Show the full-screen toggle only on desktop.
            if (fullScreenToggleButton != null)
                fullScreenToggleButton.gameObject.SetActive(isDesktop);

            // Update sign-in button label to set player expectations.
            if (googleSignInButtonLabel != null)
            {
                googleSignInButtonLabel.text = isDesktop
                    ? "Sign in with Google (Browser)"
                    : "Sign in with Google";
            }
        }

        private void RefreshSignInStatusLabel()
        {
            if (signedInStatusLabel == null) return;

            AuthenticationManager authManager = AuthenticationManager.Instance;

            if (authManager != null && authManager.IsPlayerCurrentlySignedIn)
            {
                signedInStatusLabel.text = $"Signed in as {authManager.CurrentPlayerGoogleDisplayName}";
            }
            else
            {
                signedInStatusLabel.text = "Not signed in";
            }
        }

        private void SubscribeToButtonClickEvents()
        {
            googleSignInButton?.onClick.AddListener(HandleGoogleSignInButtonClicked);
            continueAsGuestButton?.onClick.AddListener(HandleContinueAsGuestButtonClicked);
            playButton?.onClick.AddListener(HandlePlayButtonClicked);
            characterCustomizationButton?.onClick.AddListener(HandleCharacterCustomizationButtonClicked);
            fullScreenToggleButton?.onClick.AddListener(HandleFullScreenToggleButtonClicked);
        }

        private void UnsubscribeFromButtonClickEvents()
        {
            googleSignInButton?.onClick.RemoveListener(HandleGoogleSignInButtonClicked);
            continueAsGuestButton?.onClick.RemoveListener(HandleContinueAsGuestButtonClicked);
            playButton?.onClick.RemoveListener(HandlePlayButtonClicked);
            characterCustomizationButton?.onClick.RemoveListener(HandleCharacterCustomizationButtonClicked);
            fullScreenToggleButton?.onClick.RemoveListener(HandleFullScreenToggleButtonClicked);
        }

        private void SubscribeToAuthenticationEvents()
        {
            if (AuthenticationManager.Instance == null) return;
            AuthenticationManager.Instance.OnPlayerSignedInWithGoogle += HandlePlayerSignedIn;
            AuthenticationManager.Instance.OnPlayerSignInFailed       += HandlePlayerSignInFailed;
        }

        private void UnsubscribeFromAuthenticationEvents()
        {
            if (AuthenticationManager.Instance == null) return;
            AuthenticationManager.Instance.OnPlayerSignedInWithGoogle -= HandlePlayerSignedIn;
            AuthenticationManager.Instance.OnPlayerSignInFailed       -= HandlePlayerSignInFailed;
        }

        // ── Button click handlers ──────────────────────────────────────────

        private void HandleGoogleSignInButtonClicked()
        {
            SetSignInLoadingSpinnerVisible(true);
            AuthenticationManager.Instance.BeginGoogleSignInFlow();
        }

        private void HandleContinueAsGuestButtonClicked()
        {
            AuthenticationManager.Instance.ContinueAsGuest();
            RefreshSignInStatusLabel();
            LoadSafariZoneScene();
        }

        private void HandlePlayButtonClicked()
        {
            LoadSafariZoneScene();
        }

        private void HandleCharacterCustomizationButtonClicked()
        {
            GameManager.Instance.GameStateController.TransitionToGameState(
                GameStateController.GameState.CharacterCustomization);

            UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SCENE_NAME_CHARACTER_CUSTOMIZATION);
        }

        private void HandleFullScreenToggleButtonClicked()
        {
            PlatformManager.Instance.ToggleFullScreenOnDesktop();
        }

        // ── Auth event handlers ────────────────────────────────────────────

        private void HandlePlayerSignedIn(string googleUserId)
        {
            SetSignInLoadingSpinnerVisible(false);
            RefreshSignInStatusLabel();
            Debug.Log($"[MainMenuUI] Sign-in succeeded for user: {googleUserId}");
        }

        private void HandlePlayerSignInFailed(string errorMessage)
        {
            SetSignInLoadingSpinnerVisible(false);
            Debug.LogWarning($"[MainMenuUI] Sign-in failed: {errorMessage}");
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void SetSignInLoadingSpinnerVisible(bool isVisible)
        {
            if (signInLoadingSpinnerPanel != null)
                signInLoadingSpinnerPanel.SetActive(isVisible);
        }

        private void LoadSafariZoneScene()
        {
            GameManager.Instance.GameStateController.TransitionToGameState(
                GameStateController.GameState.Playing);

            UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SCENE_NAME_SAFARI_ZONE);
        }
    }
}
