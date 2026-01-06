using UnityEngine;

namespace Gameplay
{
    public class NPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private string dialogueMessage = "Hello, traveler!";

        public void Interact()
        {
            Debug.Log($"<color=cyan>[{npcName}]:</color> {dialogueMessage}");
        }

        public string GetInteractPrompt()
        {
            return $"Talk to {npcName}";
        }
    }
}
