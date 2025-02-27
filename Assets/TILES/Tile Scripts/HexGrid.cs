using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Splines;

public struct Orientation
{
    public float f0, f1, f2, f3; // Transformation matrix for hex -> world
    public float b0, b1, b2, b3; // Inverse transformation matrix for world -> hex
    public float startAngle;  // in multiples of 60°

    public Orientation(float f0, float f1, float f2, float f3, float b0, float b1, float b2, float b3, float startAngle)
    {
        this.f0 = f0;
        this.f1 = f1;
        this.f2 = f2;
        this.f3 = f3;
        this.b0 = b0;
        this.b1 = b1;
        this.b2 = b2;
        this.b3 = b3;
        this.startAngle = startAngle;
    }
}

public class HexGrid : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float hexSize = 1f;

    [SerializeField] LayerMask tileLayer;

    public float HexSize => hexSize;

    private Orientation layoutFlatTop = new Orientation
        (
            f0: 3.0f / 2.0f,
            f1: 0.0f,
            f2: Mathf.Sqrt(3.0f) / 2.0f,
            f3: Mathf.Sqrt(3.0f),
            b0: Mathf.Sqrt(3.0f) / 3.0f,
            b1: -1.0f / 3.0f,
            b2: 0.0f,
            b3: 2.0f / 3.0f,
            startAngle: 0.0f
        );
    public Orientation Layout => layoutFlatTop;

    [SerializeField] private Vector3 layoutOrigin = Vector3.zero;  // Origin (center) of the grid
    public Vector3 Origin => layoutOrigin;

    private List<Hex> gridCells = new List<Hex>();

    private Dictionary<Hex, Tile> tilesByHex = new();
    public Dictionary<Hex, Tile> TilesByHex => tilesByHex;

    public List<Hex> GridCells => gridCells;

    public List<Hex> OccupiedHexPositions => new List<Hex>(tilesByHex.Keys);

    public Vector3 HexToWorld(Hex hex)
    {
        var M = layoutFlatTop;
        float x = (M.f0 * hex.Q + M.f1 * hex.R) * hexSize;
        float z = (M.f2 * hex.Q + M.f3 * hex.R) * hexSize;

        return new Vector3(x, 0f, z);
    }

    public Hex WorldToHex(Vector3 worldPos)
    {
        float q = Layout.b0 * worldPos.x + Layout.b1 * worldPos.z;
        float r = Layout.b2 * worldPos.x + Layout.b3 * worldPos.z;
        Hex roundedHex = Hex.Round(q, r);

        return roundedHex;
    }

    public Hex WorldToHex_WithDebugLogs(Vector3 worldPos)
    {
        // Log the world position
        Debug.Log($"World Position: {worldPos}");

        // Apply the inverse transformation (Hex to World)
        float q = Layout.b0 * worldPos.x + Layout.b1 * worldPos.z;
        float r = Layout.b2 * worldPos.x + Layout.b3 * worldPos.z;

        // Log intermediate results
        Debug.Log($"Intermediate q: {q}, r: {r}");

        Hex roundedHex = Hex.Round(q, r);

        Debug.Log($"Rounded Hex: Q = {roundedHex.Q}, R = {roundedHex.R}");

        return roundedHex;
    }

    public void AddTile(Hex hex, Tile tile) => tilesByHex.Add(hex, tile);

    // Method to check if the given hex has any neighbors that are not occupied
    public bool HasNeighbours(Hex hex)
    {
        var neighbors = GetNeighbours(hex);
        foreach (var neighbor in neighbors)
        {
            if (!IsPositionOccupied(neighbor))
            {
                return true;
            }
        }
        return false; // No neighbors available
    }

    // Method to get the neighbors of a given hex
    public Hex[] GetNeighbours(Hex hex)
    {
        var neighbors = new Hex[6];

        for (int i = 0; i < neighbors.Length; i++)
        {
            var dir = HexUtils.Directions[i];
            Hex neighbor = new Hex(hex.Q + dir.Q, hex.R + dir.R);
            neighbors[i] = neighbor;
        }

        return neighbors;
    }

    internal bool IsPositionOccupied(Hex hexPosition)
    {
        return TilesByHex.ContainsKey(hexPosition);
    }
}
