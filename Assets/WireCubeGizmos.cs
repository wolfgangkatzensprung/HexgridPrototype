using UnityEngine;

public class WireCubeGizmos : MonoBehaviour
{
    public Color gizmoColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
