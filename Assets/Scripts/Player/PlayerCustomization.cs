using UnityEngine;
using CrudCustodian.Core;
using CrudCustodian.Data;

namespace CrudCustodian.Player
{
    /// <summary>
    /// Applies the saved character customization (body type, color palette, hat)
    /// to the player's SpriteRenderer and child hat sprite.
    /// Call ApplyCurrentCustomizationFromSaveData() after loading to restore
    /// the player's previous look.
    /// </summary>
    public class PlayerCustomization : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Config Reference")]
        [Tooltip("The ScriptableObject that lists all body types, palettes, and hats.")]
        [SerializeField] private CharacterCustomizationConfig characterCustomizationConfig;

        [Header("Component References")]
        [Tooltip("The SpriteRenderer on the player root used to display the body sprite.")]
        [SerializeField] private SpriteRenderer playerBodySpriteRenderer;

        [Tooltip("A child SpriteRenderer positioned over the player's head for hats.")]
        [SerializeField] private SpriteRenderer playerHatSpriteRenderer;

        // ── Internal state ─────────────────────────────────────────────────
        private int activeBodyTypeIndex;
        private int activeColorPaletteIndex;
        private int activeHatOptionIndex;

        // ── Public read-only accessors ─────────────────────────────────────
        public int ActiveBodyTypeIndex      => activeBodyTypeIndex;
        public int ActiveColorPaletteIndex  => activeColorPaletteIndex;
        public int ActiveHatOptionIndex     => activeHatOptionIndex;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Start()
        {
            ApplyCurrentCustomizationFromSaveData();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Reads the saved indices and immediately updates all visuals to match.
        /// Call this once at startup or after the player loads into a scene.
        /// </summary>
        public void ApplyCurrentCustomizationFromSaveData()
        {
            PlayerProgressionData savedData = GameManager.Instance.SaveDataManager.LoadedPlayerData;

            SetBodyType(savedData.selectedCharacterBodyTypeIndex);
            SetColorPalette(savedData.selectedCharacterColorPaletteIndex);
            SetHat(savedData.selectedCharacterHatOptionIndex);
        }

        /// <summary>
        /// Switches the player's body type to the given index and saves the choice.
        /// </summary>
        public void SetBodyType(int bodyTypeIndex)
        {
            if (!IsBodyTypeIndexValid(bodyTypeIndex)) return;

            activeBodyTypeIndex = bodyTypeIndex;
            CharacterBodyTypeOption selectedBodyType = characterCustomizationConfig.availableBodyTypes[bodyTypeIndex];
            playerBodySpriteRenderer.sprite = selectedBodyType.characterIdleSprite;

            GameManager.Instance.SaveDataManager.SaveSelectedCharacterBodyTypeIndex(bodyTypeIndex);
            Debug.Log($"[PlayerCustomization] Body type set to index {bodyTypeIndex}: {selectedBodyType.bodyTypeDisplayName}");
        }

        /// <summary>
        /// Applies the color palette at the given index to the player sprite tint.
        /// </summary>
        public void SetColorPalette(int colorPaletteIndex)
        {
            if (!IsColorPaletteIndexValid(colorPaletteIndex)) return;

            activeColorPaletteIndex = colorPaletteIndex;
            CharacterColorPaletteOption selectedPalette =
                characterCustomizationConfig.availableColorPalettes[colorPaletteIndex];

            playerBodySpriteRenderer.color = selectedPalette.primaryOutfitColor;

            GameManager.Instance.SaveDataManager.SaveSelectedCharacterColorPaletteIndex(colorPaletteIndex);
            Debug.Log($"[PlayerCustomization] Color palette set to index {colorPaletteIndex}: {selectedPalette.colorPaletteDisplayName}");
        }

        /// <summary>
        /// Puts the hat at the given index on the player's head.
        /// Index 0 means no hat.
        /// </summary>
        public void SetHat(int hatOptionIndex)
        {
            if (!IsHatOptionIndexValid(hatOptionIndex)) return;

            activeHatOptionIndex = hatOptionIndex;
            CharacterHatOption selectedHat = characterCustomizationConfig.availableHatOptions[hatOptionIndex];
            playerHatSpriteRenderer.sprite  = selectedHat.hatSprite;
            playerHatSpriteRenderer.enabled = selectedHat.hatSprite != null;

            Debug.Log($"[PlayerCustomization] Hat set to index {hatOptionIndex}: {selectedHat.hatDisplayName}");
        }

        // ── Validation helpers ─────────────────────────────────────────────

        private bool IsBodyTypeIndexValid(int index)
        {
            bool isValid = index >= 0 && index < characterCustomizationConfig.availableBodyTypes.Length;
            if (!isValid)
                Debug.LogWarning($"[PlayerCustomization] Invalid body type index: {index}");
            return isValid;
        }

        private bool IsColorPaletteIndexValid(int index)
        {
            bool isValid = index >= 0 && index < characterCustomizationConfig.availableColorPalettes.Length;
            if (!isValid)
                Debug.LogWarning($"[PlayerCustomization] Invalid color palette index: {index}");
            return isValid;
        }

        private bool IsHatOptionIndexValid(int index)
        {
            bool isValid = index >= 0 && index < characterCustomizationConfig.availableHatOptions.Length;
            if (!isValid)
                Debug.LogWarning($"[PlayerCustomization] Invalid hat option index: {index}");
            return isValid;
        }
    }
}
