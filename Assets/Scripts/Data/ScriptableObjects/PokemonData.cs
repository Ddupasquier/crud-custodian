using UnityEngine;

namespace CrudCustodian.Data
{
    /// <summary>
    /// Holds all static data that describes a single Pokemon species used in
    /// the Safari Zone stalls.  Create one asset per Pokemon via
    /// Assets → Create → CrudCustodian → PokemonData.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewPokemonData",
        menuName = "CrudCustodian/PokemonData",
        order = 1)]
    public class PokemonData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("The Pokemon's official name displayed in the UI (e.g. 'Bulbasaur').")]
        public string pokemonDisplayName;

        [Tooltip("National Pokedex number (001 = Bulbasaur). Used for sorting and lookup.")]
        public int nationalPokedexNumber;

        [Header("Visuals")]
        [Tooltip("Sprite shown on the stall sign and in the UI.")]
        public Sprite pokemonPortraitSprite;

        [Tooltip("The animated sprite used inside the stall enclosure.")]
        public Sprite pokemonIdleSprite;

        [Tooltip("Tint color applied to the stall background to match the Pokemon's type.")]
        public Color stallBackgroundTintColor = Color.white;

        [Header("Poop Behaviour")]
        [Tooltip("Minimum seconds between poop spawns for this specific Pokemon. " +
                 "Overrides the global minimum if set above zero.")]
        [Min(0f)]
        public float pokemonSpecificMinimumPoopIntervalSeconds = 0f;

        [Tooltip("Maximum seconds between poop spawns for this specific Pokemon. " +
                 "Overrides the global maximum if set above zero.")]
        [Min(0f)]
        public float pokemonSpecificMaximumPoopIntervalSeconds = 0f;

        [Tooltip("Scale of the poop pile sprite spawned by this Pokemon. " +
                 "Larger Pokemon produce larger poop.")]
        [Min(0.1f)]
        public float poopPileSizeScaleMultiplier = 1f;

        [Header("Coin Reward")]
        [Tooltip("Bonus coins added on top of the base reward when cleaning this Pokemon's poop. " +
                 "Rarer / more desirable Pokemon can give extra incentive.")]
        [Min(0)]
        public int bonusCoinsPerPoopClean = 0;

        [Header("Unlock Info")]
        [Tooltip("Flavour text shown in the unlock screen for this Pokemon's stall.")]
        [TextArea(2, 4)]
        public string pokemonStallFlavorDescription;
    }
}
