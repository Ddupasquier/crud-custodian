using UnityEngine;

namespace CrudCustodian.Core
{
    /// <summary>
    /// Detects the current runtime platform and exposes helpers that other
    /// systems query instead of calling Application.platform directly.
    ///
    /// Centralising platform checks here means you only need to update one
    /// file if Unity adds a new platform or if platform detection logic changes.
    ///
    /// Attach to the GameManager root GameObject so it initialises before
    /// every other system.
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static PlatformManager Instance { get; private set; }

        // ── Cached platform flags ──────────────────────────────────────────
        // These are set once in Awake and read-only thereafter to avoid
        // repeated platform checks at runtime.

        /// <summary>True when running as a Windows desktop build.</summary>
        public bool IsWindowsDesktop { get; private set; }

        /// <summary>True when running as a macOS desktop build.</summary>
        public bool IsMacOsDesktop { get; private set; }

        /// <summary>True on either Windows or macOS desktop.</summary>
        public bool IsAnyDesktopPlatform => IsWindowsDesktop || IsMacOsDesktop;

        /// <summary>True when running on an iOS device or the iOS simulator.</summary>
        public bool IsIosMobile { get; private set; }

        /// <summary>True when running on an Android device or emulator.</summary>
        public bool IsAndroidMobile { get; private set; }

        /// <summary>True on either iOS or Android.</summary>
        public bool IsAnyMobilePlatform => IsIosMobile || IsAndroidMobile;

        /// <summary>
        /// True when running inside the Unity Editor regardless of the active
        /// build target.  Useful for editor-only debugging paths.
        /// </summary>
        public bool IsRunningInUnityEditor { get; private set; }

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            EnforceSingletonPattern();
            DetectAndCacheCurrentPlatform();
            ApplyDesktopWindowSettingsIfOnDesktop();
        }

        // ── Private helpers ────────────────────────────────────────────────

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

        private void DetectAndCacheCurrentPlatform()
        {
#if UNITY_EDITOR
            IsRunningInUnityEditor = true;
            // In the editor, mirror the currently selected build target so
            // platform-specific code paths can be tested without building.
            IsWindowsDesktop = Application.platform == RuntimePlatform.WindowsEditor
                               || Application.platform == RuntimePlatform.WindowsPlayer;
            IsMacOsDesktop   = Application.platform == RuntimePlatform.OSXEditor
                               || Application.platform == RuntimePlatform.OSXPlayer;
            IsIosMobile      = Application.platform == RuntimePlatform.IPhonePlayer;
            IsAndroidMobile  = Application.platform == RuntimePlatform.Android;
#elif UNITY_STANDALONE_WIN
            IsWindowsDesktop       = true;
            IsMacOsDesktop         = false;
            IsIosMobile            = false;
            IsAndroidMobile        = false;
            IsRunningInUnityEditor = false;
#elif UNITY_STANDALONE_OSX
            IsWindowsDesktop       = false;
            IsMacOsDesktop         = true;
            IsIosMobile            = false;
            IsAndroidMobile        = false;
            IsRunningInUnityEditor = false;
#elif UNITY_IOS
            IsWindowsDesktop       = false;
            IsMacOsDesktop         = false;
            IsIosMobile            = true;
            IsAndroidMobile        = false;
            IsRunningInUnityEditor = false;
#elif UNITY_ANDROID
            IsWindowsDesktop       = false;
            IsMacOsDesktop         = false;
            IsIosMobile            = false;
            IsAndroidMobile        = true;
            IsRunningInUnityEditor = false;
#else
            // Fallback: treat unknown platforms as desktop.
            IsWindowsDesktop       = false;
            IsMacOsDesktop         = false;
            IsIosMobile            = false;
            IsAndroidMobile        = false;
            IsRunningInUnityEditor = false;
#endif

            Debug.Log($"[PlatformManager] Platform detected: " +
                      $"Windows={IsWindowsDesktop}, macOS={IsMacOsDesktop}, " +
                      $"iOS={IsIosMobile}, Android={IsAndroidMobile}, " +
                      $"Editor={IsRunningInUnityEditor}");
        }

        /// <summary>
        /// On desktop, enforces the minimum window size and restores the last
        /// saved resolution from PlayerPrefs if one exists.
        /// </summary>
        private void ApplyDesktopWindowSettingsIfOnDesktop()
        {
            if (!IsAnyDesktopPlatform) return;

            int savedWindowWidth  = PlayerPrefs.GetInt("DesktopWindowWidth",  GameConstants.DESKTOP_DEFAULT_WINDOW_WIDTH_PIXELS);
            int savedWindowHeight = PlayerPrefs.GetInt("DesktopWindowHeight", GameConstants.DESKTOP_DEFAULT_WINDOW_HEIGHT_PIXELS);

            // Clamp to the minimum so the UI is never crushed.
            int clampedWindowWidth  = Mathf.Max(savedWindowWidth,  GameConstants.DESKTOP_MINIMUM_WINDOW_WIDTH_PIXELS);
            int clampedWindowHeight = Mathf.Max(savedWindowHeight, GameConstants.DESKTOP_MINIMUM_WINDOW_HEIGHT_PIXELS);

            Screen.SetResolution(clampedWindowWidth, clampedWindowHeight, FullScreenMode.Windowed);

            Debug.Log($"[PlatformManager] Desktop window set to {clampedWindowWidth}×{clampedWindowHeight} (windowed).");
        }

        // ── Public helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Saves the current window dimensions to PlayerPrefs so they can be
        /// restored on the next launch.  Call this from a settings/quit screen.
        /// </summary>
        public void SaveCurrentDesktopWindowResolution()
        {
            if (!IsAnyDesktopPlatform) return;

            PlayerPrefs.SetInt("DesktopWindowWidth",  Screen.width);
            PlayerPrefs.SetInt("DesktopWindowHeight", Screen.height);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Toggles between windowed and full-screen on desktop.
        /// Does nothing on mobile platforms where this toggle is not applicable.
        /// </summary>
        public void ToggleFullScreenOnDesktop()
        {
            if (!IsAnyDesktopPlatform) return;

            if (Screen.fullScreen)
            {
                // Restore to the saved (or default) windowed resolution.
                int restoredWidth  = PlayerPrefs.GetInt("DesktopWindowWidth",  GameConstants.DESKTOP_DEFAULT_WINDOW_WIDTH_PIXELS);
                int restoredHeight = PlayerPrefs.GetInt("DesktopWindowHeight", GameConstants.DESKTOP_DEFAULT_WINDOW_HEIGHT_PIXELS);
                Screen.SetResolution(restoredWidth, restoredHeight, FullScreenMode.Windowed);
            }
            else
            {
                Screen.SetResolution(
                    Display.main.systemWidth,
                    Display.main.systemHeight,
                    FullScreenMode.FullScreenWindow);
            }
        }
    }
}
