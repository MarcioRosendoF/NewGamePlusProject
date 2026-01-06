using UnityEngine;
using UnityEngine.InputSystem;
using Core;

namespace Gameplay
{
    public class UIInputHandler : MonoBehaviour
    {
        public void OnToggleInventory(InputValue value)
        {
            if (value.isPressed && UIManager.Instance != null)
            {
                UIManager.Instance.ToggleView("Inventory");
            }
        }
    }
}
