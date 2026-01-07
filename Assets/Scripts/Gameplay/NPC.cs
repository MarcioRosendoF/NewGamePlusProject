using UnityEngine;

namespace Gameplay
{
    public class NPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField, TextArea] private string dialogueMessage = "Hello, traveler!";

        public event System.Action OnInteractionStarted;
        public event System.Action OnInteractionEnded;

        private bool isInteracting;

        public void Interact()
        {
            if (isInteracting)
            {
                EndInteraction();
                return;
            }

            isInteracting = true;
            OnInteractionStarted?.Invoke();
            Debug.Log($"<color=cyan>[{npcName}]:</color> {dialogueMessage}");
        }

        private void EndInteraction()
        {
            isInteracting = false;
            OnInteractionEnded?.Invoke();
        }

        public string GetInteractPrompt()
        {
            return isInteracting ? "Continue..." : $"Talk to {npcName}";
        }
    }
}
