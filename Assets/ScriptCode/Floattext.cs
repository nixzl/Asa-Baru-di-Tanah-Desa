using UnityEngine;
using TMPro;

public class Floattext : MonoBehaviour
{
    // TMP_Text works for BOTH standard TextMeshPro and TextMeshProUGUI
    private TMP_Text textMesh;

    public void Setup(string message, Color color)
    {
        textMesh = GetComponentInChildren<TMP_Text>(); 
        
        if (textMesh != null)
        {
            textMesh.text = message;
            textMesh.color = color;
        }
        else
        {
            Debug.LogError("Floattext Error: No TextMeshPro component found on this prefab or its children!");
        }

        Destroy(gameObject, 2f); // Disappear after 2 seconds
    }

    void Update()
    {
        // Float upward at 2 units per second
        transform.position += Vector3.up * 2f * Time.deltaTime; 
    }
}