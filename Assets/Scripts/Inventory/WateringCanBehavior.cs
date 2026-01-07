using UnityEngine;
using Inventory;
using Gameplay;

public class WateringCanBehavior : ItemBehavior
{
    public override void OnUse(GameObject user)
    {
        var interactor = user.GetComponent<Interactor>();
        if (interactor == null || interactor.CurrentInteractable == null)
            return;

        if (interactor.CurrentInteractable is HarvestableResource harvestable)
        {
            harvestable.InstantRegenerate();
        }
    }
}
