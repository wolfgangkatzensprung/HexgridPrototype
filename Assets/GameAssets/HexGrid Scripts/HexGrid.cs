using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using System.Collections;

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

public class HexGrid : SerializedMonoBehaviour
{
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

    private Dictionary<Hex, Tile> tilesByHex = new();
    public Dictionary<Hex, Tile> Board => tilesByHex;

    public List<Hex> OccupiedHexPositions => new List<Hex>(tilesByHex.Keys);

    public void AddTile(Hex hex, Tile tile)
    {
        tilesByHex.Add(hex, tile);
    }

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
        var neighbors = HexUtils.GetAllNeighbours(hex);
        foreach (var neighbor in neighbors)
        {
            if (IsPositionOccupied(neighbor))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get the occupied neighbour hex positions
    /// </summary>
    public Hex[] GetOccupiedNeighbours(Hex hex)
    {
        return HexUtils.GetAllNeighbours(hex).Where(IsPositionOccupied).ToArray();
    }

    /// <summary>
    /// Returns true when there is already a hex tile at that position
    /// </summary>
    public bool IsPositionOccupied(Hex hexPosition)
    {
        //Debug.Log($"Is {hexPosition} occupied = {tilesByHex.ContainsKey(hexPosition)}");
        return tilesByHex.ContainsKey(hexPosition);
    }

    /// <summary>
    /// Returns true when the edges of this tile match all neighbour edges
    /// </summary>
    public bool TileMatchesNeighbours(Hex hex, Tile tile)
    {
        foreach (var neighborHex in HexUtils.GetAllNeighbours(hex))
        {
            if (tilesByHex.TryGetValue(neighborHex, out var neighborTile))
            {
                int sharedEdge = HexUtils.GetSharedEdgeIndex(hex, neighborHex, tile.CurrentRotation);
                //Debug.Log($"Shared Edge: {sharedEdge} = {tile.Edges[sharedEdge]}");

                if (!tile.Matches(neighborTile, sharedEdge)) return false;
            }
        }
        return true;
    }

    public bool CanBePlaced(Hex hexPosition, Tile tile)
    {
        return
            !IsPositionOccupied(hexPosition)
            & HasNeighbours(hexPosition)
            & TileMatchesNeighbours(hexPosition, tile);
    }

    public void ResetAll()
    {
        tilesByHex.Clear();

        StartCoroutine(DeleteTilesRoutine());
    }

    private IEnumerator DeleteTilesRoutine()
    {
        foreach (Transform c in transform)
        {
            yield return new WaitForSeconds(.1f);
            Destroy(c.gameObject);
        }
    }
}
