using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runSpeedMultiplier = 1.5f;

        private Vector2 moveInput;
        private Vector2 lastMoveInput;
        private bool isRunning;
        private Rigidbody2D rb;

        public Vector2 MoveInput => moveInput;
        public Vector2 LastMoveInput => lastMoveInput;
        public bool IsRunning => isRunning;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            lastMoveInput = Vector2.down;
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveInput = moveInput.normalized;
            }
        }

        public void OnSprint(InputValue value)
        {
            isRunning = value.isPressed;
        }

        private void FixedUpdate()
        {
            var moveDir = moveInput;
            if (moveDir.sqrMagnitude > 1f)
            {
                moveDir.Normalize();
            }

            var targetSpeed = isRunning ? moveSpeed * runSpeedMultiplier : moveSpeed;
            rb.linearVelocity = moveDir * targetSpeed;
        }

        public float GetCurrentSpeed() => moveInput.magnitude;
    }
}
