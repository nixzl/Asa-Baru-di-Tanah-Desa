using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [Header("Building")]
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private int woodCost = 30;

    [Header("Resources")]
    [SerializeField] private int woodAmount = 100;

    [Header("Placement")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask groundLayer;

    private void Update()
    {
        // Bangun fasilitas ketika pemain klik kiri mouse
        if (Input.GetMouseButtonDown(0))
        {
            TryBuild();
        }
    }

    private void TryBuild()
    {
        // Periksa apakah bahan mencukupi
        if (woodAmount < woodCost)
        {
            Debug.Log("Kayu tidak cukup untuk membangun!");
            return;
        }

        // Periksa apakah prefab sudah diatur
        if (buildingPrefab == null || playerCamera == null)
        {
            Debug.LogWarning("Building Prefab atau Camera belum diatur!");
            return;
        }

        // Arahkan ray dari kamera ke lokasi yang diklik
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            // Buat fasilitas pada lokasi yang dipilih
            Instantiate(
                buildingPrefab,
                hit.point,
                Quaternion.identity
            );

            // Kurangi bahan setelah bangunan berhasil dibuat
            woodAmount -= woodCost;

            Debug.Log("Bangunan berhasil dibuat!");
            Debug.Log("Sisa kayu: " + woodAmount);
        }
        else
        {
            Debug.Log("Pilih lokasi di atas tanah!");
        }
    }
}
