using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrudCustodian.Core;
using CrudCustodian.Data;

namespace CrudCustodian.UI
{
    /// <summary>
    /// Shows the player all available stalls, their unlock costs, and their
    /// automation costs.  Lets the player unlock the next stall or automate
    /// any already-unlocked stall.
    ///
    /// Works identically on desktop and mobile; all interaction is via UI buttons.
    /// </summary>
    public class StallUnlockUI : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Stall Panel Prefab")]
        [Tooltip("Prefab instantiated once per stall in the scroll list. Must have a StallListItemUI component.")]
        [SerializeField] private StallListItemUI stallListItemUIPrefab;

        [Tooltip("Container transform inside the scroll view where stall list items are spawned.")]
        [SerializeField] private Transform stallListScrollViewContent;

        [Header("Next Unlock Preview")]
        [Tooltip("Shows the name of the next stall available to unlock.")]
        [SerializeField] private TextMeshProUGUI nextStallToUnlockNameLabel;

        [Tooltip("Shows the coin cost to unlock the next stall. Displays 'FREE' for the first one.")]
        [SerializeField] private TextMeshProUGUI nextStallUnlockCostLabel;

        [Tooltip("Button that triggers unlocking the next stall.")]
        [SerializeField] private Button unlockNextStallButton;

        [Header("Stall Manager References")]
        [SerializeField] private Stalls.StallUnlockManager stallUnlockManager;
        [SerializeField] private Stalls.StallAutomationManager stallAutomationManager;

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Start()
        {
            BuildStallListUI();
            RefreshNextUnlockPreview();
            unlockNextStallButton?.onClick.AddListener(HandleUnlockNextStallButtonClicked);
            stallUnlockManager.OnStallSuccessfullyUnlocked += HandleStallUnlocked;
            stallAutomationManager.OnStallSuccessfullyAutomated += HandleStallAutomated;
        }

        private void OnDestroy()
        {
            unlockNextStallButton?.onClick.RemoveListener(HandleUnlockNextStallButtonClicked);
            if (stallUnlockManager != null)
                stallUnlockManager.OnStallSuccessfullyUnlocked -= HandleStallUnlocked;
            if (stallAutomationManager != null)
                stallAutomationManager.OnStallSuccessfullyAutomated -= HandleStallAutomated;
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void BuildStallListUI()
        {
            if (stallListItemUIPrefab == null || stallListScrollViewContent == null) return;

            // Clear any pre-existing items (e.g. after a scene reload).
            foreach (Transform existingItem in stallListScrollViewContent)
                Destroy(existingItem.gameObject);

            for (int stallIndex = 0; stallIndex < GameConstants.TOTAL_NUMBER_OF_STALLS_ON_MAP; stallIndex++)
            {
                StallListItemUI spawnedListItem =
                    Instantiate(stallListItemUIPrefab, stallListScrollViewContent);

                int unlockCost     = stallUnlockManager.CalculateUnlockCostForStallAtIndex(stallIndex);
                int automationCost = stallAutomationManager.CalculateAutomationCostForStallAtIndex(stallIndex);
                bool isUnlocked    = stallIndex < GameManager.Instance.SaveDataManager.LoadedPlayerData.numberOfStallsUnlocked;
                bool isAutomated   = stallAutomationManager.IsStallAutomated(stallIndex);

                spawnedListItem.Initialize(
                    stallIndex:        stallIndex,
                    isUnlocked:        isUnlocked,
                    isAutomated:       isAutomated,
                    unlockCostInCoins: unlockCost,
                    automationCostInCoins: automationCost,
                    onAutomateButtonClicked: HandleAutomateButtonClicked);
            }
        }

        private void RefreshNextUnlockPreview()
        {
            int nextStallIndex = stallUnlockManager.GetIndexOfNextLockedStall();

            if (nextStallIndex < 0)
            {
                nextStallToUnlockNameLabel.text  = "All stalls unlocked!";
                nextStallUnlockCostLabel.text    = "";
                unlockNextStallButton.interactable = false;
                return;
            }

            int unlockCost = stallUnlockManager.CalculateUnlockCostForStallAtIndex(nextStallIndex);
            nextStallToUnlockNameLabel.text = $"Stall {nextStallIndex + 1}";
            nextStallUnlockCostLabel.text   = unlockCost == 0 ? "FREE" : $"{unlockCost:N0} 🪙";

            unlockNextStallButton.interactable =
                unlockCost == 0 || GameManager.Instance.CurrencyManager.CanAfford(unlockCost);
        }

        private void HandleUnlockNextStallButtonClicked()
        {
            if (stallUnlockManager.TryUnlockNextStall())
                RebuildAndRefreshUI();
        }

        private void HandleAutomateButtonClicked(int stallIndex)
        {
            if (stallAutomationManager.TryAutomateStallAtIndex(stallIndex))
                RebuildAndRefreshUI();
        }

        private void HandleStallUnlocked(int unlockedStallIndex)
        {
            RebuildAndRefreshUI();
        }

        private void HandleStallAutomated(int automatedStallIndex)
        {
            RebuildAndRefreshUI();
        }

        private void RebuildAndRefreshUI()
        {
            BuildStallListUI();
            RefreshNextUnlockPreview();
        }
    }
}
