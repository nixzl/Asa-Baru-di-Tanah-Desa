using UnityEngine;
using System.Collections.Generic;

public class BuildingValidator : MonoBehaviour
{
    public bool isValid = true;
    private List<Collider> obstacles = new List<Collider>();
    
    private MeshRenderer[] renderers;
    private Color[] originalColors;

    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        originalColors = new Color[renderers.Length];
        
        // Save the original colors of the building
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignore the ground/floor layer so the floor doesn't trigger the red error
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) return;

        obstacles.Add(other);
        UpdateColor();
    }

    void OnTriggerExit(Collider other)
    {
        if (obstacles.Contains(other))
        {
            obstacles.Remove(other);
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        isValid = obstacles.Count == 0;
        
        for (int i = 0; i < renderers.Length; i++)
        {
            // Tint red and semi-transparent if blocked, otherwise restore original
            renderers[i].material.color = isValid ? originalColors[i] : new Color(1f, 0f, 0f, 0.5f);
        }
    }
}