using UnityEngine;

namespace CrudCustodian.Data
{
    /// <summary>
    /// All body type, color palette, and accessory options available for
    /// player character customization.  Assign sprites in the Inspector.
    /// Create via Assets → Create → CrudCustodian → CharacterCustomizationConfig.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CharacterCustomizationConfig",
        menuName  = "CrudCustodian/CharacterCustomizationConfig",
        order     = 3)]
    public class CharacterCustomizationConfig : ScriptableObject
    {
        [Header("Body Types")]
        [Tooltip("All body-type sprite sets the player can choose from. " +
                 "Index 0 is the default.")]
        public CharacterBodyTypeOption[] availableBodyTypes;

        [Header("Color Palettes")]
        [Tooltip("Color tints applied to the character sprite. Index 0 is the default.")]
        public CharacterColorPaletteOption[] availableColorPalettes;

        [Header("Hat Options")]
        [Tooltip("Hat sprites placed on top of the character. Index 0 = no hat.")]
        public CharacterHatOption[] availableHatOptions;
    }

    // ── Supporting data types ──────────────────────────────────────────────

    [System.Serializable]
    public class CharacterBodyTypeOption
    {
        [Tooltip("Name shown in the customization UI (e.g. 'Stocky', 'Slim').")]
        public string bodyTypeDisplayName;

        [Tooltip("Idle animation sprite used when the character stands still.")]
        public Sprite characterIdleSprite;

        [Tooltip("Walk animation sprites (ordered by frame).")]
        public Sprite[] characterWalkAnimationFrames;
    }

    [System.Serializable]
    public class CharacterColorPaletteOption
    {
        [Tooltip("Name shown in the customization UI (e.g. 'Classic Blue', 'Sunset Red').")]
        public string colorPaletteDisplayName;

        [Tooltip("The primary tint color applied to the character's outfit.")]
        public Color primaryOutfitColor = Color.blue;

        [Tooltip("The secondary tint color used for accessories/details.")]
        public Color secondaryOutfitColor = Color.white;
    }

    [System.Serializable]
    public class CharacterHatOption
    {
        [Tooltip("Name shown in the customization UI.")]
        public string hatDisplayName;

        [Tooltip("Hat sprite placed on the character's head. Leave null for no hat.")]
        public Sprite hatSprite;
    }
}
