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

        private MovementController movementController;
        private Animator animator;
        private IInteractable currentInteractable;

        public IInteractable CurrentInteractable => currentInteractable;
        public event Action<IInteractable> OnInteractableChanged;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            DetectInteractable();
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed && currentInteractable != null)
            {
                currentInteractable.Interact();

                if (animator != null)
                {
                    animator.SetTrigger("isInteracting");
                }
            }
        }

        private void DetectInteractable()
        {
            var direction = movementController != null ? movementController.LastMoveInput : Vector2.down;
            var hit = Physics2D.Raycast(transform.position, direction, interactRange, interactableLayer);

            IInteractable detected = null;
            if (hit.collider != null)
            {
                detected = hit.collider.GetComponent<IInteractable>();
            }

            if (detected != currentInteractable)
            {
                currentInteractable = detected;
                OnInteractableChanged?.Invoke(currentInteractable);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (movementController == null) return;

            var direction = movementController.LastMoveInput;
            var start = transform.position;
            var end = start + (Vector3)direction * interactRange;

            Gizmos.color = currentInteractable != null ? Color.green : Color.yellow;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.2f);
        }
    }
}
