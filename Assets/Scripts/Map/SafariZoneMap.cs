using UnityEngine;
using UnityEngine.Tilemaps;

namespace CrudCustodian.Map
{
    /// <summary>
    /// Defines the Safari Zone's map layout at a high level.
    /// Holds references to the Tilemap layers, zone boundaries, and
    /// spawn/entry points used by other systems.
    ///
    /// Attach to the Map root GameObject in the SafariZone scene.
    /// </summary>
    public class SafariZoneMap : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Tilemap Layers")]
        [Tooltip("The base ground tilemap layer (grass, dirt paths, water edges).")]
        [SerializeField] private Tilemap groundTilemapLayer;

        [Tooltip("Decorative elements placed above the ground (trees, flowers, rocks).")]
        [SerializeField] private Tilemap decorationTilemapLayer;

        [Tooltip("Solid collision tiles that block the player (walls, fences, water).")]
        [SerializeField] private Tilemap collisionTilemapLayer;

        [Header("Spawn Points")]
        [Tooltip("The world position where the player character spawns when entering the zone.")]
        [SerializeField] private Transform playerEntrySpawnPoint;

        [Tooltip("Camera confiner boundary used to prevent the camera from leaving the map.")]
        [SerializeField] private PolygonCollider2D mapCameraConfinementBoundary;

        [Header("Zone Metadata")]
        [Tooltip("The display name of this zone shown in the UI header (e.g. 'Safari Zone – East').")]
        [SerializeField] private string zoneDisplayName = "Safari Zone";

        [Tooltip("Background music clip that plays while the player is in this zone.")]
        [SerializeField] private AudioClip zoneBackgroundMusicClip;

        // ── Public read-only properties ────────────────────────────────────
        public Transform PlayerEntrySpawnPoint          => playerEntrySpawnPoint;
        public PolygonCollider2D MapCameraConfinementBoundary => mapCameraConfinementBoundary;
        public string ZoneDisplayName                   => zoneDisplayName;
        public AudioClip ZoneBackgroundMusicClip        => zoneBackgroundMusicClip;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            ValidateRequiredReferences();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the world-space bounds of the entire passable map area,
        /// useful for clamping random spawn positions.
        /// </summary>
        public Bounds GetPassableMapWorldBounds()
        {
            return groundTilemapLayer.localBounds;
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void ValidateRequiredReferences()
        {
            if (groundTilemapLayer == null)
                Debug.LogError("[SafariZoneMap] Ground Tilemap Layer is not assigned in the Inspector.");

            if (playerEntrySpawnPoint == null)
                Debug.LogError("[SafariZoneMap] Player Entry Spawn Point is not assigned in the Inspector.");
        }
    }
}
