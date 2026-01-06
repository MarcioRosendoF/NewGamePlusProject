using UnityEngine;

namespace Gameplay
{
    public class AnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private MovementController movementController;

        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsInteractingHash = Animator.StringToHash("isInteracting");

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
                var cardinalDir = GetCardinalDirection(lastDir);

                animator.SetFloat(MoveXHash, cardinalDir.x);
                animator.SetFloat(MoveYHash, cardinalDir.y);
                animator.SetFloat(SpeedHash, 0);

                Debug.Log($"[AnimController] Start - Cardinal: ({cardinalDir.x}, {cardinalDir.y})");
            }
        }

        private void Update()
        {
            if (movementController == null || animator == null) return;

            var currentSpeed = movementController.GetCurrentSpeed();
            var isRunning = movementController.IsRunning;

            animator.SetFloat(SpeedHash, currentSpeed);
            animator.SetBool(IsRunningHash, isRunning);

            if (currentSpeed > 0.01f)
            {
                var moveInput = movementController.MoveInput;
                var cardinalDir = GetCardinalDirection(moveInput);

                animator.SetFloat(MoveXHash, cardinalDir.x);
                animator.SetFloat(MoveYHash, cardinalDir.y);
            }
        }

        public void SetInteracting(bool state)
        {
            if (animator != null) animator.SetBool(IsInteractingHash, state);
        }

        private Vector2 GetCardinalDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f) return Vector2.zero;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                return input.y > 0 ? Vector2.up : Vector2.down;
            }
        }
    }
}
