using UnityEngine;

namespace Gameplay
{
    public class AnimationController : MonoBehaviour
    {
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private MovementController movementController;

        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsInteractingHash = Animator.StringToHash("isInteracting");

        private void Awake()
        {
            animator = GetComponent<Animator>();

            movementController = GetComponent<MovementController>();
            if (movementController == null) movementController = GetComponentInParent<MovementController>();

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (animator == null) Debug.LogError($"{name}: Animator component missing!");
            if (movementController == null) Debug.LogError($"{name}: MovementController component missing on this object or Parent!");
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

                Debug.Log($"[AnimController] Input: ({moveInput.x:F2}, {moveInput.y:F2}) → Cardinal: ({cardinalDir.x}, {cardinalDir.y}) | Speed: {currentSpeed:F2}");
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
