using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuButton : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI buildingNameText;
    
    // NEW: Separated UI text elements for costs
    public TextMeshProUGUI rockCostText;
    public TextMeshProUGUI woodCostText;
    
    public Button myButton; 

    private BuildingData buildingData;

    public void Setup(BuildingData data)
    {
        if (data == null) return;
        buildingData = data;

        if (iconImage != null && data.uiIcon != null) iconImage.sprite = data.uiIcon;
        
        // NEW: Assign costs to their individual text objects
        if (rockCostText != null) rockCostText.text = data.rockCost.ToString();
        if (woodCostText != null) woodCostText.text = data.woodCost.ToString();

        // Check if the player has enough population
        bool isUnlocked = ResourceManager.Instance.totalWorkers >= data.requiredTotalPopulation && 
                          ReputationManager.Instance.currentLevel >= data.requiredTotalReputation;

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
                buildingNameText.text = " (Terkunci) " + data.buildingName;
                buildingNameText.color = Color.red;
            }
        }
    }

    public void OnClickSelectBuilding()
    {
        if (buildingData != null) BuildMenuManager.Instance.SelectBuilding(buildingData);
    }
}