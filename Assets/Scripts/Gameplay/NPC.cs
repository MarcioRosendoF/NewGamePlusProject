using UnityEngine;

namespace Gameplay
{
    public class NPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField, TextArea] private string dialogueMessage = "Hello, traveler!";

        public event System.Action OnInteractionStarted;
        public event System.Action OnInteractionEnded;

        private bool _isInteracting;

        public void Interact()
        {
            if (_isInteracting)
            {
                EndInteraction();
                return;
            }

            _isInteracting = true;
            OnInteractionStarted?.Invoke();
            Debug.Log($"<color=cyan>[{npcName}]:</color> {dialogueMessage}");
        }

        private void EndInteraction()
        {
            _isInteracting = false;
            OnInteractionEnded?.Invoke();
        }

        public string GetInteractPrompt()
        {
            return _isInteracting ? "Continue..." : $"Talk to {npcName}";
        }
    }
}
