using UnityEngine;

public class BuildButton : MonoBehaviour
{
    // Drag your specific BuildingData (e.g., "BarracksData") in here
    public BuildingData buildingToBuild;

    // Link this to the button's OnClick() event in the Inspector
    public void OnClickSelectBuilding()
    {
        BuildingPlacer.Instance.StartPlacement(buildingToBuild);
    }
}