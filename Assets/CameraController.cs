using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 0.1f;
    public float minFov = 20f;
    public float maxFov = 90f;
    public float panSpeed = 0.1f;

    private Vector3 lastMousePosition;

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        if (Input.GetMouseButtonDown(2)) // Middle mouse button pressed
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2)) // Middle mouse button held
        {
            float deltaY = Input.mousePosition.y - lastMousePosition.y;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - deltaY * zoomSpeed, minFov, maxFov);
            lastMousePosition = Input.mousePosition;
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click pressed
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1)) // Right-click held
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x * panSpeed, 0, -delta.y * panSpeed);

            transform.position += transform.right * move.x + Vector3.forward * move.z;
            lastMousePosition = Input.mousePosition;
        }
    }


}
