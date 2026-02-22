using System;
using UnityEngine;
using CrudCustodian.Core;
using CrudCustodian.Data;

namespace CrudCustodian.Stalls
{
    /// <summary>
    /// Central manager for stall progression.  Tracks which stalls are unlocked,
    /// calculates unlock costs using the exponential formula defined in
    /// GameConstants, and gates purchases through CurrencyManager.
    ///
    /// Unlock cost formula:
    ///   Stall 0 (index 0) → Free (0 coins)
    ///   Stall n (n > 0)   → STALL_UNLOCK_BASE_COST * (STALL_UNLOCK_COST_MULTIPLIER ^ (n - 1))
    ///   Rounded to the nearest integer.
    /// </summary>
    public class StallUnlockManager : MonoBehaviour
    {
        // ── Events ─────────────────────────────────────────────────────────
        /// <summary>Fires when a stall is successfully unlocked. Passes stall index.</summary>
        public event Action<int> OnStallSuccessfullyUnlocked;

        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Stall References")]
        [Tooltip("All PokemonStall components on the map, ordered by their stallUnlockOrderIndex.")]
        [SerializeField] private PokemonStall[] allStallsOnMap;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Start()
        {
            RestoreUnlockedStallsFromSaveData();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the coin cost required to unlock the stall at the given index.
        /// Index 0 is always free.
        /// </summary>
        public int CalculateUnlockCostForStallAtIndex(int stallIndex)
        {
            // Allow per-stall overrides via StallData.
            if (stallIndex < allStallsOnMap.Length)
            {
                int overrideCost = allStallsOnMap[stallIndex].StallConfigurationData.overriddenUnlockCostInCoins;
                if (overrideCost >= 0)
                    return overrideCost;
            }

            if (stallIndex == 0)
                return GameConstants.FIRST_STALL_UNLOCK_COST_IN_COINS;

            float calculatedCost = GameConstants.STALL_UNLOCK_BASE_COST_IN_COINS
                * Mathf.Pow(GameConstants.STALL_UNLOCK_COST_MULTIPLIER_PER_TIER, stallIndex - 1);

            return Mathf.RoundToInt(calculatedCost);
        }

        /// <summary>
        /// Returns the index of the next stall the player has NOT yet unlocked,
        /// or -1 if all stalls are already unlocked.
        /// </summary>
        public int GetIndexOfNextLockedStall()
        {
            int numberOfStallsAlreadyUnlocked =
                GameManager.Instance.SaveDataManager.LoadedPlayerData.numberOfStallsUnlocked;

            if (numberOfStallsAlreadyUnlocked >= GameConstants.TOTAL_NUMBER_OF_STALLS_ON_MAP)
                return -1;

            return numberOfStallsAlreadyUnlocked;
        }

        /// <summary>
        /// Attempts to unlock the next available stall for the player.
        /// Deducts coins if required.  Returns true if the unlock succeeded.
        /// </summary>
        public bool TryUnlockNextStall()
        {
            int nextStallIndexToUnlock = GetIndexOfNextLockedStall();

            if (nextStallIndexToUnlock < 0)
            {
                Debug.Log("[StallUnlockManager] All stalls are already unlocked.");
                return false;
            }

            int unlockCost = CalculateUnlockCostForStallAtIndex(nextStallIndexToUnlock);
            bool purchaseSucceeded = GameManager.Instance.CurrencyManager.TrySpendCoins(unlockCost);

            if (!purchaseSucceeded)
            {
                Debug.Log($"[StallUnlockManager] Cannot afford stall {nextStallIndexToUnlock}. " +
                          $"Cost: {unlockCost} coins.");
                return false;
            }

            ActivateStallAtIndex(nextStallIndexToUnlock);

            int newTotalUnlockedStallCount = nextStallIndexToUnlock + 1;
            GameManager.Instance.SaveDataManager.SaveNumberOfStallsUnlocked(newTotalUnlockedStallCount);

            OnStallSuccessfullyUnlocked?.Invoke(nextStallIndexToUnlock);

            Debug.Log($"[StallUnlockManager] Stall {nextStallIndexToUnlock} unlocked for {unlockCost} coins.");
            return true;
        }

        // ── Private helpers ────────────────────────────────────────────────

        /// <summary>
        /// On scene load, re-activates any stalls the player already unlocked
        /// in a previous session.
        /// </summary>
        private void RestoreUnlockedStallsFromSaveData()
        {
            int numberOfStallsAlreadyUnlocked =
                GameManager.Instance.SaveDataManager.LoadedPlayerData.numberOfStallsUnlocked;

            for (int stallIndex = 0; stallIndex < numberOfStallsAlreadyUnlocked; stallIndex++)
            {
                ActivateStallAtIndex(stallIndex);
            }

            Debug.Log($"[StallUnlockManager] Restored {numberOfStallsAlreadyUnlocked} unlocked stalls.");
        }

        private void ActivateStallAtIndex(int stallIndex)
        {
            if (stallIndex >= allStallsOnMap.Length)
            {
                Debug.LogWarning($"[StallUnlockManager] Stall index {stallIndex} is out of range. " +
                                 $"Only {allStallsOnMap.Length} stalls are registered.");
                return;
            }

            allStallsOnMap[stallIndex].UnlockThisStall();
        }
    }
}
