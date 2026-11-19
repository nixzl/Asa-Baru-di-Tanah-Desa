using UnityEngine;

[CreateAssetMenu(fileName = "NewRockData", menuName = "Game/Rock Data")]
public class RockData : ScriptableObject
{
    [Header("Basic Info")]
    public string rockName = "Stone Node";
    public ResourceCategory resourceType = ResourceCategory.Stone; // Set to Wood for Trees
    public GameObject rockPrefab;

    [Header("Resource Yield")]
    public int minRockAmount = 4;
    public int maxRockAmount = 8;

    [Header("Respawn Settings")]
    public bool canRespawn = true;
    public float respawnDelay = 15f;

    [Header("Floating Text VFX")]
    public GameObject floatingTextPrefab;
    public Vector3 textOffset = new Vector3(0f, 1.5f, 0f);
}