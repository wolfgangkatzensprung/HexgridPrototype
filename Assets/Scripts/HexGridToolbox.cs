using UnityEngine;
using System.Collections.Generic;
using PrimeTween;
using Unity.VisualScripting;

public class HexGridToolbox : MonoBehaviour
{
    [Inspectable]
    public HexGrid hexGrid;

    public Hex[] GetNeighbours(Hex hex)
    {
        var neighbors = HexUtils.GetAllNeighbours(hex);
        return neighbors;
    }

    public void DropTile(Tile tile)
    {
        PrimeTween.Sequence.Create()
            .Chain(Tween.PositionY(tile.transform, endValue: tile.transform.position.y + 10, duration: 1, ease: Ease.InOutSine))
            .Chain(Tween.PositionY(tile.transform, endValue: tile.transform.position.y - 100, duration: 1, ease: Ease.InOutSine));
    }

    public List<Hex> GenerateHexPattern(Hex center, int radius)
    {
        List<Hex> pattern = new List<Hex>();
        for (int q = -radius; q <= radius; q++)
        {
            for (int r = Mathf.Max(-radius, -q - radius); r <= Mathf.Min(radius, -q + radius); r++)
            {
                pattern.Add(new Hex(center.Q + q, center.R + r));
            }
        }
        return pattern;
    }

    public List<Hex> GenerateSpiralPattern(Hex center, int layers)
    {
        var spiral = new List<Hex> { center };
        Hex current = center;
        var dir = HexUtils.Directions;

        for (int layer = 1; layer <= layers; layer++)
        {
            current = new Hex(center.Q + dir[4].Q * layer, center.R + dir[4].R * layer);
            for (int direction = 0; direction < 6; direction++)
            {
                for (int step = 0; step < layer; step++)
                {
                    spiral.Add(current);
                    current = new Hex(current.Q + dir[direction].Q, current.R + dir[direction].R);
                }
            }
        }
        return spiral;
    }

    public List<Hex> GenerateSkewedSpiralPattern(Hex center, int layers, float skewFactor)
    {
        List<Hex> spiral = new List<Hex> { center };
        Hex current = center;
        int[] directionsQ = { 1, 1, 0, -1, -1, 0 }; // q-coordinates for each direction (NE, E, SE, SW, W, NW)
        int[] directionsR = { 0, 1, 1, 0, -1, -1 }; // r-coordinates for each direction (NE, E, SE, SW, W, NW)

        for (int layer = 1; layer <= layers; layer++)
        {
            for (int direction = 0; direction < 6; direction++)
            {
                int steps = Mathf.RoundToInt((direction == 0 || direction == 3)
                                            ? (layer * skewFactor)  // Skew factor for NE and SW
                                            : (layer - 1) * skewFactor);  // Skew factor for other directions

                for (int step = 0; step < steps; step++)
                {
                    current = new Hex(current.Q + directionsQ[direction], current.R + directionsR[direction]);
                    spiral.Add(current);
                }
            }
        }

        return spiral;
    }


    public List<Hex> GenerateCoolConePattern(Hex center, int layers)
    {
        List<Hex> spiral = new List<Hex> { center };
        Hex current = center;
        int[] directionsQ = { 1, 0, -1, -1, 0, 1 };
        int[] directionsR = { 0, 1, 1, 0, -1, -1 };

        for (int layer = 1; layer <= layers; layer++)
        {
            for (int direction = 0; direction < 6; direction++)
            {
                for (int step = 0; step < layer; step++)
                {
                    spiral.Add(current);
                    current = new Hex(current.Q + directionsQ[direction], current.R + directionsR[direction]);
                }
            }
        }
        return spiral;
    }


    public void RotateGrid(float angle)
    {
        hexGrid.transform.Rotate(Vector3.up, angle);
    }

    public void ScaleGrid(float scaleFactor)
    {
        hexGrid.transform.localScale *= scaleFactor;
    }

    public void HighlightTile(Hex position, Color color)
    {
        Tile tile = hexGrid.GetTile(position);
        if (tile != null)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
    }

    public void BounceTile(Tile tile)
    {
        PrimeTween.Sequence.Create()
            .Chain(Tween.PositionY(tile.transform, tile.transform.position.y + 0.5f, 0.3f, Ease.OutQuad))
            .Chain(Tween.PositionY(tile.transform, tile.transform.position.y - 0.5f, 0.3f, Ease.InQuad));
    }

    public void ShatterGrid()
    {
        foreach (Tile tile in hexGrid.GetAllTiles())
        {
            PrimeTween.Sequence.Create()
                .Chain(Tween.Position(tile.transform, tile.transform.position + Random.insideUnitSphere * 2, 0.5f, Ease.OutQuad));
        }
    }

    public List<Hex> GetTilesInRange(Hex center, int range)
    {
        List<Hex> tilesInRange = new List<Hex>();
        List<Hex> pattern = GenerateHexPattern(center, range);
        foreach (Hex hex in pattern)
        {
            tilesInRange.Add(hex);
        }
        return tilesInRange;
    }

    public void MoveTileToNewPosition(Tile tile, Hex newPosition)
    {
        Vector3 newWorldPosition = hexGrid.HexToWorld(newPosition);
        PrimeTween.Sequence.Create()
            .Chain(Tween.Position(tile.transform, newWorldPosition, 0.5f, Ease.InOutQuad));
    }

    public Hex WorldToHex(Vector3 pos) => hexGrid.WorldToHex(pos);
    public Vector3 HexToWorld(Hex pos) => hexGrid.HexToWorld(pos);

    public GameObject GetRandomTilePrefab()
    {
        var tileTray = FindAnyObjectByType<TileTray>();
        return tileTray.RndTilePrefab;
    }
}
