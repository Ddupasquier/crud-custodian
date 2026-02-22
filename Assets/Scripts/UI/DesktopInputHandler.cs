using UnityEngine;
using UnityEngine.InputSystem;
using CrudCustodian.Core;

namespace CrudCustodian.UI
{
    /// <summary>
    /// Handles input actions that are meaningful only on desktop platforms
    /// (Windows and macOS):
    ///
    ///   • Alt+Enter  → toggles full-screen / windowed mode
    ///   • Application quit → saves the current window resolution so it is
    ///                        restored on the next launch
    ///
    /// Attach to the GameManager root GameObject alongside PlatformManager.
    /// This component is harmless on mobile (all code paths guard with
    /// PlatformManager.IsAnyDesktopPlatform) so it is safe to leave in the
    /// scene for all build targets.
    /// </summary>
    public class DesktopInputHandler : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Input Action Asset")]
        [Tooltip("The CrudCustodianInputActions asset. The Player/ToggleFullScreen " +
                 "action in this asset is bound to Alt+Enter on desktop.")]
        [SerializeField] private InputActionAsset playerInputActionAsset;

        // ── Cached input action ────────────────────────────────────────────
        private InputAction toggleFullScreenInputAction;

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            // Only wire up keyboard shortcuts when running on a desktop OS.
            if (!PlatformManager.Instance.IsAnyDesktopPlatform) return;

            InputActionMap playerActionMap = playerInputActionAsset.FindActionMap("Player", throwIfNotFound: true);
            toggleFullScreenInputAction = playerActionMap.FindAction("ToggleFullScreen", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            if (toggleFullScreenInputAction == null) return;
            toggleFullScreenInputAction.Enable();
            toggleFullScreenInputAction.performed += HandleToggleFullScreenInputPerformed;
        }

        private void OnDisable()
        {
            if (toggleFullScreenInputAction == null) return;
            toggleFullScreenInputAction.performed -= HandleToggleFullScreenInputPerformed;
            toggleFullScreenInputAction.Disable();
        }

        private void OnApplicationQuit()
        {
            if (!PlatformManager.Instance.IsAnyDesktopPlatform) return;

            // Save the current window size so the next launch restores it.
            PlatformManager.Instance.SaveCurrentDesktopWindowResolution();
            Debug.Log("[DesktopInputHandler] Saved window resolution on quit.");
        }

        // ── Input handler ──────────────────────────────────────────────────

        private void HandleToggleFullScreenInputPerformed(InputAction.CallbackContext inputCallbackContext)
        {
            // Alt must be held for the full-screen toggle to fire (matches the
            // Windows and macOS convention of Alt+Enter).
            bool isAltKeyCurrentlyHeld = Keyboard.current != null
                && (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);

            if (!isAltKeyCurrentlyHeld) return;

            PlatformManager.Instance.ToggleFullScreenOnDesktop();
            Debug.Log("[DesktopInputHandler] Full-screen toggled via Alt+Enter.");
        }
    }
}
