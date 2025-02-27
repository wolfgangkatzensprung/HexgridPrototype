using UnityEngine;
using UnityEngine.Splines;

public class HexTileStreetGenerator : MonoBehaviour
{
    public SplineContainer splineContainer; // Assigned in the Inspector
    public float middleOffset = 0.5f; // Adjusts the curve shape

    void OnEnable()
    {
        GenerateStreet();
    }

    void GenerateStreet()
    {
        if (Random.value > 0.5f) // 50% chance to remove the street
        {
            splineContainer.gameObject.SetActive(false); // Disable spline object
            return;
        }

        splineContainer.gameObject.SetActive(true); // Ensure spline is active

        // Randomly select two different corner points
        int startIndex = Random.Range(0, 6);
        int endIndex = Random.Range(0, 6);
        while (endIndex == startIndex) endIndex = Random.Range(0, 6);

        // Get positions for the spline
        Vector3 start = GetEdgePosition(startIndex);
        Vector3 end = GetEdgePosition(endIndex);
        Vector3 middle = transform.position;

        // Modify spline knots
        Spline spline = splineContainer.Spline;
        spline.Clear(); // Remove existing knots
        spline.Add(new BezierKnot(start));
        spline.Add(new BezierKnot(middle));
        spline.Add(new BezierKnot(end));
    }

    Vector3 GetEdgePosition(int edge)
    {
        float size = 1.0f; // Adjust based on hex size
        Vector3[] edgePositions = new Vector3[]
        {
            new Vector3(0, 0, size),     // Top
            new Vector3(size * 0.87f, 0, size * 0.5f),  // Top-right
            new Vector3(size * 0.87f, 0, -size * 0.5f), // Bottom-right
            new Vector3(0, 0, -size),    // Bottom
            new Vector3(-size * 0.87f, 0, -size * 0.5f), // Bottom-left
            new Vector3(-size * 0.87f, 0, size * 0.5f)   // Top-left
        };

        return transform.position + edgePositions[edge];
    }
}
