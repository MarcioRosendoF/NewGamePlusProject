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

        private Vector2 _moveInput;
        private Vector2 _lastMoveInput;
        private bool _isRunning;
        private Rigidbody2D _rb;

        public Vector2 MoveInput => _moveInput;
        public Vector2 LastMoveInput => _lastMoveInput;
        public bool IsRunning => _isRunning;
        public float MoveSpeed => moveSpeed;
        public float RunSpeedMultiplier => runSpeedMultiplier;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _lastMoveInput = Vector2.down;
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
            _moveInput = input;

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                _lastMoveInput = GetCardinalDirection(_moveInput);
            }
        }

        public void SetRunning(bool state)
        {
            _isRunning = state;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void SetLastMoveInput(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                _lastMoveInput = GetCardinalDirection(direction);
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
            var moveDir = _moveInput;
            if (moveDir.sqrMagnitude > 1f)
            {
                moveDir.Normalize();
            }

            var targetSpeed = _isRunning ? moveSpeed * runSpeedMultiplier : moveSpeed;
            _rb.linearVelocity = moveDir * targetSpeed;
        }

        public float GetCurrentSpeed() => _moveInput.magnitude;
    }
}
