using UnityEngine;

namespace Inventory
{
    public abstract class ItemBehavior : MonoBehaviour
    {
        public abstract void OnUse(GameObject user);
    }
}
