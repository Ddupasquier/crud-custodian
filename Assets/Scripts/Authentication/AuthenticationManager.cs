using System;
using UnityEngine;
using CrudCustodian.Core;

namespace CrudCustodian.Authentication
{
    /// <summary>
    /// Manages the player's authentication state for the app.
    /// Supports Google Sign-In and guest (offline) play.
    /// Delegates the platform-specific sign-in flow to GoogleAuthManager.
    ///
    /// Attach to the Auth root GameObject (kept alive via DontDestroyOnLoad).
    /// </summary>
    public class AuthenticationManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static AuthenticationManager Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
        /// <summary>Fired when the player successfully signs in. Passes the Google user ID.</summary>
        public event Action<string> OnPlayerSignedInWithGoogle;

        /// <summary>Fired if sign-in fails or is cancelled. Passes an error message.</summary>
        public event Action<string> OnPlayerSignInFailed;

        /// <summary>Fired when the player signs out or chooses to play as a guest.</summary>
        public event Action OnPlayerSignedOut;

        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Component References")]
        [Tooltip("The GoogleAuthManager component that handles the low-level OAuth flow.")]
        [SerializeField] private GoogleAuthManager googleAuthManager;

        // ── Runtime state ──────────────────────────────────────────────────
        private bool isPlayerCurrentlySignedIn;
        private string currentPlayerGoogleUserId;
        private string currentPlayerGoogleDisplayName;

        // ── Public read-only properties ────────────────────────────────────
        public bool IsPlayerCurrentlySignedIn           => isPlayerCurrentlySignedIn;
        public string CurrentPlayerGoogleUserId         => currentPlayerGoogleUserId;
        public string CurrentPlayerGoogleDisplayName    => currentPlayerGoogleDisplayName;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            EnforceSingletonPattern();
        }

        private void Start()
        {
            RestoreSignInStateFromSaveData();
            SubscribeToGoogleAuthManagerEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromGoogleAuthManagerEvents();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Starts the Google Sign-In flow.  The result is delivered via
        /// OnPlayerSignedInWithGoogle or OnPlayerSignInFailed events.
        /// </summary>
        public void BeginGoogleSignInFlow()
        {
            Debug.Log("[AuthenticationManager] Starting Google Sign-In flow.");
            googleAuthManager.StartGoogleSignIn();
        }

        /// <summary>
        /// Signs the player out and clears all stored auth credentials.
        /// </summary>
        public void SignOut()
        {
            isPlayerCurrentlySignedIn      = false;
            currentPlayerGoogleUserId      = string.Empty;
            currentPlayerGoogleDisplayName = string.Empty;

            // Remove stored credentials from save data.
            GameManager.Instance.SaveDataManager.LoadedPlayerData.googleAccountUserId      = string.Empty;
            GameManager.Instance.SaveDataManager.LoadedPlayerData.googleAccountDisplayName = string.Empty;
            GameManager.Instance.SaveDataManager.SaveAllPlayerData(
                GameManager.Instance.SaveDataManager.LoadedPlayerData);

            OnPlayerSignedOut?.Invoke();
            Debug.Log("[AuthenticationManager] Player signed out.");
        }

        /// <summary>
        /// Skips sign-in and lets the player continue as a guest.
        /// Progress is saved locally but not to a cloud account.
        /// </summary>
        public void ContinueAsGuest()
        {
            isPlayerCurrentlySignedIn      = false;
            currentPlayerGoogleUserId      = string.Empty;
            currentPlayerGoogleDisplayName = "Guest";

            Debug.Log("[AuthenticationManager] Player chose to continue as guest.");
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void EnforceSingletonPattern()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void RestoreSignInStateFromSaveData()
        {
            string savedGoogleUserId = GameManager.Instance.SaveDataManager.LoadedPlayerData.googleAccountUserId;

            if (!string.IsNullOrEmpty(savedGoogleUserId))
            {
                currentPlayerGoogleUserId      = savedGoogleUserId;
                currentPlayerGoogleDisplayName = GameManager.Instance.SaveDataManager.LoadedPlayerData.googleAccountDisplayName;
                isPlayerCurrentlySignedIn      = true;

                Debug.Log($"[AuthenticationManager] Restored sign-in for user: {currentPlayerGoogleDisplayName}");
            }
        }

        private void SubscribeToGoogleAuthManagerEvents()
        {
            googleAuthManager.OnGoogleSignInSucceeded += HandleGoogleSignInSucceeded;
            googleAuthManager.OnGoogleSignInFailed    += HandleGoogleSignInFailed;
        }

        private void UnsubscribeFromGoogleAuthManagerEvents()
        {
            if (googleAuthManager == null) return;
            googleAuthManager.OnGoogleSignInSucceeded -= HandleGoogleSignInSucceeded;
            googleAuthManager.OnGoogleSignInFailed    -= HandleGoogleSignInFailed;
        }

        private void HandleGoogleSignInSucceeded(string googleUserId, string googleDisplayName)
        {
            isPlayerCurrentlySignedIn      = true;
            currentPlayerGoogleUserId      = googleUserId;
            currentPlayerGoogleDisplayName = googleDisplayName;

            // Persist credentials so we can restore them next launch.
            GameManager.Instance.SaveDataManager.LoadedPlayerData.googleAccountUserId      = googleUserId;
            GameManager.Instance.SaveDataManager.LoadedPlayerData.googleAccountDisplayName = googleDisplayName;
            GameManager.Instance.SaveDataManager.SaveAllPlayerData(
                GameManager.Instance.SaveDataManager.LoadedPlayerData);

            OnPlayerSignedInWithGoogle?.Invoke(googleUserId);
            Debug.Log($"[AuthenticationManager] Google sign-in succeeded for: {googleDisplayName} (ID: {googleUserId})");
        }

        private void HandleGoogleSignInFailed(string errorMessage)
        {
            isPlayerCurrentlySignedIn = false;
            OnPlayerSignInFailed?.Invoke(errorMessage);
            Debug.LogWarning($"[AuthenticationManager] Google sign-in failed: {errorMessage}");
        }
    }
}
