using UnityEngine;
using TMPro; 

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Resource Caps")]
    public int maxResourceCapacity = 1000;

    [Header("Current Resources")]
    public int currentRock = 50;
    public int currentWood = 50;

    [Header("UI Text Elements")]
    public TextMeshProUGUI rockAmountText;
    public TextMeshProUGUI woodAmountText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddRock(int amount)
    {
        currentRock += amount;
        // Forces currentRock to stay at or below maxResourceCapacity
        currentRock = Mathf.Min(currentRock, maxResourceCapacity);
        UpdateUI();
    }

    public void AddWood(int amount)
    {
        currentWood += amount;
        // Forces currentWood to stay at or below maxResourceCapacity
        currentWood = Mathf.Min(currentWood, maxResourceCapacity);
        UpdateUI();
    }

    public bool CanAfford(int rockCost, int woodCost)
    {
        if (currentRock >= rockCost && currentWood >= woodCost)
        {
            return true;
        }
        
        Debug.Log("Not enough resources to build this!");
        return false;
    }

    public void SpendResources(int rockCost, int woodCost)
    {
        currentRock -= rockCost;
        currentWood -= woodCost;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Updates the UI to show the format "Current / Max" (e.g., "1000 / 1000")
        if (rockAmountText != null) rockAmountText.text = currentRock.ToString() + "/" + maxResourceCapacity.ToString();
        if (woodAmountText != null) woodAmountText.text = currentWood.ToString() + "/" + maxResourceCapacity.ToString();
    }
}