using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.01f;
    [SerializeField] private float zoomSpeed = 10f;

    private Vector3 lastMousePosition;

    void Update()
    {
        // Klik dan tahan scroll button (tombol tengah)
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        // Geser kamera selama tombol tengah ditahan
        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            Vector3 move = new Vector3(
                -delta.x,
                0f,
                -delta.y
            );

            transform.Translate(
                move * moveSpeed,
                Space.World
            );

            lastMousePosition = Input.mousePosition;
        }

        // Zoom dengan roda mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        transform.position +=
            transform.forward * scroll * zoomSpeed;
    }
}
