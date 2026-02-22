using UnityEngine;
using CrudCustodian.Data;

namespace CrudCustodian.Stalls
{
    /// <summary>
    /// Represents a single poop pile object in the game world.
    /// Spawned by PokemonStall and collected by PlayerPoopCollector or via automation.
    /// Attach to the PoopPile prefab root.  The root must have a Collider2D set to
    /// IsTrigger = true and the tag "PoopPile".
    /// </summary>
    public class PoopPile : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Visuals")]
        [Tooltip("SpriteRenderer used to display the poop pile sprite.")]
        [SerializeField] private SpriteRenderer poopPileSpriteRenderer;

        [Tooltip("Optional particle effect played when this poop pile has been sitting " +
                 "uncollected for too long (adds visual urgency).")]
        [SerializeField] private ParticleSystem stallStinkParticleEffect;

        [Header("Decay")]
        [Tooltip("Seconds before this poop pile auto-destroys itself even without collection. " +
                 "Prevents the scene from filling up with uncollected poop. 0 = never auto-decay.")]
        [Min(0f)]
        [SerializeField] private float maximumSecondsBeforeAutoDecay = 120f;

        // ── Runtime state ──────────────────────────────────────────────────
        private PokemonData assignedPokemonData;
        private float secondsAlive;

        // ── Public read-only property ──────────────────────────────────────
        /// <summary>The Pokemon whose stall produced this poop pile.</summary>
        public PokemonData AssignedPokemonData => assignedPokemonData;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Update()
        {
            if (maximumSecondsBeforeAutoDecay <= 0f) return;

            secondsAlive += Time.deltaTime;

            if (secondsAlive >= maximumSecondsBeforeAutoDecay)
            {
                Debug.Log("[PoopPile] Poop pile decayed without being collected.");
                DestroyThisPoopPile();
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Called immediately after spawning by PokemonStall to link this pile
        /// to its source Pokemon (affects coin rewards and scale).
        /// </summary>
        public void InitializeWithPokemonData(PokemonData sourcePoopPokemonData)
        {
            assignedPokemonData = sourcePoopPokemonData;

            if (sourcePoopPokemonData != null)
            {
                // Scale the poop pile to match the Pokemon's size.
                transform.localScale = Vector3.one * sourcePoopPokemonData.poopPileSizeScaleMultiplier;

                // Start stink particles if assigned.
                if (stallStinkParticleEffect != null)
                    stallStinkParticleEffect.Play();
            }
        }

        /// <summary>Cleanly destroys this poop pile GameObject.</summary>
        public void DestroyThisPoopPile()
        {
            Destroy(gameObject);
        }
    }
}
