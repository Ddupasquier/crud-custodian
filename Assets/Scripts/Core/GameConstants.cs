namespace CrudCustodian.Core
{
    /// <summary>
    /// Central location for every magic number used across the game.
    /// If you need to tune game feel, start here.
    /// </summary>
    public static class GameConstants
    {
        // ── Player movement ────────────────────────────────────────────────

        /// <summary>How many Unity units per second the player walks.</summary>
        public const float PLAYER_WALK_SPEED_UNITS_PER_SECOND = 4f;

        /// <summary>How many Unity units per second the player runs (sprint button held).</summary>
        public const float PLAYER_SPRINT_SPEED_UNITS_PER_SECOND = 7f;

        // ── Poop / cleaning ────────────────────────────────────────────────

        /// <summary>
        /// Radius (in Unity units) within which the player can automatically
        /// pick up a poop pile.
        /// </summary>
        public const float POOP_AUTO_COLLECT_RADIUS_UNITS = 0.5f;

        /// <summary>
        /// How many coins the player earns for manually cleaning one poop pile.
        /// </summary>
        public const int COINS_EARNED_PER_MANUAL_POOP_CLEAN = 5;

        /// <summary>
        /// How many coins are earned per poop pile when that stall is automated.
        /// Lower than manual because automation is passive income.
        /// </summary>
        public const int COINS_EARNED_PER_AUTOMATED_POOP_CLEAN = 2;

        // ── Poop spawning ──────────────────────────────────────────────────

        /// <summary>Minimum seconds between poop spawns at a single active stall.</summary>
        public const float POOP_SPAWN_MINIMUM_INTERVAL_SECONDS = 8f;

        /// <summary>Maximum seconds between poop spawns at a single active stall.</summary>
        public const float POOP_SPAWN_MAXIMUM_INTERVAL_SECONDS = 20f;

        // ── Stall progression ──────────────────────────────────────────────

        /// <summary>Total number of Pokemon stalls available on the map.</summary>
        public const int TOTAL_NUMBER_OF_STALLS_ON_MAP = 12;

        /// <summary>
        /// The first stall is always unlocked for free so new players have
        /// something to do immediately.
        /// </summary>
        public const int FIRST_STALL_UNLOCK_COST_IN_COINS = 0;

        /// <summary>
        /// Cost multiplier applied to each successive stall unlock.
        /// Cost[n] = BASE_STALL_COST * (STALL_COST_MULTIPLIER_PER_TIER ^ n)
        /// </summary>
        public const float STALL_UNLOCK_COST_MULTIPLIER_PER_TIER = 2.5f;

        /// <summary>The base coin cost used to calculate stall unlock prices.</summary>
        public const int STALL_UNLOCK_BASE_COST_IN_COINS = 100;

        // ── Stall automation ───────────────────────────────────────────────

        /// <summary>
        /// Base cost to automate a stall.  Multiplied by stall index so
        /// later stalls are more expensive to automate.
        /// </summary>
        public const int STALL_AUTOMATION_BASE_COST_IN_COINS = 1000;

        /// <summary>
        /// Additional cost added per stall index when automating.
        /// Automation cost = BASE + (index * COST_PER_INDEX)
        /// </summary>
        public const int STALL_AUTOMATION_ADDITIONAL_COST_PER_INDEX = 500;

        // ── Desktop window defaults (Windows & macOS Standalone) ──────────

        /// <summary>Default window width in pixels when first launched on desktop.</summary>
        public const int DESKTOP_DEFAULT_WINDOW_WIDTH_PIXELS = 1280;

        /// <summary>Default window height in pixels when first launched on desktop.</summary>
        public const int DESKTOP_DEFAULT_WINDOW_HEIGHT_PIXELS = 720;

        /// <summary>Minimum allowed window width so UI never becomes unusable.</summary>
        public const int DESKTOP_MINIMUM_WINDOW_WIDTH_PIXELS = 960;

        /// <summary>Minimum allowed window height so UI never becomes unusable.</summary>
        public const int DESKTOP_MINIMUM_WINDOW_HEIGHT_PIXELS = 540;

        // ── Authentication ─────────────────────────────────────────────────

        /// <summary>
        /// The OAuth 2.0 Web Client ID from the Google Cloud Console used
        /// for Google Sign-In.  Replace this with the real value from your
        /// Firebase / Google Cloud project before building.
        /// </summary>
        public const string GOOGLE_OAUTH_WEB_CLIENT_ID = "YOUR_GOOGLE_WEB_CLIENT_ID.apps.googleusercontent.com";

        /// <summary>
        /// Loopback redirect URI used for the desktop OAuth browser flow.
        /// Google's OAuth server redirects here after the player approves sign-in.
        /// Must match a registered URI in your Google Cloud Console credential.
        /// </summary>
        public const string GOOGLE_OAUTH_DESKTOP_REDIRECT_URI = "http://localhost:8080/";

        /// <summary>
        /// Port the local HTTP listener binds to while waiting for the desktop
        /// OAuth callback.  Must match the port in GOOGLE_OAUTH_DESKTOP_REDIRECT_URI.
        /// </summary>
        public const int GOOGLE_OAUTH_DESKTOP_CALLBACK_PORT = 8080;

        // ── Scene names ────────────────────────────────────────────────────

        /// <summary>Exact name of the Main Menu scene as it appears in Build Settings.</summary>
        public const string SCENE_NAME_MAIN_MENU = "MainMenu";

        /// <summary>Exact name of the Safari Zone gameplay scene.</summary>
        public const string SCENE_NAME_SAFARI_ZONE = "SafariZone";

        /// <summary>Exact name of the Character Customization scene.</summary>
        public const string SCENE_NAME_CHARACTER_CUSTOMIZATION = "CharacterCustomization";
    }
}
