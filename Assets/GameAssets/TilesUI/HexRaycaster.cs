using Lean.Touch;
using UnityEngine;

public class HexRaycaster : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    Plane plane = new Plane(Vector3.down, 0f);

    private Vector3 screenPosition;
    public Vector3 ScreenPosition => screenPosition;

    private Vector3 worldPosition;
    public Vector3 WorldPosition => worldPosition;
    private Hex hexPosition;
    public Hex HexPosition => hexPosition;

    private Ray ray;

    private void Update()
    {
        ray = CalculateRay();
        if (plane.Raycast(ray, out float distance))
        {
            worldPosition = ray.GetPoint(distance);
            hexPosition = hexGrid.WorldToHex(worldPosition);
        }
    }

    private Ray CalculateRay()
    {
#if UNITY_ANDROID
        if (LeanTouch.GuiInUse) return this.ray;

        screenPosition = LeanTouch.Fingers.Count > 0 ?
            LeanTouch.Fingers[0].LastScreenPosition :
            screenPosition;
#else
        screenPosition = Input.mousePosition;
#endif
        screenPosition.z = Camera.main.nearClipPlane + 1;  // get in front of camera
        var ray = Camera.main.ScreenPointToRay(screenPosition);
        return ray;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            var ray = CalculateRay();

            if (plane.Raycast(ray, out float distance))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(ray);
                Gizmos.DrawWireSphere(ray.origin + ray.direction * distance, .5f);
            }
        }
    }
}
