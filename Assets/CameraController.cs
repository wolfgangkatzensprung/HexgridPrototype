using UnityEngine;
using Lean.Touch;
using System.Collections.Generic;
using System;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 0.1f;
    public float minFov = 20f;
    public float maxFov = 90f;
    public float panSpeed = 0.1f;

    private Vector3 lastMousePosition;

    void OnEnable()
    {
        LeanTouch.OnGesture += HandleTouchGesture;
    }

    void OnDisable()
    {
        LeanTouch.OnGesture -= HandleTouchGesture;
    }

    void Update()
    {
        HandleMouseInput();
    }

    void HandleMouseInput()
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

        if (Input.GetMouseButtonDown(1)) // Right mouse button pressed
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x * panSpeed, 0, -delta.y * panSpeed);
            transform.position += transform.right * move.x + Vector3.forward * move.z;
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleTouchGesture(List<LeanFinger> list)
    {
        if (LeanTouch.GuiInUse) return;

        var fingersAmount = list[0].Index == -42 ? 1 : 0; // Ignore the simulated hover finger with index -42

        Debug.Log($"Handle Touch for {list.Count} Fingers");

        if (list.Count < fingersAmount + 2) return;

        float pinchScale = LeanGesture.GetPinchScale(1f);
        if (pinchScale != 1f)
        {
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - (pinchScale - 1f) * zoomSpeed * 50f, minFov, maxFov);
        }

        HandleTouchPan(list);
    }

    void HandleTouchPan(List<LeanFinger> fingers)
    {
        if (LeanTouch.GuiInUse) return;

        if (LeanGesture.GetScreenDelta(fingers) == Vector2.zero) return;

        Vector2 delta = LeanGesture.GetScreenDelta(fingers);
        Vector3 move = new Vector3(-delta.x * panSpeed * 0.1f, 0, -delta.y * panSpeed * 0.1f);
        transform.position += transform.right * move.x + Vector3.forward * move.z;
    }

}
