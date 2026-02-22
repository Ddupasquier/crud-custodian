using System;
using UnityEngine;

namespace CrudCustodian.Core
{
    /// <summary>
    /// Tracks which high-level state the game is currently in and broadcasts
    /// transitions so UI and gameplay systems can react without polling.
    /// </summary>
    public class GameStateController : MonoBehaviour
    {
        // ── Enum ───────────────────────────────────────────────────────────
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            CharacterCustomization,
            StallUnlockScreen,
            LoadingScreen
        }

        // ── Events ─────────────────────────────────────────────────────────
        /// <summary>Fires whenever the game transitions to a new state.</summary>
        public event Action<GameState> OnGameStateChanged;

        // ── Internal state ─────────────────────────────────────────────────
        private GameState currentGameState;

        // ── Public read-only property ──────────────────────────────────────
        public GameState CurrentGameState => currentGameState;

        // ── Initialization ─────────────────────────────────────────────────
        public void Initialize()
        {
            TransitionToGameState(GameState.MainMenu);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Moves the game into the requested state and notifies all listeners.
        /// </summary>
        public void TransitionToGameState(GameState newGameState)
        {
            if (currentGameState == newGameState) return;

            Debug.Log($"[GameStateController] Transitioning: {currentGameState} → {newGameState}");
            currentGameState = newGameState;
            OnGameStateChanged?.Invoke(currentGameState);

            HandleGameStateEntryBehavior(currentGameState);
        }

        // ── Private helpers ────────────────────────────────────────────────

        /// <summary>
        /// Performs any immediate side-effects required when entering a state
        /// (e.g. pausing physics when the game is paused).
        /// </summary>
        private void HandleGameStateEntryBehavior(GameState enteredState)
        {
            switch (enteredState)
            {
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;

                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;

                default:
                    // Other states do not alter time scale.
                    break;
            }
        }
    }
}
