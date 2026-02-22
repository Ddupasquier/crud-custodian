using System;
using UnityEngine;
using CrudCustodian.Core;

namespace CrudCustodian.Player
{
    /// <summary>
    /// Detects when the player walks over a poop pile and collects it,
    /// awarding coins and playing the collection effect.
    /// Attach to the Player GameObject alongside PlayerCharacter.
    /// </summary>
    public class PlayerPoopCollector : MonoBehaviour
    {
        // ── Events ─────────────────────────────────────────────────────────
        /// <summary>Fired when a poop pile is collected. Passes the coins earned.</summary>
        public event Action<int> OnPoopPileCollected;

        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Collection Settings")]
        [Tooltip("Particle system played at the poop pile's world position when collected.")]
        [SerializeField] private ParticleSystem poopCollectionParticleEffect;

        [Tooltip("Sound clip played when a poop pile is collected.")]
        [SerializeField] private AudioClip poopCollectionSoundClip;

        // ── Cached component references ────────────────────────────────────
        private AudioSource playerAudioSource;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            playerAudioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            // The poop pile prefab must have the tag "PoopPile" for this to work.
            if (!otherCollider.CompareTag("PoopPile")) return;

            PoopPile poopPileComponent = otherCollider.GetComponent<PoopPile>();
            if (poopPileComponent == null) return;

            CollectPoopPile(poopPileComponent);
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void CollectPoopPile(PoopPile poopPileToCollect)
        {
            int totalCoinsEarned = CalculateCoinRewardForPoopPile(poopPileToCollect);

            // Award coins via the central manager.
            GameManager.Instance.CurrencyManager.AddCoins(totalCoinsEarned);

            // Update lifetime stat.
            GameManager.Instance.SaveDataManager.LoadedPlayerData.lifetimeTotalPoopPilesCleaned++;
            GameManager.Instance.SaveDataManager.SaveAllPlayerData(
                GameManager.Instance.SaveDataManager.LoadedPlayerData);

            PlayCollectionVisualAndAudioEffects(poopPileToCollect.transform.position);
            OnPoopPileCollected?.Invoke(totalCoinsEarned);

            poopPileToCollect.DestroyThisPoopPile();

            Debug.Log($"[PlayerPoopCollector] Collected poop pile. Earned {totalCoinsEarned} coins.");
        }

        private int CalculateCoinRewardForPoopPile(PoopPile poopPile)
        {
            int baseReward  = GameConstants.COINS_EARNED_PER_MANUAL_POOP_CLEAN;
            int bonusReward = poopPile.AssignedPokemonData != null
                ? poopPile.AssignedPokemonData.bonusCoinsPerPoopClean
                : 0;

            return baseReward + bonusReward;
        }

        private void PlayCollectionVisualAndAudioEffects(Vector3 effectWorldPosition)
        {
            if (poopCollectionParticleEffect != null)
            {
                poopCollectionParticleEffect.transform.position = effectWorldPosition;
                poopCollectionParticleEffect.Play();
            }

            if (poopCollectionSoundClip != null && playerAudioSource != null)
            {
                playerAudioSource.PlayOneShot(poopCollectionSoundClip);
            }
        }
    }
}
