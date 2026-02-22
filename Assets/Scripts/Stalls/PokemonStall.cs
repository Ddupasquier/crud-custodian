using System.Collections;
using UnityEngine;
using CrudCustodian.Core;
using CrudCustodian.Data;

namespace CrudCustodian.Stalls
{
    /// <summary>
    /// Represents a single active Pokemon stall in the Safari Zone.
    /// Spawns poop piles at random intervals while the stall is unlocked,
    /// and auto-collects them when the stall has been automated.
    /// Attach this to the Stall prefab root GameObject.
    /// </summary>
    public class PokemonStall : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Stall Configuration")]
        [Tooltip("The ScriptableObject that describes this stall's Pokemon, costs, and visuals.")]
        [SerializeField] private StallData stallConfigurationData;

        [Header("Poop Spawning")]
        [Tooltip("Prefab instantiated each time the Pokemon produces a poop pile.")]
        [SerializeField] private GameObject poopPilePrefab;

        [Tooltip("Transform used as the parent for spawned poop piles so the hierarchy stays clean.")]
        [SerializeField] private Transform poopPileSpawnParentTransform;

        [Header("Visual Components")]
        [Tooltip("The SpriteRenderer used to display the stall sign (locked / unlocked / automated).")]
        [SerializeField] private SpriteRenderer stallSignSpriteRenderer;

        [Tooltip("The SpriteRenderer used for the Pokemon's idle sprite inside the enclosure.")]
        [SerializeField] private SpriteRenderer pokemonIdleSpriteRenderer;

        [Tooltip("Visual overlay shown on top of the stall when it is still locked.")]
        [SerializeField] private GameObject lockedOverlayGameObject;

        // ── Runtime state ──────────────────────────────────────────────────
        private bool isStallCurrentlyUnlocked;
        private bool isStallCurrentlyAutomated;
        private Coroutine poopSpawnCoroutine;

        // ── Public read-only properties ────────────────────────────────────
        public StallData StallConfigurationData   => stallConfigurationData;
        public bool IsStallCurrentlyUnlocked      => isStallCurrentlyUnlocked;
        public bool IsStallCurrentlyAutomated     => isStallCurrentlyAutomated;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Start()
        {
            ApplyLockedVisualState();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Called by StallUnlockManager when the player successfully unlocks this stall.
        /// Starts the poop-spawn loop and updates visuals.
        /// </summary>
        public void UnlockThisStall()
        {
            if (isStallCurrentlyUnlocked)
            {
                Debug.LogWarning($"[PokemonStall] '{stallConfigurationData.stallDisplayName}' is already unlocked.");
                return;
            }

            isStallCurrentlyUnlocked = true;
            ApplyUnlockedVisualState();
            BeginPoopSpawnLoop();

            Debug.Log($"[PokemonStall] '{stallConfigurationData.stallDisplayName}' is now unlocked.");
        }

        /// <summary>
        /// Called by StallAutomationManager when the player pays to automate this stall.
        /// The stall will now auto-collect poop piles instead of requiring the player.
        /// </summary>
        public void AutomateThisStall()
        {
            if (!isStallCurrentlyUnlocked)
            {
                Debug.LogWarning($"[PokemonStall] Cannot automate '{stallConfigurationData.stallDisplayName}' " +
                                 "because it is not yet unlocked.");
                return;
            }

            if (isStallCurrentlyAutomated)
            {
                Debug.LogWarning($"[PokemonStall] '{stallConfigurationData.stallDisplayName}' is already automated.");
                return;
            }

            isStallCurrentlyAutomated = true;
            ApplyAutomatedVisualState();

            Debug.Log($"[PokemonStall] '{stallConfigurationData.stallDisplayName}' is now automated.");
        }

        // ── Private visual helpers ─────────────────────────────────────────

        private void ApplyLockedVisualState()
        {
            lockedOverlayGameObject.SetActive(true);
            pokemonIdleSpriteRenderer.enabled = false;

            if (stallConfigurationData.stallLockedPreviewSprite != null)
                stallSignSpriteRenderer.sprite = stallConfigurationData.stallLockedPreviewSprite;
        }

        private void ApplyUnlockedVisualState()
        {
            lockedOverlayGameObject.SetActive(false);
            pokemonIdleSpriteRenderer.enabled = true;

            if (stallConfigurationData.assignedPokemonData != null)
                pokemonIdleSpriteRenderer.sprite = stallConfigurationData.assignedPokemonData.pokemonIdleSprite;

            if (stallConfigurationData.stallUnlockedSignSprite != null)
                stallSignSpriteRenderer.sprite = stallConfigurationData.stallUnlockedSignSprite;
        }

        private void ApplyAutomatedVisualState()
        {
            if (stallConfigurationData.stallAutomatedSignSprite != null)
                stallSignSpriteRenderer.sprite = stallConfigurationData.stallAutomatedSignSprite;
        }

        // ── Poop spawn loop ────────────────────────────────────────────────

        private void BeginPoopSpawnLoop()
        {
            if (poopSpawnCoroutine != null)
                StopCoroutine(poopSpawnCoroutine);

            poopSpawnCoroutine = StartCoroutine(PoopSpawnLoopCoroutine());
        }

        private IEnumerator PoopSpawnLoopCoroutine()
        {
            while (isStallCurrentlyUnlocked)
            {
                float minimumInterval = stallConfigurationData.assignedPokemonData?.pokemonSpecificMinimumPoopIntervalSeconds > 0f
                    ? stallConfigurationData.assignedPokemonData.pokemonSpecificMinimumPoopIntervalSeconds
                    : GameConstants.POOP_SPAWN_MINIMUM_INTERVAL_SECONDS;

                float maximumInterval = stallConfigurationData.assignedPokemonData?.pokemonSpecificMaximumPoopIntervalSeconds > 0f
                    ? stallConfigurationData.assignedPokemonData.pokemonSpecificMaximumPoopIntervalSeconds
                    : GameConstants.POOP_SPAWN_MAXIMUM_INTERVAL_SECONDS;

                float secondsUntilNextPoopSpawn = Random.Range(minimumInterval, maximumInterval);
                yield return new WaitForSeconds(secondsUntilNextPoopSpawn);

                SpawnOnePoopPile();
            }
        }

        private void SpawnOnePoopPile()
        {
            if (poopPilePrefab == null)
            {
                Debug.LogWarning($"[PokemonStall] No poop pile prefab assigned to '{stallConfigurationData.stallDisplayName}'.");
                return;
            }

            // Pick a random position within the stall's spawn area.
            Vector2 randomOffsetWithinStallArea = new Vector2(
                Random.Range(-stallConfigurationData.stallPoopSpawnAreaSize.x / 2f,
                              stallConfigurationData.stallPoopSpawnAreaSize.x / 2f),
                Random.Range(-stallConfigurationData.stallPoopSpawnAreaSize.y / 2f,
                              stallConfigurationData.stallPoopSpawnAreaSize.y / 2f));

            Vector3 poopSpawnWorldPosition = transform.position + (Vector3)randomOffsetWithinStallArea;

            GameObject spawnedPoopPileObject =
                Instantiate(poopPilePrefab, poopSpawnWorldPosition, Quaternion.identity, poopPileSpawnParentTransform);

            PoopPile spawnedPoopPileComponent = spawnedPoopPileObject.GetComponent<PoopPile>();
            if (spawnedPoopPileComponent != null)
            {
                spawnedPoopPileComponent.InitializeWithPokemonData(stallConfigurationData.assignedPokemonData);

                // If automated, collect the poop immediately without player involvement.
                if (isStallCurrentlyAutomated)
                    CollectPoopPileAutomatically(spawnedPoopPileComponent);
            }
        }

        /// <summary>
        /// Automated collection earns reduced coins but requires no player action.
        /// </summary>
        private void CollectPoopPileAutomatically(PoopPile poopPileToAutoCollect)
        {
            int automatedCoinsEarned = GameConstants.COINS_EARNED_PER_AUTOMATED_POOP_CLEAN
                + (stallConfigurationData.assignedPokemonData?.bonusCoinsPerPoopClean ?? 0);

            GameManager.Instance.CurrencyManager.AddCoins(automatedCoinsEarned);
            poopPileToAutoCollect.DestroyThisPoopPile();

            Debug.Log($"[PokemonStall] Auto-collected poop at '{stallConfigurationData.stallDisplayName}'. " +
                      $"Earned {automatedCoinsEarned} coins.");
        }
    }
}
