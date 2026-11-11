using UnityEngine;
public class WarehouseBuilding : MonoBehaviour, Interact
{
    public void Interact()
    {
        InventoryUIManager.Instance.OpenWarehouse();
    }
}