using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    [Header("Vertical Tilt (Optional)")]
    public bool allowVerticalTilt = true;
    public float tiltSpeed = 3f;
    public float minHeight = 3f;
    public float maxHeight = 20f;

    private CinemachineFollow followComponent;
    private bool isRotating = false;

    void Awake()
    {
        followComponent = GetComponent<CinemachineFollow>();
    }

    void Update()
    {
        if (followComponent == null) return;

        // Ignore camera rotation while UI panels pause the game
        if (Time.timeScale == 0f)
        {
            isRotating = false;
            return;
        }

        // Start rotating ONLY if right-click was pressed when NOT placing a building
        if (Input.GetMouseButtonDown(1))
        {
            bool isPlacingBuilding = BuildingPlacer.Instance != null && BuildingPlacer.Instance.pendingBuilding != null;
            if (!isPlacingBuilding)
            {
                isRotating = true;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        // Apply rotation while holding right-click
        if (isRotating && Input.GetMouseButton(1))
        {
            Vector3 offset = followComponent.FollowOffset;

            // Horizontal orbit around the player
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            offset = Quaternion.Euler(0f, mouseX, 0f) * offset;

            // Vertical pitch clamped safely above the ground
            if (allowVerticalTilt)
            {
                float mouseY = Input.GetAxis("Mouse Y") * tiltSpeed;
                offset.y = Mathf.Clamp(offset.y - mouseY, minHeight, maxHeight);
            }

            followComponent.FollowOffset = offset;
        }
    }
}