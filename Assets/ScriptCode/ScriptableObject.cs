using UnityEngine;


public enum ItemType 
{ 
    Resource,   // Things you just hold (Rocks, Wood)
    Placeable,  // Things you build (Walls, Campfires, Workbenches)
    Tool        // Things you use (Axe, Hammer)
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType type; // Let's you select Resource, Placeable, or Tool

    [Header("Building Data (Only for Placeables)")]
    public GameObject buildPrefab; // The 3D model that spawns when you build this
}