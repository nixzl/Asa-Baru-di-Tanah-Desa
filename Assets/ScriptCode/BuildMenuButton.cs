using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuButton : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI costText;
    public Button myButton; // Drag the Button component here in the Inspector

    private BuildingData buildingData;

    public void Setup(BuildingData data)
    {
        if (data == null) return;
        buildingData = data;

        if (iconImage != null && data.uiIcon != null) iconImage.sprite = data.uiIcon;
        if (buildingNameText != null) buildingNameText.text = data.buildingName;
        if (costText != null) costText.text = $"R: {data.rockCost} | W: {data.woodCost}";

        // LOCK SYSTEM: Disable the button if population is too low
        if (ResourceManager.Instance.totalWorkers < data.requiredTotalPopulation)
        {
            myButton.interactable = false;
            buildingNameText.text = data.buildingName + " (Locked)";
            buildingNameText.color = Color.red;
        }
        else
        {
            myButton.interactable = true;
            buildingNameText.color = Color.white;
        }
    }

    public void OnClickSelectBuilding()
    {
        if (buildingData != null) BuildMenuManager.Instance.SelectBuilding(buildingData);
    }
}