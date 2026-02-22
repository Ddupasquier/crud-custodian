using UnityEngine;
using CrudCustodian.Data;

namespace CrudCustodian.Core
{
    /// <summary>
    /// Reads and writes all persistent player data to and from PlayerPrefs
    /// (JSON-serialized).  A proper cloud-save solution can replace PlayerPrefs
    /// here without touching any other system.
    /// </summary>
    public class SaveDataManager : MonoBehaviour
    {
        // ── Constants ──────────────────────────────────────────────────────
        private const string PLAYER_DATA_SAVE_KEY = "CrudCustodian_PlayerSaveData_v1";

        // ── Internal state ─────────────────────────────────────────────────
        private PlayerProgressionData loadedPlayerData;

        // ── Public read-only property ──────────────────────────────────────
        /// <summary>
        /// The player's save data loaded during Initialize().
        /// Always contains at least default values; never null.
        /// </summary>
        public PlayerProgressionData LoadedPlayerData => loadedPlayerData;

        // ── Initialization ─────────────────────────────────────────────────

        /// <summary>
        /// Loads data from disk.  Called by GameManager before other managers
        /// so they can read the restored values.
        /// </summary>
        public void Initialize()
        {
            loadedPlayerData = LoadPlayerDataFromDisk();
            Debug.Log($"[SaveDataManager] Loaded player data. Coins: {loadedPlayerData.currentCoinBalance}, " +
                      $"Stalls unlocked: {loadedPlayerData.numberOfStallsUnlocked}");
        }

        // ── Public save helpers ────────────────────────────────────────────

        /// <summary>Persists the full player progression data object to disk.</summary>
        public void SaveAllPlayerData(PlayerProgressionData dataToSave)
        {
            string serializedJson = JsonUtility.ToJson(dataToSave, prettyPrint: true);
            PlayerPrefs.SetString(PLAYER_DATA_SAVE_KEY, serializedJson);
            PlayerPrefs.Save();
            loadedPlayerData = dataToSave;
            Debug.Log("[SaveDataManager] Full player data saved.");
        }

        /// <summary>
        /// Convenience method — updates only the coin balance without requiring
        /// the caller to hold a reference to the full data object.
        /// </summary>
        public void SaveCurrentCoinBalance(int updatedCoinBalance)
        {
            loadedPlayerData.currentCoinBalance = updatedCoinBalance;
            SaveAllPlayerData(loadedPlayerData);
        }

        /// <summary>
        /// Convenience method — updates the number of unlocked stalls.
        /// </summary>
        public void SaveNumberOfStallsUnlocked(int updatedStallCount)
        {
            loadedPlayerData.numberOfStallsUnlocked = updatedStallCount;
            SaveAllPlayerData(loadedPlayerData);
        }

        /// <summary>
        /// Convenience method — updates which stalls are automated.
        /// </summary>
        public void SaveStallAutomationFlags(bool[] updatedStallAutomationFlags)
        {
            loadedPlayerData.stallAutomationFlags = updatedStallAutomationFlags;
            SaveAllPlayerData(loadedPlayerData);
        }

        /// <summary>
        /// Convenience method — updates the selected character body type index.
        /// </summary>
        public void SaveSelectedCharacterBodyTypeIndex(int selectedBodyTypeIndex)
        {
            loadedPlayerData.selectedCharacterBodyTypeIndex = selectedBodyTypeIndex;
            SaveAllPlayerData(loadedPlayerData);
        }

        /// <summary>
        /// Convenience method — updates the selected character color palette index.
        /// </summary>
        public void SaveSelectedCharacterColorPaletteIndex(int selectedColorPaletteIndex)
        {
            loadedPlayerData.selectedCharacterColorPaletteIndex = selectedColorPaletteIndex;
            SaveAllPlayerData(loadedPlayerData);
        }

        /// <summary>Wipes all saved data and resets to defaults (for testing / account reset).</summary>
        public void DeleteAllSavedDataAndResetToDefaults()
        {
            PlayerPrefs.DeleteKey(PLAYER_DATA_SAVE_KEY);
            PlayerPrefs.Save();
            loadedPlayerData = PlayerProgressionData.CreateDefaultData();
            Debug.Log("[SaveDataManager] All save data deleted and reset to defaults.");
        }

        // ── Private helpers ────────────────────────────────────────────────

        private PlayerProgressionData LoadPlayerDataFromDisk()
        {
            if (!PlayerPrefs.HasKey(PLAYER_DATA_SAVE_KEY))
            {
                Debug.Log("[SaveDataManager] No existing save found. Creating default player data.");
                return PlayerProgressionData.CreateDefaultData();
            }

            string serializedJson = PlayerPrefs.GetString(PLAYER_DATA_SAVE_KEY);
            PlayerProgressionData restoredData = JsonUtility.FromJson<PlayerProgressionData>(serializedJson);

            if (restoredData == null)
            {
                Debug.LogWarning("[SaveDataManager] Save data was corrupt. Resetting to defaults.");
                return PlayerProgressionData.CreateDefaultData();
            }

            return restoredData;
        }
    }
}
