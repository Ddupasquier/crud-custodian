using System;
using UnityEngine;

namespace CrudCustodian.Core
{
    /// <summary>
    /// Manages the player's coin balance.  All earning and spending must go
    /// through this class so the balance never goes negative and the save
    /// system always stays in sync.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        // ── Events ─────────────────────────────────────────────────────────
        /// <summary>Fired whenever the coin balance changes. Passes new balance.</summary>
        public event Action<int> OnCoinBalanceChanged;

        // ── Internal state ─────────────────────────────────────────────────
        private int currentCoinBalance;

        // ── Public read-only property ──────────────────────────────────────
        public int CurrentCoinBalance => currentCoinBalance;

        // ── Initialization ─────────────────────────────────────────────────

        /// <summary>
        /// Called by GameManager after save data has been loaded.
        /// </summary>
        /// <param name="savedCoinBalance">The coin balance restored from disk.</param>
        public void Initialize(int savedCoinBalance)
        {
            currentCoinBalance = savedCoinBalance;
            Debug.Log($"[CurrencyManager] Initialized with {currentCoinBalance} coins.");
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Adds the given number of coins to the balance (e.g. from cleaning poop).
        /// </summary>
        /// <param name="coinsToAdd">Must be a positive integer.</param>
        public void AddCoins(int coinsToAdd)
        {
            if (coinsToAdd <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] AddCoins called with non-positive value: {coinsToAdd}");
                return;
            }

            currentCoinBalance += coinsToAdd;
            Debug.Log($"[CurrencyManager] +{coinsToAdd} coins. New balance: {currentCoinBalance}");
            NotifyBalanceChanged();
        }

        /// <summary>
        /// Attempts to spend coins. Returns true if successful, false if the
        /// player cannot afford it.
        /// </summary>
        /// <param name="coinsToSpend">Amount to deduct from the balance.</param>
        /// <returns>True when the purchase succeeds.</returns>
        public bool TrySpendCoins(int coinsToSpend)
        {
            if (coinsToSpend <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] TrySpendCoins called with non-positive value: {coinsToSpend}");
                return false;
            }

            if (currentCoinBalance < coinsToSpend)
            {
                Debug.Log($"[CurrencyManager] Not enough coins. Need {coinsToSpend}, have {currentCoinBalance}.");
                return false;
            }

            currentCoinBalance -= coinsToSpend;
            Debug.Log($"[CurrencyManager] -{coinsToSpend} coins. New balance: {currentCoinBalance}");
            NotifyBalanceChanged();
            return true;
        }

        /// <summary>
        /// Returns true when the player can afford the given cost without spending.
        /// </summary>
        public bool CanAfford(int coinCost) => currentCoinBalance >= coinCost;

        // ── Private helpers ────────────────────────────────────────────────

        private void NotifyBalanceChanged()
        {
            OnCoinBalanceChanged?.Invoke(currentCoinBalance);

            // Keep save data in sync immediately so progress is never lost.
            GameManager.Instance.SaveDataManager.SaveCurrentCoinBalance(currentCoinBalance);
        }
    }
}
