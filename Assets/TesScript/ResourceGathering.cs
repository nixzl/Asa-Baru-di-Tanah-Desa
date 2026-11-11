using UnityEngine;

public class ResourceGathering : MonoBehaviour
{
    public enum ResourceType
    {
        Kayu,
        Batu
    }

    [Header("Pengaturan Sumber Daya")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int resourceAmount = 10;

    private static int totalKayu = 0;
    private static int totalBatu = 0;

    // Mengumpulkan sumber daya saat objek diklik
    private void OnMouseDown()
    {
        if (resourceType == ResourceType.Kayu)
        {
            totalKayu += resourceAmount;
        }
        else
        {
            totalBatu += resourceAmount;
        }

        Debug.Log("Sumber daya berhasil dikumpulkan!");

        Debug.Log("Total Kayu: " + totalKayu);
        Debug.Log("Total Batu: " + totalBatu);

        QuestSystem quest = FindFirstObjectByType<QuestSystem>();
        
        if (quest != null)
        {
            quest.TambahKayu(1);
        }

        // Hilangkan sumber daya setelah diambil
        Destroy(gameObject);
    }
}
