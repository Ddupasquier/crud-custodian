using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace CrudCustodian.UI
{
    /// <summary>
    /// A single row in the Stall Unlock UI scroll list.
    /// Displays the stall's index, unlock / automation status, and costs.
    /// Attach to the StallListItem prefab.
    /// </summary>
    public class StallListItemUI : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Labels")]
        [Tooltip("Displays the stall number (e.g. 'Stall 3').")]
        [SerializeField] private TextMeshProUGUI stallNumberLabel;

        [Tooltip("Displays the stall's current status: Locked / Unlocked / Automated.")]
        [SerializeField] private TextMeshProUGUI stallStatusLabel;

        [Tooltip("Shows the automation cost (e.g. 'Automate: 2,000 🪙').")]
        [SerializeField] private TextMeshProUGUI automationCostLabel;

        [Header("Buttons")]
        [Tooltip("Button that requests automation for this stall. Hidden when locked or already automated.")]
        [SerializeField] private Button automateThisStallButton;

        // ── Private state ──────────────────────────────────────────────────
        private int thisStallIndex;
        private Action<int> onAutomateButtonClickedCallback;

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Populates all UI elements for this list item.
        /// Called by StallUnlockUI immediately after instantiation.
        /// </summary>
        public void Initialize(
            int      stallIndex,
            bool     isUnlocked,
            bool     isAutomated,
            int      unlockCostInCoins,
            int      automationCostInCoins,
            Action<int> onAutomateButtonClicked)
        {
            thisStallIndex                = stallIndex;
            onAutomateButtonClickedCallback = onAutomateButtonClicked;

            stallNumberLabel.text = $"Stall {stallIndex + 1}";

            if (isAutomated)
            {
                stallStatusLabel.text = "✅ Automated";
                automationCostLabel.text = "";
                automateThisStallButton.gameObject.SetActive(false);
            }
            else if (isUnlocked)
            {
                stallStatusLabel.text = "🔓 Unlocked";
                automationCostLabel.text = $"Automate: {automationCostInCoins:N0} 🪙";
                automateThisStallButton.gameObject.SetActive(true);
                automateThisStallButton.onClick.AddListener(HandleAutomateButtonClicked);
            }
            else
            {
                stallStatusLabel.text = unlockCostInCoins == 0
                    ? "🔒 Locked (FREE to unlock)"
                    : $"🔒 Locked ({unlockCostInCoins:N0} 🪙 to unlock)";
                automationCostLabel.text = "";
                automateThisStallButton.gameObject.SetActive(false);
            }
        }

        // ── Button handler ─────────────────────────────────────────────────

        private void HandleAutomateButtonClicked()
        {
            onAutomateButtonClickedCallback?.Invoke(thisStallIndex);
        }
    }
}
