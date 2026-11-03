using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuButton : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI costText;

    private BuildingData buildingData;

    public void Setup(BuildingData data)
    {
        buildingData = data;
        if (iconImage != null && data.uiIcon != null) iconImage.sprite = data.uiIcon;
        if (buildingNameText != null) buildingNameText.text = data.buildingName;
        if (costText != null) costText.text = $"R: {data.rockCost} | W: {data.woodCost}";
    }

    public void OnClickSelectBuilding()
    {
        BuildMenuManager.Instance.SelectBuilding(buildingData);
    }
}