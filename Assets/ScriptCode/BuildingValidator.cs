using System.Collections.Generic;
using UnityEngine;

public class BuildingValidator : MonoBehaviour
{
    [Header("Marker Colors")]
    public Color validGreenColor = new Color(0.2f, 1f, 0.2f, 0.65f);
    public Color blockedRedColor = new Color(1f, 0.2f, 0.2f, 0.65f);

    [Header("Collision Filter")]
    public LayerMask groundLayer; // Assign your Terrain/Ground layer so the floor doesn't block placement

    private readonly HashSet<Collider> overlappingObstacles = new HashSet<Collider>();
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private readonly List<Material> previewMaterials = new List<Material>();
    private GameObject groundMarkerInstance;
    private bool isBuildPhase = false;

    public bool IsValidPlacement => overlappingObstacles.Count == 0;

    public void BeginBuildPhase(LayerMask groundMask, GameObject markerPrefab = null)
    {
        groundLayer = groundMask;
        isBuildPhase = true;
        overlappingObstacles.Clear();

        // Cache original materials so they can be cleanly restored when placed
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            originalMaterials[r] = r.sharedMaterials;

            Material[] instancedMats = r.materials;
            for (int i = 0; i < instancedMats.Length; i++)
            {
                previewMaterials.Add(instancedMats[i]);
            }
        }

        // Optional flat ground marker under the building during build phase only
        if (markerPrefab != null)
        {
            groundMarkerInstance = Instantiate(markerPrefab, transform);
            groundMarkerInstance.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            foreach (Renderer r in groundMarkerInstance.GetComponentsInChildren<Renderer>())
            {
                previewMaterials.AddRange(r.materials);
            }
        }

        UpdateMarkerVisuals();
    }

    void Update()
    {
        if (!isBuildPhase) return;

        // Clean up any destroyed objects (such as harvested trees/rocks) from the overlap list
        overlappingObstacles.RemoveWhere(col => col == null || !col.enabled || !col.gameObject.activeInHierarchy);
        UpdateMarkerVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isBuildPhase) return;
        if (IsIgnoredCollider(other)) return;

        overlappingObstacles.Add(other);
        UpdateMarkerVisuals();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isBuildPhase) return;

        if (overlappingObstacles.Remove(other))
        {
            UpdateMarkerVisuals();
        }
    }

    private bool IsIgnoredCollider(Collider other)
    {
        if (other == null || other.transform.IsChildOf(transform)) return true;

        // Ignore the ground/terrain layer
        if (((1 << other.gameObject.layer) & groundLayer) != 0) return true;

        return false;
    }

    private void UpdateMarkerVisuals()
    {
        Color targetColor = IsValidPlacement ? validGreenColor : blockedRedColor;

        foreach (Material mat in previewMaterials)
        {
            if (mat == null) continue;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", targetColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", targetColor);
        }
    }

    // Call this when finalizing placement so the placed building never reacts to the player later
    public void RestoreAndCleanup()
    {
        isBuildPhase = false;

        foreach (var entry in originalMaterials)
        {
            if (entry.Key != null)
            {
                entry.Key.sharedMaterials = entry.Value;
            }
        }

        foreach (Material mat in previewMaterials)
        {
            if (mat != null) Destroy(mat);
        }
        previewMaterials.Clear();

        if (groundMarkerInstance != null)
        {
            Destroy(groundMarkerInstance);
        }
    }
}