using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class Interactor : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private LayerMask interactableLayer;

        private MovementController _movementController;
        private Animator _animator;
        private IInteractable _currentInteractable;

        public IInteractable CurrentInteractable => _currentInteractable;
        public event Action<IInteractable> OnInteractableChanged;

        private void Awake()
        {
            _movementController = GetComponent<MovementController>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            DetectInteractable();
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed && _currentInteractable != null)
            {
                _currentInteractable.Interact();

                if (_animator != null)
                {
                    _animator.SetTrigger("isInteracting");
                }
            }
        }

        private void DetectInteractable()
        {
            var direction = _movementController != null ? _movementController.LastMoveInput : Vector2.down;
            var hit = Physics2D.Raycast(transform.position, direction, interactRange, interactableLayer);

            IInteractable detected = null;
            if (hit.collider != null)
            {
                detected = hit.collider.GetComponent<IInteractable>();
            }

            if (detected != _currentInteractable)
            {
                _currentInteractable = detected;
                OnInteractableChanged?.Invoke(_currentInteractable);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_movementController == null) return;

            var direction = _movementController.LastMoveInput;
            var start = transform.position;
            var end = start + (Vector3)direction * interactRange;

            Gizmos.color = _currentInteractable != null ? Color.green : Color.yellow;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.2f);
        }
    }
}
