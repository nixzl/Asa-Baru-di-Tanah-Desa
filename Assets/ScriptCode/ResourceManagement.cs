using UnityEngine;
using TMPro; 

// Moved OUTSIDE ResourceManager so QuestManager and other scripts can access it directly
public enum ResourceCategory { Stone, Wood, Wheat }

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    public int totalWorkers = 3;
    public int availableWorkers = 3;

    [Header("Resource Caps")]
    public int maxResourceCapacity = 1000;

    [Header("Current Resources")]
    public int currentRock = 50;
    public int currentWood = 50;
    public int currentWheat = 50;

    [Header("UI Text Elements")]
    public TextMeshProUGUI rockAmountText;
    public TextMeshProUGUI wheatAmountText;
    public TextMeshProUGUI woodAmountText;
    public TextMeshProUGUI popualtionAmountText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddWheat(int amount)
    {
        currentWheat = Mathf.Min(currentWheat + amount, maxResourceCapacity);
        UpdateUI(); // Added so Wheat UI updates immediately
    }

    public bool HasResource(ResourceCategory type, int amount)
    {
        switch (type)
        {
            case ResourceCategory.Stone: return currentRock >= amount;
            case ResourceCategory.Wood:  return currentWood >= amount;
            case ResourceCategory.Wheat: return currentWheat >= amount;
            default: return false;
        }
    }

    public bool SpendSingleResource(ResourceCategory type, int amount)
    {
        if (!HasResource(type, amount)) return false;

        // Fixed: Actually deducts the delivered resource instead of returning a comparison
        switch (type)
        {
            case ResourceCategory.Stone: currentRock -= amount; break;
            case ResourceCategory.Wood:  currentWood -= amount; break;
            case ResourceCategory.Wheat: currentWheat -= amount; break;   
        }

        UpdateUI();
        return true;
    }

    public void AddPopulation(int amount)
    {
        if (amount <= 0) return;

        totalWorkers += amount;
        availableWorkers += amount;
        UpdateUI();
    }

    public bool AssignWorker()
    {
        if (availableWorkers > 0)
        {
            availableWorkers--;
            UpdateUI(); // Added so Population UI updates when workers start working
            return true;
        }
        return false;
    }

    public void UnassignWorker()
    {
        if (availableWorkers < totalWorkers)
        {
            availableWorkers++;
            UpdateUI(); // Added so Population UI updates when workers finish
        }
    }

    public void AddRock(int amount)
    {
        currentRock += amount;
        currentRock = Mathf.Min(currentRock, maxResourceCapacity);
        UpdateUI();
    }

    public void AddWood(int amount)
    {
        currentWood += amount;
        currentWood = Mathf.Min(currentWood, maxResourceCapacity);
        UpdateUI();
    }

    public bool CanAfford(int rockCost, int woodCost)
    {
        if (currentRock >= rockCost && currentWood >= woodCost)
        {
            return true;
        }
        
        Debug.Log("Bahan Tidak Cukup");
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
        if (rockAmountText != null) rockAmountText.text = currentRock.ToString() + "/" + maxResourceCapacity.ToString();
        if (woodAmountText != null) woodAmountText.text = currentWood.ToString() + "/" + maxResourceCapacity.ToString();
        if (wheatAmountText != null) wheatAmountText.text = currentWheat.ToString() + "/" + maxResourceCapacity.ToString(); // Fixed: was updating woodAmountText
        if (popualtionAmountText != null) popualtionAmountText.text = availableWorkers.ToString() + "/" + totalWorkers.ToString();
    }
}