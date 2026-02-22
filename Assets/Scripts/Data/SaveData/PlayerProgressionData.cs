using System;
using UnityEngine;

namespace CrudCustodian.Data
{
    /// <summary>
    /// All data that must persist between sessions for a single player.
    /// Serialized to JSON and stored in PlayerPrefs (or a cloud save backend).
    /// Add new fields here and update CreateDefaultData() accordingly.
    /// </summary>
    [Serializable]
    public class PlayerProgressionData
    {
        // ── Currency ───────────────────────────────────────────────────────

        [Tooltip("How many coins the player currently has.")]
        public int currentCoinBalance;

        [Tooltip("Lifetime total coins ever earned (used for stats / achievements).")]
        public int lifetimeTotalCoinsEarned;

        // ── Stall progression ──────────────────────────────────────────────

        [Tooltip("How many stalls the player has unlocked (includes the first free stall).")]
        public int numberOfStallsUnlocked;

        [Tooltip("One flag per stall (indexed to match StallData.stallUnlockOrderIndex). " +
                 "True means the player has paid to automate that stall.")]
        public bool[] stallAutomationFlags;

        // ── Character customization ────────────────────────────────────────

        [Tooltip("Index into the CharacterCustomizationConfig.availableBodyTypes array.")]
        public int selectedCharacterBodyTypeIndex;

        [Tooltip("Index into the CharacterCustomizationConfig.availableColorPalettes array.")]
        public int selectedCharacterColorPaletteIndex;

        [Tooltip("Index into the CharacterCustomizationConfig.availableHatOptions array.")]
        public int selectedCharacterHatOptionIndex;

        // ── Authentication ─────────────────────────────────────────────────

        [Tooltip("The Google user ID returned after a successful Google Sign-In. " +
                 "Empty string means the player is playing as a guest.")]
        public string googleAccountUserId;

        [Tooltip("Display name pulled from the player's Google account.")]
        public string googleAccountDisplayName;

        // ── Statistics ─────────────────────────────────────────────────────

        [Tooltip("Total number of poop piles the player has cleaned across all sessions.")]
        public int lifetimeTotalPoopPilesCleaned;

        // ── Factory ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns a brand-new PlayerProgressionData with safe default values
        /// for a first-time player (one stall unlocked for free, no coins, etc.).
        /// </summary>
        public static PlayerProgressionData CreateDefaultData()
        {
            return new PlayerProgressionData
            {
                currentCoinBalance                 = 0,
                lifetimeTotalCoinsEarned           = 0,
                numberOfStallsUnlocked             = 1,   // First stall is always free
                stallAutomationFlags               = new bool[GameConstants.TOTAL_NUMBER_OF_STALLS_ON_MAP],
                selectedCharacterBodyTypeIndex     = 0,
                selectedCharacterColorPaletteIndex = 0,
                selectedCharacterHatOptionIndex    = 0,
                googleAccountUserId                = string.Empty,
                googleAccountDisplayName           = string.Empty,
                lifetimeTotalPoopPilesCleaned      = 0
            };
        }
    }
}
