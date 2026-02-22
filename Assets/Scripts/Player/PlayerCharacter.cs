using UnityEngine;
using UnityEngine.InputSystem;
using CrudCustodian.Core;

namespace CrudCustodian.Player
{
    /// <summary>
    /// Handles all movement and animation for the player's character sprite.
    /// Reads from Unity's new Input System and moves the Rigidbody2D each
    /// physics tick.  Attach to the Player root GameObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerCharacter : MonoBehaviour
    {
        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Movement Settings")]
        [Tooltip("Reference to the Input Action Asset that contains the Player action map.")]
        [SerializeField] private InputActionAsset playerInputActionAsset;

        [Tooltip("Multiplied against PLAYER_WALK_SPEED during sprint. " +
                 "Automatically pulls the value from GameConstants; shown here for quick reference.")]
        [SerializeField] [ReadOnly]
        private float displayedWalkSpeed = GameConstants.PLAYER_WALK_SPEED_UNITS_PER_SECOND;

        // ── Cached component references ────────────────────────────────────
        private Rigidbody2D playerRigidbody2D;
        private Animator playerAnimator;
        private SpriteRenderer playerSpriteRenderer;

        // ── Input action references ────────────────────────────────────────
        private InputAction moveInputAction;
        private InputAction sprintInputAction;

        // ── Movement state ─────────────────────────────────────────────────
        private Vector2 currentMovementInput;
        private bool isPlayerCurrentlySprinting;

        // ── Animator parameter name constants ─────────────────────────────
        private static readonly int ANIMATOR_PARAM_IS_MOVING    = Animator.StringToHash("IsMoving");
        private static readonly int ANIMATOR_PARAM_MOVE_X       = Animator.StringToHash("MoveX");
        private static readonly int ANIMATOR_PARAM_MOVE_Y       = Animator.StringToHash("MoveY");

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            playerRigidbody2D    = GetComponent<Rigidbody2D>();
            playerAnimator       = GetComponent<Animator>();
            playerSpriteRenderer = GetComponent<SpriteRenderer>();

            SetUpInputActions();
        }

        private void OnEnable()
        {
            moveInputAction.Enable();
            sprintInputAction.Enable();
        }

        private void OnDisable()
        {
            moveInputAction.Disable();
            sprintInputAction.Disable();
        }

        private void Update()
        {
            ReadCurrentInputValues();
            UpdateAnimatorParameters();
        }

        private void FixedUpdate()
        {
            ApplyMovementToRigidbody();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Prevents the player from moving (e.g. while a UI panel is open).</summary>
        public void LockPlayerMovement()
        {
            playerRigidbody2D.linearVelocity = Vector2.zero;
            moveInputAction.Disable();
        }

        /// <summary>Re-enables player movement after a lock.</summary>
        public void UnlockPlayerMovement()
        {
            moveInputAction.Enable();
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void SetUpInputActions()
        {
            InputActionMap playerActionMap = playerInputActionAsset.FindActionMap("Player", throwIfNotFound: true);
            moveInputAction   = playerActionMap.FindAction("Move",   throwIfNotFound: true);
            sprintInputAction = playerActionMap.FindAction("Sprint", throwIfNotFound: true);
        }

        private void ReadCurrentInputValues()
        {
            currentMovementInput        = moveInputAction.ReadValue<Vector2>();
            isPlayerCurrentlySprinting  = sprintInputAction.IsPressed();
        }

        private void ApplyMovementToRigidbody()
        {
            float appliedSpeed = isPlayerCurrentlySprinting
                ? GameConstants.PLAYER_SPRINT_SPEED_UNITS_PER_SECOND
                : GameConstants.PLAYER_WALK_SPEED_UNITS_PER_SECOND;

            playerRigidbody2D.linearVelocity = currentMovementInput.normalized * appliedSpeed;
        }

        private void UpdateAnimatorParameters()
        {
            bool isCurrentlyMoving = currentMovementInput.sqrMagnitude > 0.01f;
            playerAnimator.SetBool(ANIMATOR_PARAM_IS_MOVING, isCurrentlyMoving);
            playerAnimator.SetFloat(ANIMATOR_PARAM_MOVE_X,   currentMovementInput.x);
            playerAnimator.SetFloat(ANIMATOR_PARAM_MOVE_Y,   currentMovementInput.y);

            // Flip sprite to face the direction of horizontal movement.
            if (currentMovementInput.x != 0f)
            {
                playerSpriteRenderer.flipX = currentMovementInput.x < 0f;
            }
        }
    }
}
