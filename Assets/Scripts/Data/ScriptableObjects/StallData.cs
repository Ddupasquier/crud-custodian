using UnityEngine;

namespace CrudCustodian.Data
{
    /// <summary>
    /// Defines the static configuration for a single Pokemon stall on the map:
    /// its position, which Pokemon occupies it, and how much it costs to unlock
    /// and automate.  Create one asset per stall via
    /// Assets → Create → CrudCustodian → StallData.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewStallData",
        menuName = "CrudCustodian/StallData",
        order = 2)]
    public class StallData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Human-readable name for this stall slot (e.g. 'Stall 01 – Bulbasaur').")]
        public string stallDisplayName;

        [Tooltip("Zero-based index of this stall in the ordered unlock sequence. " +
                 "Stall at index 0 is always free.")]
        [Min(0)]
        public int stallUnlockOrderIndex;

        [Header("Pokemon Assignment")]
        [Tooltip("The Pokemon that lives in this stall.")]
        public PokemonData assignedPokemonData;

        [Header("Map Placement")]
        [Tooltip("World-space position where this stall's GameObject should be placed on the map.")]
        public Vector2 stallWorldPosition;

        [Tooltip("Size of the stall's poop-spawn area in world units.")]
        public Vector2 stallPoopSpawnAreaSize = new Vector2(2f, 2f);

        [Header("Unlock Cost")]
        [Tooltip("Coin cost to unlock this stall. Calculated automatically from the stall index " +
                 "via StallUnlockCostCalculator but can be overridden here per stall if needed.")]
        [Min(0)]
        public int overriddenUnlockCostInCoins = -1;   // -1 means: use the calculated value

        [Header("Automation Cost")]
        [Tooltip("Coin cost for the player to automate poop-cleaning at this stall. " +
                 "-1 means: use the value calculated from the stall index.")]
        [Min(-1)]
        public int overriddenAutomationCostInCoins = -1;

        [Header("Stall Visual")]
        [Tooltip("Sprite used as the stall's locked/preview image before the player unlocks it.")]
        public Sprite stallLockedPreviewSprite;

        [Tooltip("Sprite used as the stall's sign when unlocked but not yet automated.")]
        public Sprite stallUnlockedSignSprite;

        [Tooltip("Sprite used as the stall's sign when the stall has been automated.")]
        public Sprite stallAutomatedSignSprite;
    }
}
