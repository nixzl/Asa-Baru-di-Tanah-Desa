using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuButton : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI costText;
    public Button myButton; // The UI button component itself

    private BuildingData buildingData;

    public void Setup(BuildingData data)
    {
        if (data == null) return;
        buildingData = data;

        if (iconImage != null && data.uiIcon != null) iconImage.sprite = data.uiIcon;
        if (costText != null) costText.text = $"   R: {data.rockCost}    W: {data.woodCost}";

        // Check if the player has enough population
        bool isUnlocked = ResourceManager.Instance.totalWorkers >= data.requiredTotalPopulation;

        // 1. Safely lock/unlock the button
        if (myButton != null)
        {
            myButton.interactable = isUnlocked;
        }
        else
        {
            Debug.LogWarning("My Button is missing in the Inspector!", this);
        }

        // 2. Safely apply the building name and (Locked) text
        if (buildingNameText != null)
        {
            if (isUnlocked)
            {

            }
            else
            {
                buildingNameText.text =   " (Locked)";
                buildingNameText.color = Color.red;
            }
        }
    }

    public void OnClickSelectBuilding()
    {
        if (buildingData != null) BuildMenuManager.Instance.SelectBuilding(buildingData);
    }
}