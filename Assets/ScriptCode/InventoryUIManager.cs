using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    public GameObject warehousePanel;
    public GameObject missionPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenWarehouse() { warehousePanel.SetActive(true); Time.timeScale = 0f; }
    public void CloseWarehouse() { warehousePanel.SetActive(false); Time.timeScale = 1f; }

    public void OpenMissions() { missionPanel.SetActive(true); Time.timeScale = 0f; }
    public void CloseMissions() { missionPanel.SetActive(false); Time.timeScale = 1f; }
}