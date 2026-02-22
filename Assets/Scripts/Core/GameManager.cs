using UnityEngine;

namespace CrudCustodian.Core
{
    /// <summary>
    /// Central hub that initializes all game systems and holds references to
    /// the major managers. Only one GameManager exists at a time (singleton).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Serialized references to other managers ────────────────────────
        [Header("Core Manager References")]
        [Tooltip("Handles all in-game currency earn/spend operations.")]
        [SerializeField] private CurrencyManager currencyManager;

        [Tooltip("Handles saving and loading all persistent player data.")]
        [SerializeField] private SaveDataManager saveDataManager;

        [Tooltip("Tracks the current state of the game (menu, playing, paused, etc.).")]
        [SerializeField] private GameStateController gameStateController;

        [Tooltip("Detects the runtime platform and applies platform-specific settings " +
                 "(desktop window size, input scheme, etc.).")]
        [SerializeField] private PlatformManager platformManager;

        // ── Public accessors for other scripts ─────────────────────────────
        public CurrencyManager CurrencyManager => currencyManager;
        public SaveDataManager SaveDataManager => saveDataManager;
        public GameStateController GameStateController => gameStateController;
        public PlatformManager PlatformManager => platformManager;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            EnforceSingletonPattern();
        }

        private void Start()
        {
            InitializeAllManagers();
        }

        // ── Private helpers ────────────────────────────────────────────────

        /// <summary>
        /// Ensures only one GameManager exists across scene loads.
        /// </summary>
        private void EnforceSingletonPattern()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Calls Init() on every manager in dependency order.
        /// </summary>
        private void InitializeAllManagers()
        {
            saveDataManager.Initialize();
            currencyManager.Initialize(saveDataManager.LoadedPlayerData.currentCoinBalance);
            gameStateController.Initialize();

            Debug.Log("[GameManager] All managers initialized successfully.");
        }
    }
}
