using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HexGridViewer : MonoBehaviour
{
    [SerializeField] private bool showHexNumbers = true; // Toggle for hex number display

    [SerializeField] private HexGrid hexGrid;

    private void Start()
    {
        hexGrid = GetComponent<HexGrid>();
    }

    private void DrawHexagon(Vector3 center, float size)
    {
        float angleOffset = Mathf.PI / 3f;
        Vector3 lastVertex = center + new Vector3(size, 0, 0);

        for (int i = 1; i <= 6; i++)
        {
            float angle = angleOffset * i;
            Vector3 nextVertex = center + new Vector3(size * Mathf.Cos(angle), 0, size * Mathf.Sin(angle));
            Gizmos.DrawLine(lastVertex, nextVertex);
            lastVertex = nextVertex;
        }

        Gizmos.DrawLine(lastVertex, center + new Vector3(size, 0, 0));
    }

    private void OnDrawGizmos()
    {
        foreach (var hex in hexGrid.GridCells)
        {
            Vector3 worldPos = hexGrid.HexToWorld(hex);
            Gizmos.color = Color.white;
            DrawHexagon(worldPos, hexGrid.HexSize);

            if (showHexNumbers)
            {
                GUIStyle labelStyle = new GUIStyle
                {
                    fontSize = 12,
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.UpperCenter,
                    richText = true
                };

                var hexString = $"<color=green>{hex.Q}, <color=yellow>{hex.R}, <color=cyan>{hex.S}";
#if UNITY_EDITOR
                Handles.Label(worldPos, hexString, labelStyle);
#endif
            }
        }

        Gizmos.color = Color.red;
        foreach (var occupiedHex in hexGrid.OccupiedHexPositions)
        {
            Vector3 worldPos = hexGrid.HexToWorld(occupiedHex);
            DrawHexagon(worldPos, hexGrid.HexSize);
        }
    }
}

