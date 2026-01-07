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
        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            lastMoveInput = Vector2.down;
        }

        public void OnMove(InputValue value)
        {
            SetMoveInput(value.Get<Vector2>());
        }

        public void OnSprint(InputValue value)
        {
            SetRunning(value.isPressed);
        }

        public void SetMoveInput(Vector2 input)
        {
            moveInput = input;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveInput = GetCardinalDirection(moveInput);
            }
        }

        public void SetRunning(bool state)
        {
            isRunning = state;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void SetLastMoveInput(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                lastMoveInput = GetCardinalDirection(direction);
            }
        }

        public Vector2 GetCardinalDirection(Vector2 input)
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
