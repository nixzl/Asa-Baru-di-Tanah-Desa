using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "CityBuilder/Building Data")]
public class BuildingData : ScriptableObject
{
    public int workersProvided = 0;
    public int requiredTotalPopulation = 0;
    public int requiredTotalReputation = 1;
    [Header("Display Info")]
    public string buildingName;
    public string description;
    public Sprite uiIcon;

    [Header("3D Model")]
    // The actual 3D object that gets placed in the world
    public GameObject buildingPrefab; 

    [Header("Build Costs")]
    public int rockCost;
    public int woodCost;
}