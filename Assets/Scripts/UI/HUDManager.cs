using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrudCustodian.Core;

namespace CrudCustodian.UI
{
    /// <summary>
    /// In-game HUD: shows the coin balance, a pause button, and — on desktop —
    /// a full-screen toggle button.
    ///
    /// Subscribe to CurrencyManager.OnCoinBalanceChanged so the coin label
    /// always reflects the live balance without polling every frame.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Coin Display")]
        [Tooltip("Text label that shows the player's current coin balance.")]
        [SerializeField] private TextMeshProUGUI currentCoinBalanceLabel;

        [Header("Buttons")]
        [Tooltip("Button that opens the pause menu.")]
        [SerializeField] private Button pauseMenuButton;

        [Tooltip("Button that toggles full-screen mode. Auto-hidden on mobile.")]
        [SerializeField] private Button desktopFullScreenToggleButton;

        [Header("Notification")]
        [Tooltip("Short text that pops up briefly when the player earns coins.")]
        [SerializeField] private TextMeshProUGUI coinEarnedPopupLabel;

        [Tooltip("How many seconds the coin-earned popup remains visible.")]
        [SerializeField] [Min(0.1f)] private float coinEarnedPopupDurationSeconds = 1.5f;

        // ── Private state ──────────────────────────────────────────────────
        private float coinEarnedPopupSecondsRemaining;

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Start()
        {
            ConfigureDesktopOnlyElements();
            RefreshCoinBalanceLabel(GameManager.Instance.CurrencyManager.CurrentCoinBalance);
            SubscribeToCurrencyManagerEvents();
            SubscribeToButtonClickEvents();
        }

        private void Update()
        {
            TickCoinEarnedPopupTimer();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCurrencyManagerEvents();
            UnsubscribeFromButtonClickEvents();
        }

        // ── Setup helpers ──────────────────────────────────────────────────

        private void ConfigureDesktopOnlyElements()
        {
            // The full-screen toggle is only useful on desktop; hide it on mobile.
            if (desktopFullScreenToggleButton != null)
            {
                desktopFullScreenToggleButton.gameObject.SetActive(
                    PlatformManager.Instance.IsAnyDesktopPlatform);
            }
        }

        private void SubscribeToCurrencyManagerEvents()
        {
            GameManager.Instance.CurrencyManager.OnCoinBalanceChanged += RefreshCoinBalanceLabel;
        }

        private void UnsubscribeFromCurrencyManagerEvents()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.CurrencyManager.OnCoinBalanceChanged -= RefreshCoinBalanceLabel;
        }

        private void SubscribeToButtonClickEvents()
        {
            pauseMenuButton?.onClick.AddListener(HandlePauseMenuButtonClicked);
            desktopFullScreenToggleButton?.onClick.AddListener(HandleFullScreenToggleButtonClicked);
        }

        private void UnsubscribeFromButtonClickEvents()
        {
            pauseMenuButton?.onClick.RemoveListener(HandlePauseMenuButtonClicked);
            desktopFullScreenToggleButton?.onClick.RemoveListener(HandleFullScreenToggleButtonClicked);
        }

        // ── Event handlers ─────────────────────────────────────────────────

        private void RefreshCoinBalanceLabel(int newCoinBalance)
        {
            if (currentCoinBalanceLabel != null)
                currentCoinBalanceLabel.text = $"🪙 {newCoinBalance:N0}";
        }

        private void HandlePauseMenuButtonClicked()
        {
            GameManager.Instance.GameStateController.TransitionToGameState(
                GameStateController.GameState.Paused);
        }

        private void HandleFullScreenToggleButtonClicked()
        {
            PlatformManager.Instance.ToggleFullScreenOnDesktop();
        }

        // ── Coin popup ─────────────────────────────────────────────────────

        /// <summary>
        /// Shows the "+N coins" popup label briefly above the HUD.
        /// Called by PlayerPoopCollector after each successful collection.
        /// </summary>
        public void ShowCoinEarnedPopup(int coinsJustEarned)
        {
            if (coinEarnedPopupLabel == null) return;

            coinEarnedPopupLabel.text    = $"+{coinsJustEarned} 🪙";
            coinEarnedPopupLabel.enabled = true;
            coinEarnedPopupSecondsRemaining = coinEarnedPopupDurationSeconds;
        }

        private void TickCoinEarnedPopupTimer()
        {
            if (coinEarnedPopupSecondsRemaining <= 0f || coinEarnedPopupLabel == null) return;

            coinEarnedPopupSecondsRemaining -= Time.deltaTime;

            if (coinEarnedPopupSecondsRemaining <= 0f)
                coinEarnedPopupLabel.enabled = false;
        }
    }
}
