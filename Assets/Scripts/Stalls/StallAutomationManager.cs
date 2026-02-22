using System;
using UnityEngine;
using CrudCustodian.Core;

namespace CrudCustodian.Stalls
{
    /// <summary>
    /// Handles purchasing automation for individual stalls.
    /// Automation allows a stall to collect its own poop without player involvement,
    /// earning a passive but reduced coin income stream.
    ///
    /// Automation cost formula:
    ///   Cost = STALL_AUTOMATION_BASE_COST + (stallIndex * STALL_AUTOMATION_ADDITIONAL_COST_PER_INDEX)
    /// </summary>
    public class StallAutomationManager : MonoBehaviour
    {
        // ── Events ─────────────────────────────────────────────────────────
        /// <summary>Fires when a stall is successfully automated. Passes stall index.</summary>
        public event Action<int> OnStallSuccessfullyAutomated;

        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Stall References")]
        [Tooltip("All PokemonStall components on the map, ordered by stallUnlockOrderIndex.")]
        [SerializeField] private PokemonStall[] allStallsOnMap;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Start()
        {
            RestoreAutomatedStallsFromSaveData();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the coin cost to automate the stall at the given index.
        /// Later stalls cost more to automate.
        /// </summary>
        public int CalculateAutomationCostForStallAtIndex(int stallIndex)
        {
            // Allow per-stall overrides via StallData.
            if (stallIndex < allStallsOnMap.Length)
            {
                int overrideCost = allStallsOnMap[stallIndex].StallConfigurationData.overriddenAutomationCostInCoins;
                if (overrideCost >= 0)
                    return overrideCost;
            }

            return GameConstants.STALL_AUTOMATION_BASE_COST_IN_COINS
                + (stallIndex * GameConstants.STALL_AUTOMATION_ADDITIONAL_COST_PER_INDEX);
        }

        /// <summary>
        /// Returns true when the stall at the given index is already automated.
        /// </summary>
        public bool IsStallAutomated(int stallIndex)
        {
            if (stallIndex < 0 || stallIndex >= allStallsOnMap.Length) return false;
            return allStallsOnMap[stallIndex].IsStallCurrentlyAutomated;
        }

        /// <summary>
        /// Attempts to purchase automation for the stall at the given index.
        /// The stall must be unlocked first.  Returns true on success.
        /// </summary>
        public bool TryAutomateStallAtIndex(int stallIndex)
        {
            if (stallIndex < 0 || stallIndex >= allStallsOnMap.Length)
            {
                Debug.LogWarning($"[StallAutomationManager] Invalid stall index: {stallIndex}");
                return false;
            }

            PokemonStall targetStall = allStallsOnMap[stallIndex];

            if (!targetStall.IsStallCurrentlyUnlocked)
            {
                Debug.Log($"[StallAutomationManager] Stall {stallIndex} must be unlocked before automation.");
                return false;
            }

            if (targetStall.IsStallCurrentlyAutomated)
            {
                Debug.Log($"[StallAutomationManager] Stall {stallIndex} is already automated.");
                return false;
            }

            int automationCost = CalculateAutomationCostForStallAtIndex(stallIndex);
            bool purchaseSucceeded = GameManager.Instance.CurrencyManager.TrySpendCoins(automationCost);

            if (!purchaseSucceeded)
            {
                Debug.Log($"[StallAutomationManager] Cannot afford automation for stall {stallIndex}. " +
                          $"Cost: {automationCost} coins.");
                return false;
            }

            targetStall.AutomateThisStall();

            // Persist the new automation flag.
            bool[] currentAutomationFlags = BuildCurrentAutomationFlagsArray();
            currentAutomationFlags[stallIndex] = true;
            GameManager.Instance.SaveDataManager.SaveStallAutomationFlags(currentAutomationFlags);

            OnStallSuccessfullyAutomated?.Invoke(stallIndex);

            Debug.Log($"[StallAutomationManager] Stall {stallIndex} automated for {automationCost} coins.");
            return true;
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void RestoreAutomatedStallsFromSaveData()
        {
            bool[] savedAutomationFlags =
                GameManager.Instance.SaveDataManager.LoadedPlayerData.stallAutomationFlags;

            if (savedAutomationFlags == null) return;

            for (int stallIndex = 0; stallIndex < savedAutomationFlags.Length && stallIndex < allStallsOnMap.Length; stallIndex++)
            {
                if (savedAutomationFlags[stallIndex])
                    allStallsOnMap[stallIndex].AutomateThisStall();
            }

            Debug.Log("[StallAutomationManager] Restored automated stall states from save data.");
        }

        private bool[] BuildCurrentAutomationFlagsArray()
        {
            bool[] flagsArray = new bool[allStallsOnMap.Length];
            for (int i = 0; i < allStallsOnMap.Length; i++)
                flagsArray[i] = allStallsOnMap[i].IsStallCurrentlyAutomated;
            return flagsArray;
        }
    }
}
