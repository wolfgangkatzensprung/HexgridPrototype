using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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


    public void AddTile(Hex hex, Tile tile)
    {
        tilesByHex.Add(hex, tile);
    }

    #region HexGrid Utility

    public Vector3 HexToWorld(Hex hex)
    {
        float x = (layoutFlatTop.f0 * hex.Q + layoutFlatTop.f1 * hex.R) * hexSize;
        float z = (layoutFlatTop.f2 * hex.Q + layoutFlatTop.f3 * hex.R) * hexSize;

        return new Vector3(x, 0f, z);
    }

    public Vector3 FractionalHexToWorld(FractionalHex hex)
    {
        float x = (layoutFlatTop.f0 * hex.Q + layoutFlatTop.f1 * hex.R) * hexSize;
        float z = (layoutFlatTop.f2 * hex.Q + layoutFlatTop.f3 * hex.R) * hexSize;

        return new Vector3(x, 0f, z);
    }

    public Hex WorldToHex(Vector3 worldPosition)
    {
        float x = worldPosition.x / (hexSize * 3f / 2f);
        float z = worldPosition.z / (hexSize * Mathf.Sqrt(3f)); // Full axial system scaling for z

        // Adjust z position considering the offset caused by the "odd-r" or "even-r" skewing.
        float adjustedZ = z - (x / 2f);

        int q = Mathf.RoundToInt(x);
        int r = Mathf.RoundToInt(adjustedZ);
        int s = -q - r;

        return new Hex(q, r, s);
    }

    /// <summary>
    /// Returns true if there is any occupied neighbour position
    /// </summary>
    public bool HasNeighbours(Hex hex)
    {
        var neighbors = GetAllNeighbours(hex);
        foreach (var neighbor in neighbors)
        {
            if (!IsPositionOccupied(neighbor))
            {
                return true;
            }
        }
        return false; // No neighbors available
    }

    /// <summary>
    /// Get the neighbour hex positions of a given hex
    /// </summary>
    public Hex[] GetAllNeighbours(Hex hex)
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

    /// <summary>
    /// Get the occupied neighbour hex positions
    /// </summary>
    public Hex[] GetOccupiedNeighbours(Hex hex)
    {
        return GetAllNeighbours(hex).Where(IsPositionOccupied).ToArray();
    }

    public bool IsPositionOccupied(Hex hexPosition)
    {
        bool occupied = TilesByHex.ContainsKey(hexPosition);
        Debug.Log($"Tiles List check for {hexPosition}. Already exits = {occupied}");
        return occupied;
    }

    #endregion
}
