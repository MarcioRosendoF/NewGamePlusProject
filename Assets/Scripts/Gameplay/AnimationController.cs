using UnityEngine;

namespace Gameplay
{
    public class AnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private MovementController movementController;
        [SerializeField] private bool useFlipping = false;

        [Header("Animation Speed Settings")]
        [SerializeField] private bool syncAnimationSpeed = true;
        [SerializeField] private float baseAnimationSpeed = 1f;

        private static readonly int _moveXHash = Animator.StringToHash("MoveX");
        private static readonly int _moveYHash = Animator.StringToHash("MoveY");
        private static readonly int _speedHash = Animator.StringToHash("Speed");
        private static readonly int _isRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int _isInteractingHash = Animator.StringToHash("isInteracting");

        private bool _wasRunning;

        private void Awake()
        {
            FallbackSearch();
            ValidateComponents();
        }

        private void FallbackSearch()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (movementController == null) movementController = GetComponent<MovementController>();
            if (movementController == null) movementController = GetComponentInParent<MovementController>();

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void ValidateComponents()
        {
            if (animator == null) Debug.LogError($"[AnimationController] {name}: Animator component missing! Please assign it in the Inspector.");
            if (movementController == null) Debug.LogError($"[AnimationController] {name}: MovementController mission on this object or Parent!");
        }

        private void Start()
        {
            if (movementController != null && animator != null)
            {
                var lastDir = movementController.LastMoveInput;
                UpdateAnimationState(lastDir, 0);
            }
        }

        private void Update()
        {
            if (movementController == null || animator == null) return;

            var currentSpeed = movementController.GetCurrentSpeed();
            var moveInput = movementController.MoveInput;

            UpdateAnimationState(moveInput, currentSpeed);

            if (useFlipping && spriteRenderer != null)
            {
                if (moveInput.x > 0.01f) spriteRenderer.flipX = false;
                else if (moveInput.x < -0.01f) spriteRenderer.flipX = true;
            }
        }

        private void UpdateAnimationState(Vector2 input, float speed)
        {
            animator.SetFloat(_speedHash, speed);

            var isRunning = movementController.IsRunning;
            animator.SetBool(_isRunningHash, isRunning);

            if (syncAnimationSpeed && isRunning != _wasRunning)
            {
                var targetSpeed = isRunning ? baseAnimationSpeed * movementController.RunSpeedMultiplier : baseAnimationSpeed;
                animator.speed = targetSpeed;
                _wasRunning = isRunning;
            }

            if (input.sqrMagnitude > 0.01f)
            {
                var cardinalDir = movementController.GetCardinalDirection(input);
                animator.SetFloat(_moveXHash, cardinalDir.x);
                animator.SetFloat(_moveYHash, cardinalDir.y);
            }
        }

        public void SetInteracting(bool state)
        {
            if (animator != null) animator.SetBool(_isInteractingHash, state);
        }
    }
}
