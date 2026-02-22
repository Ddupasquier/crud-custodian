using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrudCustodian.Core;
using CrudCustodian.Data;

namespace CrudCustodian.UI
{
    /// <summary>
    /// Lets the player preview and select their character's body type, color
    /// palette, and hat.  The preview sprite updates live as the player cycles
    /// through options.
    ///
    /// Works identically on desktop (mouse/keyboard navigation) and mobile
    /// (touch/tap). All navigation uses standard Unity UI buttons so the
    /// Input System handles both automatically.
    /// </summary>
    public class CharacterCustomizationUI : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Live Preview")]
        [Tooltip("Displays the character with the currently selected options applied.")]
        [SerializeField] private Image characterPreviewImage;

        [Header("Body Type Controls")]
        [SerializeField] private Button previousBodyTypeButton;
        [SerializeField] private Button nextBodyTypeButton;
        [SerializeField] private TextMeshProUGUI currentBodyTypeNameLabel;

        [Header("Color Palette Controls")]
        [SerializeField] private Button previousColorPaletteButton;
        [SerializeField] private Button nextColorPaletteButton;
        [SerializeField] private TextMeshProUGUI currentColorPaletteNameLabel;

        [Header("Hat Controls")]
        [SerializeField] private Button previousHatOptionButton;
        [SerializeField] private Button nextHatOptionButton;
        [SerializeField] private TextMeshProUGUI currentHatOptionNameLabel;

        [Header("Action Buttons")]
        [Tooltip("Saves the current selection and returns to the Main Menu.")]
        [SerializeField] private Button confirmAndSaveButton;

        [Tooltip("Discards changes and returns to the Main Menu.")]
        [SerializeField] private Button cancelAndDiscardButton;

        [Header("Config Reference")]
        [SerializeField] private CharacterCustomizationConfig characterCustomizationConfig;

        // ── Private state ──────────────────────────────────────────────────
        private int previewBodyTypeIndex;
        private int previewColorPaletteIndex;
        private int previewHatOptionIndex;

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Start()
        {
            LoadCurrentSelectionsFromSaveData();
            RefreshAllPreviewLabelsAndImages();
            SubscribeToAllButtonClickEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromAllButtonClickEvents();
        }

        // ── Setup helpers ──────────────────────────────────────────────────

        private void LoadCurrentSelectionsFromSaveData()
        {
            PlayerProgressionData savedData = GameManager.Instance.SaveDataManager.LoadedPlayerData;
            previewBodyTypeIndex     = savedData.selectedCharacterBodyTypeIndex;
            previewColorPaletteIndex = savedData.selectedCharacterColorPaletteIndex;
            previewHatOptionIndex    = savedData.selectedCharacterHatOptionIndex;
        }

        private void SubscribeToAllButtonClickEvents()
        {
            previousBodyTypeButton?.onClick.AddListener(HandlePreviousBodyTypeButtonClicked);
            nextBodyTypeButton?.onClick.AddListener(HandleNextBodyTypeButtonClicked);
            previousColorPaletteButton?.onClick.AddListener(HandlePreviousColorPaletteButtonClicked);
            nextColorPaletteButton?.onClick.AddListener(HandleNextColorPaletteButtonClicked);
            previousHatOptionButton?.onClick.AddListener(HandlePreviousHatButtonClicked);
            nextHatOptionButton?.onClick.AddListener(HandleNextHatButtonClicked);
            confirmAndSaveButton?.onClick.AddListener(HandleConfirmAndSaveButtonClicked);
            cancelAndDiscardButton?.onClick.AddListener(HandleCancelAndDiscardButtonClicked);
        }

        private void UnsubscribeFromAllButtonClickEvents()
        {
            previousBodyTypeButton?.onClick.RemoveListener(HandlePreviousBodyTypeButtonClicked);
            nextBodyTypeButton?.onClick.RemoveListener(HandleNextBodyTypeButtonClicked);
            previousColorPaletteButton?.onClick.RemoveListener(HandlePreviousColorPaletteButtonClicked);
            nextColorPaletteButton?.onClick.RemoveListener(HandleNextColorPaletteButtonClicked);
            previousHatOptionButton?.onClick.RemoveListener(HandlePreviousHatButtonClicked);
            nextHatOptionButton?.onClick.RemoveListener(HandleNextHatButtonClicked);
            confirmAndSaveButton?.onClick.RemoveListener(HandleConfirmAndSaveButtonClicked);
            cancelAndDiscardButton?.onClick.RemoveListener(HandleCancelAndDiscardButtonClicked);
        }

        // ── Button click handlers ──────────────────────────────────────────

        private void HandlePreviousBodyTypeButtonClicked()
        {
            previewBodyTypeIndex = WrapIndexBackward(previewBodyTypeIndex, characterCustomizationConfig.availableBodyTypes.Length);
            RefreshAllPreviewLabelsAndImages();
        }

        private void HandleNextBodyTypeButtonClicked()
        {
            previewBodyTypeIndex = WrapIndexForward(previewBodyTypeIndex, characterCustomizationConfig.availableBodyTypes.Length);
            RefreshAllPreviewLabelsAndImages();
        }

        private void HandlePreviousColorPaletteButtonClicked()
        {
            previewColorPaletteIndex = WrapIndexBackward(previewColorPaletteIndex, characterCustomizationConfig.availableColorPalettes.Length);
            RefreshAllPreviewLabelsAndImages();
        }

        private void HandleNextColorPaletteButtonClicked()
        {
            previewColorPaletteIndex = WrapIndexForward(previewColorPaletteIndex, characterCustomizationConfig.availableColorPalettes.Length);
            RefreshAllPreviewLabelsAndImages();
        }

        private void HandlePreviousHatButtonClicked()
        {
            previewHatOptionIndex = WrapIndexBackward(previewHatOptionIndex, characterCustomizationConfig.availableHatOptions.Length);
            RefreshAllPreviewLabelsAndImages();
        }

        private void HandleNextHatButtonClicked()
        {
            previewHatOptionIndex = WrapIndexForward(previewHatOptionIndex, characterCustomizationConfig.availableHatOptions.Length);
            RefreshAllPreviewLabelsAndImages();
        }

        private void HandleConfirmAndSaveButtonClicked()
        {
            SaveCurrentPreviewSelectionsToSaveData();
            ReturnToMainMenu();
        }

        private void HandleCancelAndDiscardButtonClicked()
        {
            ReturnToMainMenu();
        }

        // ── Preview refresh ────────────────────────────────────────────────

        private void RefreshAllPreviewLabelsAndImages()
        {
            // Body type label
            if (currentBodyTypeNameLabel != null && characterCustomizationConfig.availableBodyTypes.Length > 0)
                currentBodyTypeNameLabel.text = characterCustomizationConfig.availableBodyTypes[previewBodyTypeIndex].bodyTypeDisplayName;

            // Color palette label
            if (currentColorPaletteNameLabel != null && characterCustomizationConfig.availableColorPalettes.Length > 0)
                currentColorPaletteNameLabel.text = characterCustomizationConfig.availableColorPalettes[previewColorPaletteIndex].colorPaletteDisplayName;

            // Hat label
            if (currentHatOptionNameLabel != null && characterCustomizationConfig.availableHatOptions.Length > 0)
                currentHatOptionNameLabel.text = characterCustomizationConfig.availableHatOptions[previewHatOptionIndex].hatDisplayName;

            // Preview sprite and tint
            if (characterPreviewImage != null && characterCustomizationConfig.availableBodyTypes.Length > 0)
            {
                characterPreviewImage.sprite = characterCustomizationConfig.availableBodyTypes[previewBodyTypeIndex].characterIdleSprite;

                if (characterCustomizationConfig.availableColorPalettes.Length > 0)
                    characterPreviewImage.color = characterCustomizationConfig.availableColorPalettes[previewColorPaletteIndex].primaryOutfitColor;
            }
        }

        // ── Save helper ────────────────────────────────────────────────────

        private void SaveCurrentPreviewSelectionsToSaveData()
        {
            PlayerProgressionData dataToUpdate = GameManager.Instance.SaveDataManager.LoadedPlayerData;
            dataToUpdate.selectedCharacterBodyTypeIndex     = previewBodyTypeIndex;
            dataToUpdate.selectedCharacterColorPaletteIndex = previewColorPaletteIndex;
            dataToUpdate.selectedCharacterHatOptionIndex    = previewHatOptionIndex;
            GameManager.Instance.SaveDataManager.SaveAllPlayerData(dataToUpdate);
        }

        private void ReturnToMainMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SCENE_NAME_MAIN_MENU);
        }

        // ── Index wrapping helpers ─────────────────────────────────────────

        private static int WrapIndexForward(int currentIndex, int totalCount)
            => (currentIndex + 1) % totalCount;

        private static int WrapIndexBackward(int currentIndex, int totalCount)
            => (currentIndex - 1 + totalCount) % totalCount;
    }
}
