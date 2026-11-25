using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BuildMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI rockCostText;
    public TextMeshProUGUI woodCostText;
    public Button myButton;

    private BuildingData buildingData;

    public void Setup(BuildingData data)
    {
        if (data == null) return;
        buildingData = data;

        if (iconImage != null && data.uiIcon != null) iconImage.sprite = data.uiIcon;
        if (rockCostText != null) rockCostText.text = data.rockCost.ToString();
        if (woodCostText != null) woodCostText.text = data.woodCost.ToString();

        bool isUnlocked = IsBuildingUnlocked(data);

        if (myButton != null)
        {
            myButton.interactable = isUnlocked;
        }

        if (buildingNameText != null)
        {
            if (isUnlocked)
            {

            }
            else
            {
                buildingNameText.text =  " (Terkunci)";
                buildingNameText.color = Color.red;
            }
        }
    }

    public static bool IsBuildingUnlocked(BuildingData data)
    {
        int currentPop = ResourceManager.Instance != null ? ResourceManager.Instance.totalWorkers : 0;
        int currentRep = ReputationManager.Instance != null ? ReputationManager.Instance.currentLevel : 1;

        return currentPop >= data.requiredTotalPopulation &&
               currentRep >= data.requiredTotalReputation;
    }

    public void OnClickSelectBuilding()
    {
        if (buildingData != null) BuildMenuManager.Instance.SelectBuilding(buildingData);
    }

    // Pop up the info panel when hovering over either a locked or unlocked building
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildingData != null && BuildMenuManager.Instance != null)
        {
            BuildMenuManager.Instance.ShowBuildingInfo(buildingData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (BuildMenuManager.Instance != null)
        {
            BuildMenuManager.Instance.HideBuildingInfo();
        }
    }
}